## Libpalaso Localization

### Using localizations in a project

1. Add a Nuget dependency on libpalaso.l10ns to the project where you initialize the L10nSharp `LocalizationManager`
2. Add a build step to copy the Palaso.%langcode%.xlf files to the correct folder in your project

### Updating Crowdin with source string changes (automatic)

On each commit to `master`, a GitHub Action runs to
- Extract all internationalized strings from all libpalaso projects to `../DistFiles/Palaso.en.xlf`
- Upload Palaso.en.xlf to [Crowdin](https://crowdin.com/project/sil-common-libraries)

See `../.github/workflows/l10n-source.yml`

It can also be run manually as follows:
```
msbuild l10n.proj /t:UpdateCrowdin
crowdin upload sources -i CROWDIN_PROJECT_ID -T CROWDIN_ACCESS_TOKEN
```

### Building a NuGet package with the latest translations

This process is run by a github action whenever a version tag is pushed and manually as needed
(See `../.github/workflows/l10n-packaging.yml`)

It can also be run manually on a developer machine as follows:
```
crowdin download --all -i CROWDIN_PROJECT_ID -T CROWDIN_ACCESS_TOKEN
msbuild l10n.proj /t:PackageL10ns
nuget push -ApiKey TheSilNugetApiKey SIL.libpalaso.l10n.nupkg
```