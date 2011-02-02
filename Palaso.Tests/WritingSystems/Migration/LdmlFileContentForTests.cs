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

		static private string CreateVersion0LdmlContent(string language, string script, string region, string variant)
		{
			return
String.Format(@"<?xml version='1.0' encoding='utf-8'?>
<ldml>
<identity>
	<version number='' />
	<generation date='0001-01-01T00:00:00' />
	<language type='{0}' />
	<script type='{1}' />
	<region type='{2}' />
	<variant type='{3}' />
</identity>
<collations />
<special xmlns:palaso='urn://palaso.org/ldmlExtensions/v1'>
	<palaso:defaultFontFamily value='Arial' />
	<palaso:defaultFontSize value='12' />
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
	<region type='{2}' />
	<variant type='{3}' />
</identity>
<collations />
<special xmlns:palaso='urn://palaso.org/ldmlExtensions/v1'>
	<palaso:version>1</palaso:version>
	<palaso:defaultFontFamily value='Arial' />
	<palaso:defaultFontSize value='12' />
</special>
</ldml>".Replace('\'', '"'), language, script, region, variant);
		}
	}
}
