## Libpalaso Localization

### Using localizations in a project

We are using .xlf with Crowdin so if you are using L10nSharp with TMX you will need to switch to XLF to make use of the Crowdin translations.

1. Add a Nuget dependency on libpalaso.l10ns to the project where you initialize the L10nSharp `LocalizationManager`
2. Add a build step to copy the Palaso.%langcode%.xlf files to the correct folder in your project

### Updating Crowdin with source string changes - UPLOAD TO CROWDIN NOT YET ENABLED

All the strings that are internationalized in all of the libpalaso projects are uploaded to Crowdin in Palaso.en.xlf

The L10nSharp tool ExtractXliff is run on the project to get any updates to the source strings resulting in a new Palaso.en.xlf file.

Overcrowdin is used to upload this file to Crowdin. * NOT YET *

This process is run automatically by a GitHub action if the commit comment mentions any of 'localize, l10n, i18n, internationalize, spelling' * NOT YET *

It can also be run manually as follows:
```
dotnet tool install -g overcrowdin
set CROWDIN_COMMONLIB_KEY=TheApiKeyForTheSilCommonLibrariesProject
msbuild l10n.proj /t:UpdateCrowdin
```

### Building Nuget package with the latest translations

Overcrowdin is used to build and download the latest translation data.

The resulting file is unzipped and a Nuget package is built from the l10ns.nuspec file

This process is run whenever a tag is pushed to the libpalaso repository.

It can also be run manually as follows:
```
dotnet tool install -g overcrowdin
set CROWDIN_COMMONLIB_KEY=TheApiKeyForTheSilCommonLibrariesProject
msbuild l10n.proj /t:PackageL10ns
nuget push -ApiKey TheSilNugetApiKey SIL.libpalaso.l10n.nupkg
```