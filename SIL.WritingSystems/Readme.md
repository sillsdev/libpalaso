## SIL.WritingSystems Library

This library contains many classes that make working with writing systems and language tags easier

### Updating langtags.json

To update langtags.json to the latest follow the following steps:

1. Run the unit test suite by hand and note (or fix) any failures to ByHand and SkipOnTeamCity category tests
1. Replace `Resources\langtags.json` with the content from https://ldml.api.sil.org/langtags.json
1. Run the unit test suite by hand and fix any tests that relied on old langtags data
1. Commit the changes