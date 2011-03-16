using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Palaso.Tests.WritingSystems.Migration.WritingSystemsLdmlV0To1Migration
{
	public class LdmlFileContentForTests
	{
		static public string Version0LdmlFile
		{
			get { return CreateVersion0LdmlContent("en", String.Empty,String.Empty, String.Empty); }
		}

		static public string Version1LdmlFile
		{
			get { return CreateVersion1LdmlContent("en", String.Empty, String.Empty, String.Empty); }
		}

		static public string CreateVersion0LdmlContent(string language, string script, string region, string variant)
		{
			return String.Format(
@"<?xml version='1.0' encoding='utf-8'?>
<ldml>
<identity>
	<version number='' />
	<generation date='0001-01-01T00:00:00' />
	<language type='{0}' />
	<script type='{1}' />
	<territory type='{2}' />
	<variant type='{3}' />
</identity>
<collations />
<special xmlns:palaso='urn://palaso.org/ldmlExtensions/v1'>
	<palaso:defaultFontFamily value='Arial' />
	<palaso:defaultFontSize value='12' />
</special>
</ldml>".Replace('\'', '"'), language, script, region, variant);
		}

		static public string CreateVersion0LdmlContentWithAllSortsOfDatathatdoesNotNeedSpecialAttention(string language, string script, string region, string variant)
		{
			return String.Format(
@"<?xml version='1.0' encoding='utf-8'?>
<ldml>
<identity>
	<version number='' />
	<generation date='0001-01-01T00:00:00' />
	<language type='{0}' />
	<script type='{1}' />
	<territory type='{2}' />
	<variant type='{3}' />
</identity>
<layout>
	<orientation characters='left-to-right'/>
</layout>
<collation>
	<base>
		<alias source=''/>
	</base>
</collation>
<special xmlns:palaso='urn://palaso.org/ldmlExtensions/v1'>
	<palaso:defaultFontFamily value='Arial' />
	<palaso:defaultFontSize value='12' />
	<palaso:abbreviation value='la' />
	<palaso:isLegacyEncoded value='true' />
	<palaso:defaultKeyboard value='bogusKeyboard' />
	<palaso:languageName value='language' />
	<palaso:sortRulesType value='OtherLanguage' />
</special>
</ldml>".Replace('\'', '"'), language, script, region, variant);
		}

		static private string CreateVersion1LdmlContent(string language, string script, string region, string variant)
		{
			return
String.Format(@"<?xml version='1.0' encoding='utf-8'?>
<ldml>
<identity>
	<version number='' />
	<generation date='0001-01-01T00:00:00' />
	<language type='{0}' />
	<script type='{1}' />
	<territory type='{2}' />
	<variant type='{3}' />
</identity>
<collations />
<special xmlns:palaso='urn://palaso.org/ldmlExtensions/v1'>
	<palaso:version value='1' />
	<palaso:defaultFontFamily value='Arial' />
	<palaso:defaultFontSize value='12' />
</special>
</ldml>".Replace('\'', '"'), language, script, region, variant);
		}

		static public string CreateVersion0LdmlContentWithLdmlInfoWeDontCareAbout(string language, string script, string region, string variant)
		{
			return String.Format(
@"<?xml version='1.0' encoding='utf-8'?>
<ldml>
<identity>
	<version number='' />
	<generation date='0001-01-01T00:00:00' />
	<language type='{0}' />
	<script type='{1}' />
	<territory type='{2}' />
	<variant type='{3}' />
</identity>
<fallback><testing>fallback</testing></fallback>
<localeDisplayNames><testing>localeDisplayNames</testing></localeDisplayNames>
<layout><testing>layout</testing></layout>
<characters><testing>characters</testing></characters>
<delimiters><testing>delimiters</testing></delimiters>
<measurement><testing>measurement</testing></measurement>
<dates><testing>dates</testing></dates>
<numbers><testing>numbers</testing></numbers>
<units><testing>units</testing></units>
<listPatterns><testing>listPatterns</testing></listPatterns>
<collations />
<posix><testing>posix</testing></posix>
<segmentations><testing>segmentations</testing></segmentations>
<rbnf><testing>rbnf</testing></rbnf>
<references><testing>references</testing></references>
<special xmlns:palaso='urn://palaso.org/ldmlExtensions/v1'>
	<palaso:defaultFontFamily value='Arial' />
	<palaso:defaultFontSize value='12' />
</special>
</ldml>".Replace('\'', '"'), language, script, region, variant);
		}
	}
}
