using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Palaso.TestUtilities
{
	public class LiftContentForTests
	{
		public static string GetSingleEntryWithWritingSystems(string id1, string id2)
		{
			return String.Format(
@"<entry id='6f5a1f30-ade8-11e0-9f1c-0800200c9a66'>
	<lexical-unit>
	  <form lang='{0}'>
		<text>eins</text>
	  </form>
	</lexical-unit>
	<sense>
		<grammatical-info value='n'/>
		<definition><form lang='{1}'><text>blah blah blah blah</text></form></definition>
		<example><form lang='{0}'><text>and example of lah blah blah blah</text></form></example>
	</sense>
</entry>", id1, id2);
		}

		public static string WrapEntriesInLiftElements(string version, string entriesXml)
		{
			return String.Format(
@"<lift
	version='{0}'
	producer='WeSay 1.0.0.0'>",version).Replace("'", "\"") + entriesXml + "</lift>";
		}

		public static string GetSingleEntryWithLexicalUnitContainingWritingsystemsAndContent(Dictionary<string, string> writingSystemsToContentMap)
		{
			var builder = new StringBuilder();
			builder.AppendLine();
			builder.Append(
@"  <entry id='6f5a1f30-ade8-11e0-9f1c-0800200c9a66'>
	<lexical-unit>");
			builder.AppendLine();
			foreach (var kvp in writingSystemsToContentMap)
			{
				if (String.IsNullOrEmpty(kvp.Value))
				{
					builder.AppendFormat(
@"        <form lang='{0}'>
		  <text/>
		</form>", kvp.Key);
				}
				else
				{
					builder.AppendFormat(
@"        <form lang='{0}'>
		  <text>{1}</text>
		</form>", kvp.Key, kvp.Value);
				}
				builder.AppendLine();
			}
			builder.Append(
@"    </lexical-unit>
	</entry>");
			builder.AppendLine();
			return builder.ToString();
		}

		public static string GetSingleEntrywithGlossContainingWritingsystemsAndContent(Dictionary<string, string> writingSystemsToContentMap)
		{
			var builder = new StringBuilder();
			builder.AppendLine();
			builder.Append(
@"  <entry id='6f5a1f30-ade8-11e0-9f1c-0800200c9a66'>
	<lexical-unit>
		<form lang='aiw'>
		  <text>aiw-lex</text>
		</form>
	</lexical-unit>
	<sense
		id='b7ccb68f-e16d-40dc-8a60-1102cbc20d00'>");
		builder.AppendLine();
			foreach (var kvp in writingSystemsToContentMap)
			{
				builder.AppendFormat(
@"        <gloss
			lang='{0}'>
			<text>{1}</text>
		</gloss>", kvp.Key, kvp.Value);
				builder.AppendLine();
			}
			builder.Append(
@"            <definition>
				<form
					lang='aiw'>
					<text>aiw-mean</text>
				</form>
				<form
					lang='aiw-Latn'>
					<text>aiw-mean</text>
				</form>
				<form
					lang='en'>
					<text>eng-mean</text>
				</form>
			</definition>
		</sense>
	</entry>");
			builder.AppendLine();
			return builder.ToString();
		}
	}
}
