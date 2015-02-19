using System;
using SIL.WritingSystems.Migration.WritingSystemsLdmlV0To1Migration;

namespace SIL.WritingSystems.Tests
{
	public class LdmlContentForTests
	{
		static public string NoLdml = @"<?xml version='1.0' encoding='UTF-8' ?>
<noLdml>
</noLdml>
".Replace("'", "\"");
		
		static public string Version99Default()
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

		static public string Version0WithAllSortsOfDatathatdoesNotNeedSpecialAttention(string language, string script, string region, string variant)
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

		static public string Version0WithCollationInfo(WritingSystemDefinitionV0.SortRulesType sortType)
		{
			string collationelement = GetCollationElementXml(sortType);

			return String.Format(
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

		static public string Version0WithLdmlInfoWeDontCareAbout(string language, string script, string region, string variant)
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
			string collationelement = String.Empty;
			switch (sortType)
			{
				case WritingSystemDefinitionV0.SortRulesType.DefaultOrdering:
					collationelement = String.Empty;
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
			<alias source='en'/>
		</base>
		<rules>
			<reset before='primary'><first_non_ignorable /></reset><p>a</p><s>A</s><p>b</p><s>B</s><p>o</p><s>O</s><p>m</p><s>M</s>
		</rules>
		<special xmlns:palaso='urn://palaso.org/ldmlExtensions/v1'>
			<palaso:sortRulesType value='OtherLanguage' />
		</special>
	</collation>
";
					break;
			}
			return collationelement;
		}

		static public string Version0WithFw(string language, string script, string region, string variant)
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

		static public string Version0(string language, string script, string region, string variant, string abbreviation)
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
	<palaso:abbreviation value='{4}' />
	<palaso:defaultFontFamily value='Arial' />
	<palaso:defaultFontSize value='12' />
</special>
</ldml>".Replace('\'', '"'), language, script, region, variant, abbreviation);
		}

		static public string Version0(string language, string script, string region, string variant)
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
		#endregion

		static public string Version1(string language, string script, string region, string variant, string abbreviation)
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
	<palaso:abbreviation value='{4}' />
	<palaso:version value='1' />
	<palaso:defaultFontFamily value='Arial' />
	<palaso:defaultFontSize value='12' />
</special>
</ldml>".Replace('\'', '"'), language, script, region, variant, abbreviation);
		}

		static public string Version1(string language, string script, string region, string variant)
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

		static public string Version2(string language, string script, string region, string variant)
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
	<palaso:version value='2' />
	<palaso:defaultFontFamily value='Arial' />
	<palaso:defaultFontSize value='12' />
</special>
</ldml>".Replace('\'', '"'), language, script, region, variant);
		}

		#region Version 3 

		/// <summary>
		/// Returns the string form of a collation element according to sortType
		/// </summary>
		/// <param name="sortType">Type of collation to use in the element: "standard", "simple", "simpleNeedsCompiling", "inherited"</param>
		/// <returns></returns>
		static public string CollationElem(string sortType)
		{
			string collationString = string.Empty;
			switch (sortType)
			{
				case "standard" :
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
				case "simple" :
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
				case "simpleNeedsCompiling" :
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
				case "inherited" :
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
		
		static public string FontElem = @"
	<special xmlns:sil='urn://www.sil.org/ldml/0.1'>
		<sil:external-resources>
			<sil:font types='default emphasis' name='Padauk' size='2.1' minversion='3.1.4' features='order=3 children=2 color=red createDate=1996' lang='en' engines='gr ot' otlang='abcd' subset='unknown' >
				<sil:url>http://wirl.scripts.sil.org/padauk</sil:url>
				<sil:url>http://scripts.sil.org/cms/scripts/page.php?item_id=padauk</sil:url>
			</sil:font>
		</sil:external-resources>
	</special>".Replace("'", "\"");
		
		static public string SpellCheckerElem = @"
	<special xmlns:sil='urn://www.sil.org/ldml/0.1'>
		<sil:external-resources>
			<sil:spellcheck type='hunspell'>
				<sil:url>http://wirl.scripts.sil.org/hunspell</sil:url>
				<sil:url>http://scripts.sil.org/cms/scripts/page.php?item_id=hunspell</sil:url>
			</sil:spellcheck>
		</sil:external-resources>
	</special>".Replace("'", "\"");
		
		static public string KeyboardElem = @"
	<special xmlns:sil='urn://www.sil.org/ldml/0.1'>
		<sil:external-resources>
			<sil:kbd id='Compiled Keyman9' type='kmx'>
				<sil:url>http://wirl.scripts.sil.org/keyman</sil:url>
				<sil:url>http://scripts.sil.org/cms/scripts/page.php?item_id=keyman9</sil:url>
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
		static public string Version3(string language, string script, string region, string variant, string otherElement = null)
		{
			return
				String.Format(@"<?xml version='1.0' encoding='utf-8'?>
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
		/// <param name="language"></param>
		/// <param name="script"></param>
		/// <param name="region"></param>
		/// <param name="variant"></param>
		/// <param name="uid"></param>
		/// <param name="windowsLCID"></param>
		/// <param name="defaultRegion"></param>
		/// <param name="variantName"></param>
		/// <returns></returns>
		static public string Version3Identity(string language, string script, string region, string variant,
											string uid, string windowsLCID, string defaultRegion, string variantName)
		{
			return
				String.Format(@"<?xml version='1.0' encoding='utf-8'?>
<ldml>
	<identity>
		<version number='$Revision$'>Identity version description</version>
		<generation date='$Date$' />
		<language type='{0}' />
		<script type='{1}' />
		<territory type='{2}' />
		<variant type='{3}' />
		<special xmlns:sil='urn://www.sil.org/ldml/0.1'>
			<sil:identity uid='{4}' windowsLCID='{5}' defaultRegion='{6}' variantName='{7}'></sil:identity>
		</special>
	</identity>
</ldml>".Replace("'", "\""), language, script, region, variant, uid, windowsLCID, defaultRegion, variantName );
		}
		 #endregion

		static public string CurrentVersion(string language, string script, string region, string variant, string otherElement = null)
		{
			return Version3(language, script, region, variant, otherElement);
		}

		static public string CurrentVersion(string languageTag)
		{
			string language, script, region, variant;
			IetfLanguageTag.GetParts(languageTag, out language, out script, out region, out variant);
			return CurrentVersion(language, script, region, variant);
			
		}
	}
}