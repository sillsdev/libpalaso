## Libpalaso Localization

### Using localizations in a project

We are using .xlf with Crowdin so if you are using L10nSharp with TMX you will need to switch to XLF to make use of the Crowdin translations.

1. Add a Nuget dependency on libpalaso.l10ns to the project where you initialize the L10nSharp `LocalizationManager`
2. Add a build step to copy the Palaso.%langcode%.xlf files to the correct folder in your project

### Updating Crowdin with source string changes - UPLOAD TO CROWDIN NOT YET ENABLED

All the strings that are internationalized in all of the libpalaso projects are uploaded to Crowdin in Palaso.en.xlf

A Github action runs when commits are merged into master which uses the L10nSharp tool ExtractXliff to get any updates to the source strings resulting in a new Palaso.en.xlf file.

Then the Crowdin cli is used to update that file in Crowdin based on the crowdin.yml file.

It can also be run manually as follows:
```
msbuild l10n.proj /t:UpdateCrowdin
crowdin upload sources -i CROWDIN_PROJECT_ID -T CROWDIN_ACCESS_TOKEN
```

### Building Nuget package with the latest translations
This process is run manually from a github action whenever a package with updated translations is needed

It can also be run manually on a developer machine as follows:
```
crowdin download --all -i CROWDIN_PROJECT_ID -T CROWDIN_ACCESS_TOKEN
msbuild l10n.proj /t:PackageL10ns
nuget push -ApiKey TheSilNugetApiKey SIL.libpalaso.l10n.nupkg
```