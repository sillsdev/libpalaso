External Dependencies
---------------------
The palaso library has dependencies on external packages as follows:


- libicu42 (or libicu44 or libicu48 depending which is the default in the distribution)
  * For the ICU collation library

- libicu-cil
  * For the C# bindings to the ICU library

API Policy
---------------------
The palaso library is still under active development the current deprecation policy is:

- public API's that become deprecated will be marked as obsolete, with an explanation in the default branch.
- The API and its obsolete tag will remain through the beta.
- The API will be removed when the library moves to stable.
