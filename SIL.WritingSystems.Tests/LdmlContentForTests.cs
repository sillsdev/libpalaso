using System;
using SIL.WritingSystems.Migration.WritingSystemsLdmlV0To1Migration;

namespace SIL.WritingSystems.Tests
{
	public class LdmlContentForTests
	{
		public static string NoLdml = @"<?xml version='1.0' encoding='UTF-8' ?>
<noLdml>
</noLdml>
".Replace("'", "\"");

		public static string Version99Default()
		{
			return
				@"<?xml version='1.0' encoding='utf-8'?>
<ldml>
<identity>
	<version number='' />
	<generation date='0001-01-01T00:00:00' />
	<language type='en' />
</identity>
<collations />
<special xmlns:palaso='urn://palaso.org/ldmlExtensions/v1'>
	<palaso:version value='99' />
	<palaso:defaultFontFamily value='Arial' />
	<palaso:defaultFontSize value='12' />
</special>
</ldml>".Replace('\'', '"');
		}

		#region Version 0 Ldml

		public static string Version0WithLanguageSubtagAndName(string languageSubtag, string languageName)
		{
			return string.Format(
				@"<?xml version='1.0' encoding='utf-8'?>
<ldml>
<identity>
	<version number='' />
	<generation date='0001-01-01T00:00:00' />
	<language type='{0}' />
</identity>
<collations />
<special xmlns:palaso='urn://palaso.org/ldmlExtensions/v1'>
	<palaso:languageName value='{1}' />
	<palaso:defaultFontFamily value='Arial' />
	<palaso:defaultFontSize value='12' />
</special>
</ldml>".Replace('\'', '"'), languageSubtag, languageName);
		}

		public static string Version0WithAllSortsOfDatathatdoesNotNeedSpecialAttention(string language, string script, string region, string variant)
		{
			return string.Format(
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
<collations>
	<collation>
		<base>
			<alias source=''/>
		</base>
		<special xmlns:palaso='urn://palaso.org/ldmlExtensions/v1'>
			<palaso:sortRulesType value='OtherLanguage' />
		</special>
	</collation>
</collations>
<special xmlns:palaso='urn://palaso.org/ldmlExtensions/v1'>
	<palaso:abbreviation value='la' />
	<palaso:defaultFontFamily value='Arial' />
	<palaso:defaultFontSize value='12' />
	<palaso:defaultKeyboard value='bogusKeyboard' />
	<palaso:isLegacyEncoded value='true' />
	<palaso:languageName value='language' />
	<palaso:spellCheckingId value='ol' />
</special>
</ldml>".Replace('\'', '"'), language, script, region, variant);
		}

		internal static string Version0WithSystemCollationInfo()
		{
			return
				@"<?xml version='1.0' encoding='utf-8'?>
<ldml>
<identity>
	<version number='' />
	<generation date='0001-01-01T00:00:00' />
	<language type='de' />
</identity>
<layout>
	<orientation characters='left-to-right'/>
</layout>
<collations>
	<collation>
		<base>
			<alias source='en'/>
		</base>
		<special xmlns:palaso='urn://palaso.org/ldmlExtensions/v1'>
			<palaso:sortRulesType value='OtherLanguage' />
		</special>
	</collation>
</collations>
<special xmlns:palaso='urn://palaso.org/ldmlExtensions/v1'>
	<palaso:abbreviation value='la' />
	<palaso:defaultFontFamily value='Arial' />
	<palaso:defaultFontSize value='12' />
	<palaso:defaultKeyboard value='bogusKeyboard' />
	<palaso:isLegacyEncoded value='true' />
	<palaso:languageName value='language' />
	<palaso:spellCheckingId value='ol-GB-1996' />
</special>
</ldml>".Replace('\'', '"');
		}

		internal static string Version0WithBogusSystemCollationInfo()
		{
			return
				@"<?xml version='1.0' encoding='utf-8'?>
<ldml>
<identity>
	<version number='' />
	<generation date='0001-01-01T00:00:00' />
	<language type='mvp' />
</identity>
<layout>
	<orientation characters='left-to-right'/>
</layout>
<collations>
	<collation>
		<base>
			<alias source='mvp'/>
		</base>
		<special xmlns:palaso='urn://palaso.org/ldmlExtensions/v1'>
			<palaso:sortRulesType value='OtherLanguage' />
		</special>
	</collation>
</collations>
<special xmlns:palaso='urn://palaso.org/ldmlExtensions/v1'>
	<palaso:abbreviation value='la' />
	<palaso:defaultFontFamily value='Arial' />
	<palaso:defaultFontSize value='12' />
	<palaso:defaultKeyboard value='bogusKeyboard' />
	<palaso:isLegacyEncoded value='true' />
	<palaso:languageName value='language' />
	<palaso:spellCheckingId value='ol-GB-1996' />
</special>
</ldml>".Replace('\'', '"');
		}

		internal static string Version0WithCollationInfo(WritingSystemDefinitionV0.SortRulesType sortType)
		{
			string collationelement = GetCollationElementXml(sortType);

			return string.Format(
				@"<?xml version='1.0' encoding='utf-8'?>
<ldml>
<identity>
	<version number='' />
	<generation date='0001-01-01T00:00:00' />
	<language type='en' />
</identity>
<layout>
	<orientation characters='left-to-right'/>
</layout>
<collations>
	{0}
</collations>
<special xmlns:palaso='urn://palaso.org/ldmlExtensions/v1'>
	<palaso:abbreviation value='la' />
	<palaso:defaultFontFamily value='Arial' />
	<palaso:defaultFontSize value='12' />
	<palaso:defaultKeyboard value='bogusKeyboard' />
	<palaso:isLegacyEncoded value='true' />
	<palaso:languageName value='language' />
	<palaso:spellCheckingId value='ol-GB-1996' />
</special>
</ldml>".Replace('\'', '"'), collationelement);
		}

		public static string Version0WithLdmlInfoWeDontCareAbout(string language, string script, string region, string variant)
		{
			return string.Format(
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
<measurement><testing>measurement</testing></measurement>
<dates><testing>dates</testing></dates>
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

		private static string GetCollationElementXml(WritingSystemDefinitionV0.SortRulesType sortType)
		{
			string collationelement = string.Empty;
			switch (sortType)
			{
				case WritingSystemDefinitionV0.SortRulesType.DefaultOrdering:
					collationelement = string.Empty;
					break;
				case WritingSystemDefinitionV0.SortRulesType.CustomICU:
					collationelement =
						@"<collation>
	<base>
		<alias source='' />
	</base>
	<rules>
		<reset>ab</reset><s>q</s><t>Q</t><reset>ad</reset><t>AD</t><p>x</p><t>X</t>
	</rules>
	<special xmlns:palaso='urn://palaso.org/ldmlExtensions/v1'>
		<palaso:sortRulesType value='CustomICU' />
	</special>
</collation>";
					break;
				case WritingSystemDefinitionV0.SortRulesType.CustomSimple:
					collationelement =
						@"<collation>
	<base>
		<alias source='' />
	</base>
	<rules>
		<reset before='primary'><first_non_ignorable /></reset><p>a</p><s>A</s><p>b</p><s>B</s><p>o</p><s>O</s><p>m</p><s>M</s>
	</rules>
	<special xmlns:palaso='urn://palaso.org/ldmlExtensions/v1'>
		<palaso:sortRulesType value='CustomSimple' />
	</special>
</collation>";
					break;
				case WritingSystemDefinitionV0.SortRulesType.OtherLanguage:
					collationelement =
						@"<collation>
		<base>
			<alias source='de'/>
		</base>
		<special xmlns:palaso='urn://palaso.org/ldmlExtensions/v1'>
			<palaso:sortRulesType value='OtherLanguage' />
		</special>
	</collation>
";
					break;
			}
			return collationelement;
		}

		public static string Version0WithFw(string language, string script, string region, string variant)
		{
			return string.Format(
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
<special xmlns:palaso='urn://palaso.org/ldmlExtensions/v1'>
	<palaso:defaultFontFamily value='Arial' />
	<palaso:defaultFontSize value='12' />
</special>
<special xmlns:fw='urn://fieldworks.sil.org/ldmlExtensions/v1'>
		<fw:graphiteEnabled value='True' />
		<fw:validChars value='&lt;?xml version=&quot;1.0&quot; encoding=&quot;utf-16&quot;?&gt;
&lt;ValidCharacters&gt;
  &lt;WordForming&gt;α￼Α￼ά￼ὰ￼ᾷ￼ἀ￼Ἀ￼ἁ￼Ἁ￼ἄ￼Ἄ￼ἂ￼ἅ￼Ἅ￼ἃ￼Ἃ￼ᾶ￼ᾳ￼ᾴ￼ἆ￼Ἆ￼ᾄ￼ᾅ￼β￼Β￼γ￼Γ￼δ￼Δ￼ε￼Ε￼έ￼ὲ￼ἐ￼Ἐ￼ἑ￼Ἑ￼ἔ￼Ἔ￼ἕ￼Ἕ￼ἓ￼Ἓ￼ζ￼Ζ￼η￼Η￼ή￼ὴ￼ῇ￼ἠ￼Ἠ￼ἡ￼Ἡ￼ἤ￼Ἤ￼ἢ￼ἥ￼Ἥ￼Ἢ￼ἣ￼ᾗ￼ῆ￼ῃ￼ῄ￼ἦ￼Ἦ￼ᾖ￼ἧ￼ᾐ￼ᾑ￼ᾔ￼θ￼Θ￼ι￼ί￼ὶ￼ϊ￼ΐ￼ῒ￼ἰ￼Ἰ￼ἱ￼Ἱ￼ἴ￼Ἴ￼ἵ￼Ἵ￼ἳ￼ῖ￼ἶ￼ἷ￼κ￼Κ￼λ￼Λ￼μ￼Μ￼ν￼Ν￼ξ￼Ξ￼ο￼Ο￼ό￼ὸ￼ὀ￼Ὀ￼ὁ￼Ὁ￼ὄ￼Ὄ￼ὅ￼ὂ￼Ὅ￼ὃ￼Ὃ￼π￼Π￼ρ￼Ρ￼ῥ￼Ῥ￼σ￼ς￼Σ￼τ￼Τ￼υ￼Υ￼ύ￼ὺ￼ϋ￼ΰ￼ῢ￼ὐ￼ὑ￼Ὑ￼ὔ￼ὕ￼ὒ￼Ὕ￼ὓ￼ῦ￼ὖ￼ὗ￼Ὗ￼φ￼Φ￼χ￼Χ￼ψ￼Ψ￼ω￼ώ￼ὼ￼ῷ￼ὠ￼ὡ￼Ὡ￼ὤ￼Ὤ￼ὢ￼ὥ￼Ὥ￼ᾧ￼ῶ￼ῳ￼ῴ￼ὦ￼Ὦ￼ὧ￼Ὧ￼ᾠ&lt;/WordForming&gt;
  &lt;Numeric&gt;๐￼๑￼๒￼๓￼๔￼๕￼๖￼๗￼๘￼๙&lt;/Numeric&gt;
  &lt;Other&gt;U+0020￼-￼,￼.￼’￼«￼»￼(￼)￼[￼]&lt;/Other&gt;
&lt;/ValidCharacters&gt;' />
		<fw:defaultFontFeatures value='order=3 children=2 color=red createDate=1996' />
		<fw:legacyMapping value='SomeMapper' />
		<fw:scriptName value='scriptName' />
		<fw:regionName value='regionName' />
		<fw:variantName value='aVarName' />
		<fw:windowsLCID value='4321' />
</special>
</ldml>".Replace('\'', '"'), language, script, region, variant);
		}

		public static string Version0WithOldFwValidChars(string language, string script, string region, string variant)
		{
			return string.Format(
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
<special xmlns:palaso='urn://palaso.org/ldmlExtensions/v1'>
	<palaso:defaultFontFamily value='Arial' />
	<palaso:defaultFontSize value='12' />
</special>
<special xmlns:fw='urn://fieldworks.sil.org/ldmlExtensions/v1'>
		<fw:graphiteEnabled value='True' />
		<fw:validChars value='a b c d e f g' />
		<fw:defaultFontFeatures value='order=3 children=2 color=red createDate=1996' />
		<fw:legacyMapping value='SomeMapper' />
		<fw:scriptName value='scriptName' />
		<fw:regionName value='regionName' />
		<fw:variantName value='aVarName' />
		<fw:windowsLCID value='4321' />
</special>
</ldml>".Replace('\'', '"'), language, script, region, variant);
		}

		public static string Version0Bogus(string language, string script, string region, string variant, string bogus)
		{
			return string.Format(
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
<{4} />
<special xmlns:palaso='urn://palaso.org/ldmlExtensions/v1'>
	<palaso:defaultFontFamily value='Arial' />
	<palaso:defaultFontSize value='12' />
</special>
</ldml>".Replace('\'', '"'), language, script, region, variant, bogus);
		}

		public static string Version0(string language, string script, string region, string variant, string abbreviation)
		{
			return string.Format(
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
	<palaso:abbreviation value='{4}' />
	<palaso:defaultFontFamily value='Arial' />
	<palaso:defaultFontSize value='12' />
</special>
</ldml>".Replace('\'', '"'), language, script, region, variant, abbreviation);
		}

		public static string Version0(string language, string script, string region, string variant)
		{
			return string.Format(
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
		#endregion

		public static string Version1(string language, string script, string region, string variant, string abbreviation)
		{
			return
				string.Format(@"<?xml version='1.0' encoding='utf-8'?>
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
	<palaso:abbreviation value='{4}' />
	<palaso:version value='1' />
	<palaso:defaultFontFamily value='Arial' />
	<palaso:defaultFontSize value='12' />
</special>
</ldml>".Replace('\'', '"'), language, script, region, variant, abbreviation);
		}

		public static string Version1(string language, string script, string region, string variant)
		{
			return
				string.Format(@"<?xml version='1.0' encoding='utf-8'?>
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
	<palaso:version value='1' />
</special>
</ldml>".Replace('\'', '"'), language, script, region, variant);
		}

		#region Version 2 LDML
		public static string Version2(string language, string script, string region, string variant)
		{
			return
				string.Format(@"<?xml version='1.0' encoding='utf-8'?>
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
	<palaso:version value='2' />
</special>
<special xmlns:palaso2='urn://palaso.org/ldmlExtensions/v2'>
	<palaso2:knownKeyboards>
		<palaso2:keyboard layout='English' locale='en-US' os='Win32NT' />
	</palaso2:knownKeyboards>
</special>
</ldml>".Replace('\'', '"'), language, script, region, variant);
		}

		public static string Version2WithRightToLeftLayout(string languageSubtag, string languageName)
		{
			return string.Format(
				@"<?xml version='1.0' encoding='utf-8'?>
<ldml>
<identity>
	<version number='' />
	<generation date='0001-01-01T00:00:00' />
	<language type='{0}' />
</identity>
<layout><orientation characters='right-to-left' /></layout >
<collations />
<special xmlns:palaso='urn://palaso.org/ldmlExtensions/v1'>
	<palaso:languageName value='{1}' />
	<palaso:version value='2' />
</special>
</ldml>".Replace('\'', '"'), languageSubtag, languageName);
		}


		#endregion

		#region Version 3 

		/// <summary>
		/// Returns the string form of a collation element according to sortType
		/// </summary>
		/// <param name="sortType">Type of collation to use in the element: "standard", "simple", "simpleNeedsCompiling", "inherited"</param>
		/// <returns></returns>
		public static string CollationElem(string sortType)
		{
			string collationString = string.Empty;
			switch (sortType)
			{
				case "standard":
					collationString = @"
	<collations>
		<defaultCollation>standard</defaultCollation>
		<collation type='standard'>
			<cr><![CDATA[
				&B<t<<<T<s<<<S<e<<<E
				&C<k<<<K<x<<<X<i<<<I
				&D<q<<<Q<r<<<R
				&G<o<<<O
				&W<h<<<H
			]]></cr>
		</collation>
	</collations>".Replace("'", "\"");
					break;
				case "simple":
					collationString = @"
	<collations>
		<defaultCollation>standard</defaultCollation>
		<collation type='standard'>
			<cr><![CDATA[
				&B<t<<<T<s<<<S<e<<<E
				&C<k<<<K<x<<<X<i<<<I
				&D<q<<<Q<r<<<R
				&G<o<<<O
				&W<h<<<H
			]]></cr>
			<special xmlns:sil='urn://www.sil.org/ldml/0.1'>
				<sil:simple><![CDATA[
					a/A
					b/B
					t/T
					s/S
					c/C
					k/K
					x/X
					i/I
					d/D
					q/Q
					r/R
					e/E
					f/F
					g/G
					o/O
					j/J
					l/L
					m/M
					n/N
					p/P
					u/U
					v/V
					w/W
					h/H
					y/Y
					z/Z
				]]></sil:simple>
			</special>
		</collation>
	</collations>".Replace("'", "\"");
					break;
				case "simpleNeedsCompiling":
					collationString = @"
	<collations>
		<defaultCollation>standard</defaultCollation>
		<collation type='standard'>
			<special xmlns:sil='urn://www.sil.org/ldml/0.1' sil:needscompiling='true'>
				<sil:simple><![CDATA[
					a/A
					b/B
					t/T
					s/S
					c/C
					k/K
					x/X
					i/I
					d/D
					q/Q
					r/R
					e/E
					f/F
					g/G
					o/O
					j/J
					l/L
					m/M
					n/N
					p/P
					u/U
					v/V
					w/W
					h/H
					y/Y
					z/Z
				]]></sil:simple>
			</special>
		</collation>
	</collations>".Replace("'", "\"");
					break;
				case "inherited":
					collationString = @"
	<collations>
		<defaultCollation>standard</defaultCollation>
		<collation type='standard'>
			<cr><![CDATA[
				&B<t<<<T<s<<<S<e<<<E
				&C<k<<<K<x<<<X<i<<<I
				&D<q<<<Q<r<<<R
				&G<o<<<O
				&W<h<<<H
			]]></cr>
			<special xmlns:sil='urn://www.sil.org/ldml/0.1'>
				<sil:inherited base='my' type='standard'/>
			</special>
		</collation>
	</collations>".Replace("'", "\"");
					break;
			}
			return collationString;
		}

		public static string FontElem = @"
	<special xmlns:sil='urn://www.sil.org/ldml/0.1'>
		<sil:external-resources>
			<sil:font types='default emphasis' name='Padauk' size='2.1' minversion='3.1.4' features='order=3 children=2 color=red createDate=1996' lang='en' engines='gr ot' otlang='abcd' subset='unknown' >
				<sil:url>http://wirl.scripts.sil.org/padauk</sil:url>
				<sil:url>http://scripts.sil.org/cms/scripts/page.php?item_id=padauk</sil:url>
			</sil:font>
		</sil:external-resources>
	</special>".Replace("'", "\"");

		public static string SpellCheckerElem = @"
	<special xmlns:sil='urn://www.sil.org/ldml/0.1'>
		<sil:external-resources>
			<sil:spellcheck type='hunspell'>
				<sil:url>http://wirl.scripts.sil.org/hunspell</sil:url>
				<sil:url>http://scripts.sil.org/cms/scripts/page.php?item_id=hunspell</sil:url>
			</sil:spellcheck>
		</sil:external-resources>
	</special>".Replace("'", "\"");

		public static string KeyboardElem = @"
	<special xmlns:sil='urn://www.sil.org/ldml/0.1'>
		<sil:external-resources>
			<sil:kbd id='Compiled Keyman9' type='kmx'>
				<sil:url>http://wirl.scripts.sil.org/keyman</sil:url>
				<sil:url>http://scripts.sil.org/cms/scripts/page.php?item_id=keyman9</sil:url>
			</sil:kbd>
		</sil:external-resources>
	</special>".Replace("'", "\"");

		public static string TwoKeyboardElems = @"
	<special xmlns:sil='urn://www.sil.org/ldml/0.1'>
		<sil:external-resources>
			<sil:kbd id='basic_kbdgr' type='kmp'>
				<sil:url draft='generated'>https://keyman.com/go/keyboard/basic_kbdgr/download/kmp</sil:url>
			</sil:kbd>
			<sil:kbd id='sil_euro_latin' type='kmp'>
				<sil:url draft='generated'>https://keyman.com/go/keyboard/sil_euro_latin/download/kmp</sil:url>
			</sil:kbd>
		</sil:external-resources>
	</special>".Replace("'", "\"");

		/// <summary>
		/// Minimal LDML for version 3
		/// </summary>
		/// <param name="language"></param>
		/// <param name="script"></param>
		/// <param name="region"></param>
		/// <param name="variant"></param>
		/// <param name="otherElement"></param>
		/// <returns></returns>
		public static string Version3(string language, string script, string region, string variant, string otherElement = null)
		{
			return
				string.Format(@"<?xml version='1.0' encoding='utf-8'?>
<ldml>
	<identity>
		<version number='$Revision$'>Identity version description</version>
		<generation date='$Date$' />
		<language type='{0}' />
		<script type='{1}' />
		<territory type='{2}' />
		<variant type='{3}' />
	</identity>{4}
</ldml>".Replace("'", "\""), language, script, region, variant, otherElement);
		}

		/// <summary>
		/// Minimal LDML for version 3 along with sil:identity element
		/// </summary>
		public static string Version3Identity(string language, string script, string region, string variant,
											string uid, string windowsLCID, string variantName, string defaultRegion, string revid)
		{
			return
				string.Format(@"<?xml version='1.0' encoding='utf-8'?>
<ldml>
	<identity>
		<version number='$Revision$'>Identity version description</version>
		<generation date='$Date$' />
		<language type='{0}' />
		<script type='{1}' />
		<territory type='{2}' />
		<variant type='{3}' />
		<special xmlns:sil='urn://www.sil.org/ldml/0.1'>
			<sil:identity uid='{4}' windowsLCID='{5}' variantName='{6}' defaultRegion='{7}' revid='{8}'></sil:identity>
		</special>
	</identity>
</ldml>".Replace("'", "\""), language, script, region, variant, uid, windowsLCID, variantName, defaultRegion, revid);
		}
		#endregion

		public static string CurrentVersion(string language, string script, string region, string variant, string otherElement = null)
		{
			return Version3(language, script, region, variant, otherElement);
		}

		public static string CurrentVersion(string languageTag)
		{
			string language, script, region, variant;
			IetfLanguageTag.TryGetParts(languageTag, out language, out script, out region, out variant);
			return CurrentVersion(language, script, region, variant);

		}
	}
}