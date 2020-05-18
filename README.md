Palaso Library
==============

The Palaso repo is a collection of shared libraries that are used in SIL .NET applications. The
libraries are mostly cross-platform compatible. All non-test assemblies have a corresponding nuget
package.

The Palaso library adheres to [Semantic Versioning](http://semver.org/) and
[keeps a Changelog](http://keepachangelog.com/) to record noteworthy changes.

Documentation
-------------

- Overview over the [assemblies](https://github.com/sillsdev/libpalaso/wiki/Assemblies)
- See [wiki](https://github.com/sillsdev/libpalaso/wiki) for documentation

Binaries
--------

[Binary builds](http://build.palaso.org/repository/downloadAll/bt32/.lastSuccessful/artifacts.zip) of all the Palaso libraries are available from our [Team City continuous build server](http://build.palaso.org/).

Source Code
-----------

To get the source code, you'll need Git. Then from a command line, give this command:

`git clone https://github.com/sillsdev/libpalaso`

Development
-----------

For instructions on building and contributing, see [Development](https://github.com/sillsdev/libpalaso/wiki/Development).

API Policy
----------

The palaso library follows semantic versioning. For APIs this means:

 * public APIs that become deprecated will be marked as obsolete.
 * The API and its obsolete tag will remain through the beta versions and at least one more stable version.
 * The API will be removed in one of the next stable versions.
