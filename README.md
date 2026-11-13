# Palaso Library

The Palaso repo is a collection of shared libraries that are used in SIL .NET applications. The
libraries are mostly cross-platform compatible. All non-test assemblies have a corresponding nuget
package.

The Palaso library adheres to [Semantic Versioning](http://semver.org/) and
[keeps a Changelog](http://keepachangelog.com/) to record noteworthy changes.

## Documentation

- Overview over the [assemblies](https://github.com/sillsdev/libpalaso/wiki/Assemblies)
- See [wiki](https://github.com/sillsdev/libpalaso/wiki) for documentation

## Binaries

Every commit creates a nuget package that is available on [nuget.org](https://www.nuget.org).

## Source Code

To get the source code, you'll need Git. Then from a command line, give this command:

`git clone https://github.com/sillsdev/libpalaso`

## Development

### Dependencies

#### Windows

- Building libpalaso requires the .NET 9 SDK or later (the code uses C# 13).
  You might want to install Visual Studio 2022 >= 17.12, or JetBrains Rider >= 2024.3.

#### Ubuntu Linux

On Ubuntu 24.04 (Noble), install the .NET 10 SDK, plus the .NET 8 runtime for
running the `net8.0` unit tests:

```bash
sudo apt update
sudo apt install dotnet-sdk-10.0 dotnet-runtime-8.0 libicu-dev
```

Alternatively, install .NET by following
<https://learn.microsoft.com/dotnet/core/install/linux-ubuntu>.

Running the .NET Framework (`net462`/`net48`) unit tests additionally needs Mono:

```bash
sudo apt install mono-complete libcanberra-gtk-module
```

### Develop

- Create a local topic branch:

  ```bash
  git fetch
  git checkout -b my-work origin/master
  ```

- Build:

  #### Windows

  Open `Palaso.sln` in Visual Studio and build.

  #### Linux

  Open and build `Palaso.sln` in JetBrains Rider.
  Or use the command line. `EnableWindowsTargeting` lets the `net8.0-windows`
  projects build on Linux.

  ```bash
  dotnet build Palaso.sln -p:EnableWindowsTargeting=true
  ```

- Verify that there were no new unit test failures:

  #### Windows

  ```bash
  build\TestBuild Debug Test
  ```

  #### Linux

  Run the `net8.0` tests one test project at a time (the WinForms test projects
  have no `net8.0` target), with the same filter CI uses:

  ```bash
  dotnet test SIL.Core.Tests -f net8.0 -p:EnableWindowsTargeting=true \
    --filter "(TestCategory != SkipOnTeamCity) & (TestCategory != RequiresAudioInputDevice)"
  ```

  Some tests already fail on Linux (see #1565), so compare against a run on `master`.

- Test in client projects (as applicable):

  * Set an environment variable `LOCAL_NUGET_REPO` with the path to a folder on your computer (or local network) to publish locally-built packages
  * See [these instructions](https://docs.microsoft.com/en-us/nuget/hosting-packages/local-feeds) to enable local package sources
  * `build /t:pack` will pack nuget packages and publish them to `LOCAL_NUGET_REPO`

Further instructions at https://github.com/sillsdev/libpalaso/wiki/Developing-with-locally-modified-nuget-packages

- Updating langtags.json and ianaSubtagRegistry.txt:
  * Instructions for updating the langtags.json and ianaSubtagRegistry.txt files are in [SIL.WritingSystems/Readme.md](SIL.WritingSystems/Readme.md)


### Contribute

- Commit. Push:

  ```bash
  $ git push origin TOPICBRANCHNAME
  ```

- Send a pull request (<https://help.github.com/articles/using-pull-requests>). Specify destination branch if not `master`.

### Localization

Palaso is localized with [L10NSharp](https://github.com/sillsdev/l10nsharp). Palaso-specific documentation is in [l10n/README.md](l10n/README.md).

### Semantic versioning

These libraries use [GitVersion](https://gitversion.net/). For simplicity, all library versions advance in lockstep.

- When a commit introduces a breaking change, include `+semver:major` at the start of the commit message.

- For API additions or other backward-compatible enhancements, use `+semver:minor`.

### Deployment on nuget.org

Beta builds are published automatically to [nuget.org](https://www.nuget.org/) by the `build-test` GitHub Action.

To publish an official release version:

1. Push a Git tag in the format `v10.5.1` (matching the current beta version calculated by GitVersion).

2. Manually run the `build-test` workflow in GitHub Actions.

## API Policy

The palaso library follows semantic versioning. For APIs this means:

- public APIs that become deprecated will be marked as obsolete.
- The API and its obsolete tag will remain through the beta versions and at least one more stable version.
- The API will be removed in one of the next stable versions.
