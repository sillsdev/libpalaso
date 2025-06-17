# SIL.WritingSystems Library

This library contains many classes that make working with writing systems and language tags easier

## SIL Locale Data Repository

Much of the writing system data that this library provides comes from the [SIL Locale Data repository (SLDR)](https://github.com/silnrsi/sldr?tab=readme-ov-file#sil-locale-data-repository-sldr)
To test with updated SLDR data from the staging area you can set an environment variable

`SLDR_USE_STAGING=true`

## Updating embedded writing system and language data

There is a github action that can be run to update the `langtags.json` and `ianaSubtagRegistry.txt` which are embedded in the library.
It will download the latest and, after a successful run of the WritingSystems tests, create a PR to update both files.

### langtags.json

The list of language tag identifiers is curated by the Writing Systems Technology group and provided in a `langtags.json` file.
This library is used as the final fallback in case of problems with the data served from https://ldml.api.sil.org/langtags.json

To manually update langtags.json to the latest follow the following steps:

1. Run the unit test suite by hand and note (or fix) any failures to ByHand and SkipOnTeamCity category tests
1. Replace `Resources\langtags.json` with the content from https://ldml.api.sil.org/langtags.json
1. Run the unit test suite by hand and fix any tests that relied on old langtags data
1. Commit the changes

### ianaSubtagRegistry.txt
To manually update ianaSubtagRegistry.txt to the latest, replace `Resources\ianaSubtagRegistry.txt` with
the content from https://www.iana.org/assignments/language-subtag-registry/language-subtag-registry

