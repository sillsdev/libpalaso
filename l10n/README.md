## Libpalaso Localization

### Using localizations in a project

1. Add a Nuget dependency on SIL.libpalaso.l10ns to the project where you initialize the L10nSharp `LocalizationManager`
2. Add a build step to copy the Palaso.%langcode%.xlf files to the correct folder in your project

### Updating Crowdin with source string changes (automatic)

On each commit to `master`, a GitHub Action runs to
- Download the current `Palaso.en.xlf` from Crowdin.
  (L10NSharp.ExtractXliff version 7.0.0-beta0011 fails to extract all strings, as not all are internationalized.
  Merging into the existing file is easier than fixing 128 uninternationalized strings.)
- Extract all internationalized strings from all libpalaso projects to `Palaso.en.xlf`
- Upload Palaso.en.xlf to [Crowdin](https://crowdin.com/project/sil-common-libraries)

See `../.github/workflows/l10n-source.yml`

It can also be run manually as follows (requires the [Crowdin CLI](https://crowdin.github.io/crowdin-cli/)):
```
crowdin download sources -T CROWDIN_ACCESS_TOKEN
msbuild l10n.proj /t:UpdateCrowdin
crowdin upload sources -T CROWDIN_ACCESS_TOKEN
```

### Building a NuGet package with the latest translations

This process is run by a github action whenever a version tag is pushed and manually as needed
(See `../.github/workflows/l10n-packaging.yml`)

It can also be run manually as follows (requires the [Crowdin CLI](https://crowdin.github.io/crowdin-cli/)):
```
crowdin download sources -T CROWDIN_ACCESS_TOKEN
crowdin download --all -T CROWDIN_ACCESS_TOKEN
msbuild l10n.proj /t:PackageL10ns
nuget push -ApiKey TheSilNugetApiKey SIL.libpalaso.l10n.nupkg
```