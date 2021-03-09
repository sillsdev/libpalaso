Palaso Library (6.0)
==============

## This branch is a maintenance branch for projects that are not able to consume the Nuget packages that are shipped from master. 

Documentation
-------------

- Notes on [Re-Distribution](http://projects.palaso.org/projects/palaso/wiki/Re-Distribution)
- [Error Reporting](http://projects.palaso.org/projects/palaso/wiki/Error_Reporting)
- [Writing Systems](http://projects.palaso.org/projects/palaso/wiki/Writing_Systems)


Binaries
--------

[Binary builds](http://build.palaso.org/repository/downloadAll/bt32/.lastSuccessful/artifacts.zip) of all the Palaso libraries are available from our [Team City continuous build server](http://build.palaso.org/).


Source Code
-----------

To get the source code, you'll need Git. Then from a command line, give this command:

`git clone https://github.com/sillsdev/libpalaso`


Development
-----------

For instructions on building and contributing, see <https://github.com/sillsdev/libpalaso/wiki/Development>.

API Policy
----------

The palaso library follows semantic versioning. For APIs this means:

 * public APIs that become deprecated will be marked as obsolete.
 * The API and its obsolete tag will remain through the beta versions and at least one more stable version.
 * The API will be removed in one of the next stable versions.
