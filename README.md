Palaso Library
==============

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

The palaso library is still under active development the current deprecation policy is:

- public API's that become deprecated will be marked as obsolete, with an explanation in the default branch.
- The API and its obsolete tag will remain through the beta.
- The API will be removed when the library moves to stable.
