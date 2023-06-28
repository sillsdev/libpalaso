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

- Building libpalaso requires .NET 5. You might want to
  install Visual Studio 2019 >= 16.8, or JetBrains Rider.

#### Ubuntu Linux

These libraries cannot currently be built on Linux releases later than Focal without some additional steps/dependencies (because dotnet-sdk-5.0 is no longer supported and does not have a package installer for more recent versions and ).
##### To build on Ubuntu 22.04 (Jammy), you have to:
- Use mono-devel from Focal
- Use dotnet-sdk-6.0
- To get unit tests to pass, you have to install libcanberra-gtk-module (libcanberra-gtk-module/jammy,now 0.30-10ubuntu1 amd64). Not sure what is the correct way to get that dependency installed.

##### The rest of the stuff:
- Add access to packages.microsoft.com repo for dotnet sdk:

  ```bash
  cd $(mktemp -d) &&
  curl https://packages.microsoft.com/keys/microsoft.asc | gpg --dearmor > microsoft.gpg &&
  sudo mv microsoft.gpg /etc/apt/trusted.gpg.d/ &&
  (source /etc/os-release && wget -q https://packages.microsoft.com/config/${ID}/${VERSION_ID}/prod.list -O prod.list) &&
  sudo mv prod.list /etc/apt/sources.list.d/microsoft-prod.list &&
  sudo chown root:root /etc/apt/trusted.gpg.d/microsoft.gpg /etc/apt/sources.list.d/microsoft-prod.list &&
  sudo chmod 644 /etc/apt/trusted.gpg.d/microsoft.gpg /etc/apt/sources.list.d/microsoft-prod.list
  ```

- Add access to download.mono-project.com for mono 6 by following instructions at <https://www.mono-project.com/download/stable>.

- Install package dependencies:

  ```bash
  sudo apt update
  sudo apt install libicu-dev dotnet-sdk-5.0 mono-complete
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
  Or use the commandline. `environ` is not needed if you installed mono-complete version 6.

  ```bash
  build/build
  ```

- Verify that there were no new unit test failures:

  #### Windows

  ```bash
  build\TestBuild Debug Test
  ```

  #### Linux

  ```bash
  build/TestBuild Debug Test
  ```

- Test in client projects (as applicable):

  * Set an enviroment variable `LOCAL_NUGET_REPO` with the path to a folder on your computer (or local network) to publish locally-built packages
  * See [these instructions](https://docs.microsoft.com/en-us/nuget/hosting-packages/local-feeds) to enable local package sources
  * `build /t:pack` will pack nuget packages and publish them to `LOCAL_NUGET_REPO`

Further instructions at https://github.com/sillsdev/libpalaso/wiki/Developing-with-locally-modified-nuget-packages

### Contribute

- Commit. Push:

  ```bash
  $ git push origin TOPICBRANCHNAME
  ```

- Send a pull request (<https://help.github.com/articles/using-pull-requests>). Specify destination branch if not `master`.

## API Policy

The palaso library follows semantic versioning. For APIs this means:

- public APIs that become deprecated will be marked as obsolete.
- The API and its obsolete tag will remain through the beta versions and at least one more stable version.
- The API will be removed in one of the next stable versions.
