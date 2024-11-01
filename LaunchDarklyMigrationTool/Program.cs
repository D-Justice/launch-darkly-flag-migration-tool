using AutoMapper;
using LaunchDarklyMigrationTool.Models.Flags;
using LaunchDarklyMigrationTool.Models.Pipelines;
using LaunchDarklyMigrationTool.Models.Project;
using Newtonsoft.Json;
using System.CommandLine;
using System.Data;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using Environment = LaunchDarklyMigrationTool.Models.Project.Environment;
using Item = LaunchDarklyMigrationTool.Models.Project.Item;

internal class Program
{
    private static readonly HttpClient readClient = new()
    {
        BaseAddress = new Uri("https://app.launchdarkly.com/api")
    };

    private static readonly HttpClient writeClient = new()
    {
        BaseAddress = new Uri("https://app.launchdarkly.com/api")
    };

    private static async Task<int> Main(string[] args)
    {
        var mapper = InitializeAutoMapper();
        var writeKey = new Option<string>(
            "--writeKey",
            "[Required] Launch Darkly WRITE api key");
        var readKey = new Option<string>(
            "--readKey",
            "[Required] Launch Darkly READ api key");
        var projectKey = new Option<string>(
            "--projectKey",
            "[Required] Existing Project key");
        var newProjectKey = new Option<string>(
            "--newProjectKey",
            "[Required] Name of project key that will be created");
        var newProjectName = new Option<string>(
            "--newProjectName",
            "[Required] Name of project that will be created");
        var tagValues = new Option<string>(
            "--tags",
            "[Required] Get specific flags that have this tag (to do multiple tags, seperate them with a ',' - eg. mobile-api,mobile-app)");
        var flagKeyValueToRemove = new Option<string?>(
            "--flagKeyValueToRemove",
            "[Optional] Remove specific value from all flag's key. eg. '--flagKeyValueToRemove mobile-api' will remove 'mobile-api' from all flags (mobile-api-search => search). Do not add the extra '-' on the end as the tool will already take care of that");

        var rootCommand = new RootCommand("Launch Darkly key migration tool");
        rootCommand.AddOption(writeKey);
        rootCommand.AddOption(readKey);
        rootCommand.AddOption(projectKey);
        rootCommand.AddOption(newProjectKey);
        rootCommand.AddOption(newProjectName);
        rootCommand.AddOption(tagValues);
        rootCommand.AddOption(flagKeyValueToRemove);
        rootCommand.SetHandler(async (write, read, existingKey, newKey, newName, tags, flagKeyRemovalValue) =>
            {
                readClient.DefaultRequestHeaders.Add("Authorization", read);
                writeClient.DefaultRequestHeaders.Add("Authorization", write);

                var environments = await CreateProject(newKey, newName, existingKey, mapper);
                if (environments.Items == null)
                    return;
                await CreateReleasePipelineConfig(existingKey, mapper, newKey);
                string[] tagsAsList = tags.Split(",");
                foreach (string tagValue in tagsAsList)
                {
                    await CreateFeatureFlags(existingKey, mapper, newKey, tagValue, environments, flagKeyRemovalValue);
                }

                Console.WriteLine("Migration successful!");
            },
            writeKey, readKey, projectKey, newProjectKey, newProjectName, tagValues, flagKeyValueToRemove);


        return await rootCommand.InvokeAsync(args);
    }

    private static async Task CreateFeatureFlags(string projectKey, IMapper mapper, string newProjectKey, string tag,
        Environment environments, string flagKeyRemovalValue)
    {
        var moreToRetrieve = true;
        int offset = 0;
        while (moreToRetrieve)
        {
            var items = environments.Items;
            var sample = new List<List<Item>>();
            const int batchSize = 3;
            for (var i = 0; i < items.Count; i += batchSize)
            {
                var count = Math.Min(batchSize, items.Count - i);
                sample.Add(items.GetRange(i, count));
            }

            foreach (var environmentBatch in sample)
            {
                var envQueryParams = string.Join("&env=", environmentBatch.Select(x => x.Key).ToArray());

                var response = await GetListOfFeatureFlagsByEnvironment(projectKey, tag, envQueryParams, offset);
                if (response.TotalCount < 100) moreToRetrieve = false;
                foreach (var flagRequest in response.Flags)
                {
                    var existingFlag = await GetIndividualFlagInformation(projectKey, flagRequest.Key);
                    if (!string.IsNullOrEmpty(flagKeyRemovalValue))
                    {
                        flagRequest.Key = flagRequest.Key.Replace($"{flagKeyRemovalValue}-", "");
                    }
                    var variations =
                        await CreateFeatureFlag(flagRequest, mapper, newProjectKey);
                    flagRequest.ExistingFlagInformation = existingFlag;
                    flagRequest.Variations = variations;

                    await UpdateFeatureFlag(newProjectKey, flagRequest, flagKeyRemovalValue);
                }
            }
            offset += 100;
        }
    }

    private static async Task<GetFlagListResponse> GetListOfFeatureFlagsByEnvironment(string projectKey, string tag,
        string envQueryParams, int offset)
    {
        var getFeatureFlagList =
            await readClient.GetAsync($"/api/v2/flags/{projectKey}?limit=100&tag={tag}&offset={offset}&env={envQueryParams}");
        var jsons = await getFeatureFlagList.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<GetFlagListResponse>(jsons);
    }

    private static async Task<List<Variation>> CreateFeatureFlag(Flag flagRequest, IMapper mapper,
        string newProjectKey)
    {
        var request = mapper.Map<CreateFlagRequest>(flagRequest);
        var json = JsonConvert.SerializeObject(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var writeResponse = await writeClient.PostAsync($"/api/v2/flags/{newProjectKey}", content);
        await HandleRateLimiting(writeResponse);
        if (!writeResponse.IsSuccessStatusCode)
        {
            if (writeResponse.StatusCode == HttpStatusCode.Conflict)
            {
                Console.WriteLine($"Flag already exists - getting flag information: {flagRequest.Key}");
                var individualFlag = await GetIndividualFlagInformation(newProjectKey, flagRequest.Key);
                return individualFlag.Variations;
            }
            Console.WriteLine($"Failed to create flag {flagRequest.Key}");
        }
        else
        {
            Console.WriteLine($"Successfully created flag {flagRequest.Key}");

        }
        var jsonResponse = await writeResponse.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<CreateFlagResponse>(jsonResponse).Variations;
    }

    public static async Task UpdateFeatureFlag(string newProjectKey, Flag flagRequest, string flagKeyRemovalValue)
    {
        foreach (var environmentKey in flagRequest.EnvironmentsJson.Keys)
            if (flagRequest.EnvironmentsJson.TryGetValue(environmentKey, out var environmentValue))
            {
                if (flagRequest.ExistingFlagInformation.Environments.TryGetValue(environmentKey,
                        out var individualFlag))
                {
                    var newObject = new List<object>
                    {
                        new
                        {
                            op = "replace",
                            path = $"/environments/{environmentKey}/on",
                            value = environmentValue.on
                        },
                        new
                        {
                            op = "replace",
                            path = "/deprecated",
                            value = flagRequest.Deprecated
                        },
                        new
                        {
                            op = "replace",
                            path = "/archived",
                            value = flagRequest.Archived
                        }
                    };
                    if (individualFlag.rules.Any())
                    {
                    }
                    await UpdateFlagRules(individualFlag, flagRequest, environmentKey, newProjectKey, flagKeyRemovalValue);

                    await UpdateFlagValues(flagRequest, newObject, newProjectKey);
                }
                else
                {
                    Console.WriteLine($"Couldnt find individual flag info info for environment key {environmentKey}");
                }
            }
            else
            {
                Console.WriteLine($"Couldnt find environment info for environment key {environmentKey}");
            }
    }

    private static async Task UpdateFlagValues(Flag flagRequest, object newObject, string newProjectKey)
    {
        var json = JsonConvert.SerializeObject(newObject);
        var content =
            new StringContent(json, Encoding.UTF8, "application/json");
        var response = await writeClient.PatchAsync($"/api/v2/flags/{newProjectKey}/{flagRequest.Key}", content);
        await HandleRateLimiting(response);
        if (!response.IsSuccessStatusCode)
            Console.WriteLine($"Failed to update flag flag {flagRequest.Key}");
        else
            Console.WriteLine($"Successfully updated flag {flagRequest.Key}");
    }
    private static async Task UpdateFlagRules(IndividualFlagEnvironmentData individualFlag, Flag flagRequest, 
        string environmentKey, string newProjectKey,
    string flagKeyRemovalValue)
    {
        var updateFlagRulesRequest = new
        {
            environmentKey,
            instructions = new List<object>
            {
                
                new
                {
                    kind = "updateOffVariation",
                    variationId = flagRequest.Variations[individualFlag.OffVariation].Id
                },
                new
                {
                    kind = "updateFallthroughVariationOrRollout",
                    variationId = flagRequest.Variations[individualFlag.fallThrough.Variation].Id
                }
            }
        };
        foreach (var rule in individualFlag.rules)
        {
            updateFlagRulesRequest.instructions.Add(new
            {
                description = rule.Description,
                kind = "addRule",
                variationId = flagRequest.Variations[rule.Variation].Id,
                clauses = rule.Clauses
            });
        }

        foreach (var preRequisite in individualFlag.PreRequisites)
        {
            var strippedKey = preRequisite.Key.Replace($"{flagKeyRemovalValue}-", "");
            var preRequisiteFlag = await GetIndividualFlagInformation(newProjectKey, strippedKey);
            updateFlagRulesRequest.instructions.Add(new
            { 
                kind = "addPrerequisite",
                key = strippedKey,
                variationId = preRequisiteFlag.Variations[preRequisite.Variation].Id
            });
        }

        try
        {
            if (flagRequest.Variations.Any())
            {
                    
                var json = JsonConvert.SerializeObject(updateFlagRulesRequest);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Patch,
                    RequestUri = new Uri($"/api/v2/flags/{newProjectKey}/{flagRequest.Key}",
                        UriKind.Relative),
                    Content = content
                };

                request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json")
                {
                    Parameters =
                        { new NameValueHeaderValue("domain-model", "launchdarkly.semanticpatch") }
                };

                var response = await writeClient.SendAsync(request);
                await HandleRateLimiting(response);
                Console.WriteLine(response.IsSuccessStatusCode
                    ? $"Rule creation successful for flag: {flagRequest.Key}"
                    : $"Rule creation failed for flag: {flagRequest.Key}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
        
    }

    private static async Task HandleRateLimiting(HttpResponseMessage response)
    {
        var requestsRemaining = response.Headers.GetValues("X-Ratelimit-Route-Remaining").FirstOrDefault();
        var timeUntilRateLimitExpire = response.Headers.GetValues("X-Ratelimit-Reset").FirstOrDefault();
        if (int.Parse(requestsRemaining) <= 0)
        {
            var unixTimeMillis = long.Parse(timeUntilRateLimitExpire);

            var currentUnixTimeMillis = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            var timeToWaitMillis = unixTimeMillis - currentUnixTimeMillis;
            Console.WriteLine($"Waiting to avoid ratelimit: ~{timeToWaitMillis / 1000} seconds");

            await Task.Delay((int)timeToWaitMillis);
        }
    }

    private static async Task<IndividualFlag> GetIndividualFlagInformation(string projectKey, string flagKey)
    {
        var getFeatureFlagResponse = await readClient.GetAsync($"/api/v2/flags/{projectKey}/{flagKey}");
        var json = await getFeatureFlagResponse.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<IndividualFlag>(json);
    }

    private static async Task<Environment> CreateProject(string newProjectKey, string newName, string projectKey,
        IMapper mapper)
    {
        var getProjectResponse = await readClient.GetAsync($"/api/v2/projects/{projectKey}?expand=environments");
        var response =
            JsonConvert.DeserializeObject<GetProjectResponse>(await getProjectResponse.Content.ReadAsStringAsync());
        var request = mapper.Map<CreateProjectRequest>(response);
        request.Name = newName;
        request.Key = newProjectKey;
        Console.WriteLine($"Creating Project: {newProjectKey}");
        var json = JsonConvert.SerializeObject(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var createProjectResponse = await writeClient.PostAsync("/api/v2/projects", content);
        if (createProjectResponse.IsSuccessStatusCode)
        {
            Console.WriteLine("Successfully created new Project");
        }
        else
        {
            if (createProjectResponse.StatusCode == HttpStatusCode.Conflict)
            {
                Console.WriteLine("Project already exists");
            }
            else
            {
                Console.WriteLine($"Failed to create new project. Status code: {createProjectResponse.StatusCode}");
                return new Environment();
            }
        }
        return response.Environments;
    }

    private static async Task CreateReleasePipelineConfig(string projectKey, IMapper mapper, string newProjectKey)
    {
        var getReleasePipelinesResponse = await readClient.GetAsync($"/api/v2/projects/{projectKey}/release-pipelines");
        var response =
            JsonConvert.DeserializeObject<ReleasePipelineResponse>(await getReleasePipelinesResponse.Content
                .ReadAsStringAsync());
        var request = mapper.Map<ReleasePipelineRequestWrapper>(response);
        foreach (var releasePipelineRequest in request.ReleasePipelineRequests)
        {
            Console.WriteLine($"Creating Release Pipeline: {releasePipelineRequest.Key}");
            var json = JsonConvert.SerializeObject(releasePipelineRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var jsonResponse =
                await writeClient.PostAsync($"/api/v2/projects/{newProjectKey}/release-pipelines", content);
            if (jsonResponse.IsSuccessStatusCode)
            {
                Console.WriteLine("Successfully created new Release Pipeline");
            }
            else
            {
                Console.WriteLine(jsonResponse.StatusCode == HttpStatusCode.Conflict
                    ? "Release Pipeline already exists"
                    : "Failed to created new Release Pipeline");
            }
        }
    }

    private static IMapper InitializeAutoMapper()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Flag, CreateFlagRequest>()
                .ForMember(src => src.MaintainerTeamKey, src => src.MapFrom(opt => opt.MaintainerTeam.Key));

            cfg.CreateMap<GetProjectResponse, CreateProjectRequest>()
                .ForMember(dest => dest.Environments, src => src.MapFrom(x => x.Environments.Items))
                .ForMember(dest => dest.NamingConvention, src => src.MapFrom(x => x.NamingConvention));
            cfg.CreateMap<Item, EnvironmentRequest>();
            cfg.CreateMap<NamingConvention, NamingConventionRequest>();


            cfg.CreateMap<ReleasePipelineResponse, ReleasePipelineRequestWrapper>()
                .ForMember(dest => dest.ReleasePipelineRequests, opt => opt.MapFrom(src => src.Items));

            cfg.CreateMap<LaunchDarklyMigrationTool.Models.Pipelines.Item, ReleasePipelineRequest>()
                .ForMember(dest => dest.PhaseRequests, opt => opt.MapFrom(x => x.Phases));

            cfg.CreateMap<Phase, PhaseRequest>()
                .ForMember(dest => dest.AudienceRequests, opt => opt.MapFrom(src => src.Audiences))
                .ForMember(dest => dest.ConfigurationRequest, opt => opt.MapFrom(src => src.Configuration));

            cfg.CreateMap<Audience, AudienceRequest>()
                .ForMember(dest => dest.EnvironmentKey, opt => opt.MapFrom(src => src.Environment.Key))
                .ForMember(dest => dest.SegmentKeys, opt => opt.Ignore());

            cfg.CreateMap<Configuration, ConfigurationRequest>()
                .ForMember(dest => dest.ReleaseGuardianConfigurationRequest, opt => opt.Ignore())
                .ForMember(dest => dest.BakeTimeDurationMs, opt => opt.Ignore());
        });
        return config.CreateMapper();
    }
}