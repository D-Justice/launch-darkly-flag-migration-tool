# Launch Darkly Flag migration tool #

This tool is a dotnet console app that is made to migrate all of your specified flags from one project to a new project or one of your choosing.

### How to user LDFMT ###

* Start by going to https://app.launchdarkly.com/settings/authorization and create both a READ and a WRITE API token
* From here you can either clone the repository (if you plan on running the dotnet app locally) or you can just download the .EXE file from \LaunchDarklyMigrationTool\bin\Release\net8.0
## Running Dotnet App Locally: ##
* Clone the repository
* Navigate to the cloned repository
* In the terminal run > dotnet run --writeKey *your WRITE API key* --readKey *your READ API key* --projectKey *Launch Darkly project key that you will be copying the flags from* --newProjectKey *project key of new project or an existing project that you want the flags migrated to (this does not have to exist yet)* --newProjectName *name of your project - this can be anything* --tags *flag tags that you want to copy from one project to another, these should be seperated by a comma*
* Wait for it to finish running
## Running from the .exe file: ##
* Download the file called LaunchDarklyMigrationTool.exe
* Navigate to where it is downloaded in CMD or your choice of terminal
* Run > LaunchDarklyMigrationTool.exe --writeKey *your WRITE API key* --readKey *your READ API key* --projectKey *Launch Darkly project key that you will be copying the flags from* --newProjectKey *project key of new project or an existing project that you want the flags migrated to (this does not have to exist yet)* --newProjectName *name of your project - this can be anything* --tags *flag tags that you want to copy from one project to another, these should be seperated by a comma*

There is also an optional tag (--flagKeyValueToRemove) which will remove a specified value from each flag key. Eg. If I have a group of flags that follow the naming convention of "mobile-api-mocking" or "mobile-api-test-route" I could add --flagKeyValueToRemove mobile and these flags would be created in the project as "api-mocking" and "api-test-route".

You can also run --help if you get stuck

### What exactly does this tool do? ###

This tool will copy all flags, and release pipeline configurations from your target project to the new project. If the project doesn't exist it will be created. All flags should have all tags, rules, default settings, descriptions, names etc copied over from the existing project.

### Contribution guidelines ###

* Feel free to modify this code how you see fit
