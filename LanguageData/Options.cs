// Copyright (c) 2016-2017 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using CommandLine;
using CommandLine.Text;

namespace LanguageData
{
		//LanguageData will process Ethnologue, IANA subtag and ISO693-3 data to a single language index file
		// Tasks/Parameters
		// -g Get fresh source files
		// -c Check if source files have changed (implies -g) and return
		// input directory for where source files will be and fresh ones should go (default LanguageData/Resources)
		// IANA subtags and 2to3lettercodes are used in SIL.WritingSystems by other parts,
		// but Ethnologue LanguageIndex is only used in LanguageLookup
		// output directory for index file (default SIL.WritingSystems/Reources)
		// output filename for index file
	class Options
	{
		//[ParserState]
		//public IParserState LastParserState { get; set; }

		[Option('i', "input", HelpText = "Input directory containing source files.")]
		public string InputDir { get; set; }

		[Option('o', "output", DefaultValue = "LanguageDataIndex.txt", HelpText = "Output file to write.")]
		public string OutputFile { get; set; }

		[Option('g', "get", DefaultValue = false, HelpText = "Get fresh source files.")]
		public bool GetFresh { get; set; }

		[Option('c', "check", DefaultValue = false, HelpText = "Download source files and check if they are new.")]
		public bool CheckFresh { get; set; }

		[Option('v', "verbose", DefaultValue = false, HelpText = "Verbose output.")]
		public bool Verbose { get; set; }

		[Option('h', null, HelpText = "Display this help screen.")]
		public bool ShowHelp { get; set; }


		[HelpOption]
		public string GetUsage()
		{
			var help = new HelpText {
				Heading = new HeadingInfo("LanguageData", "0.1"),
				Copyright = new CopyrightInfo("SIL International", 2016, 2017),
				AdditionalNewLineAfterOption = false,
				AddDashesToOption = true
			};
			//help.AddPreOptionsLine("<<license details here.>>");
			//help.AddPreOptionsLine("Usage: app -p Someone");
			help.AddPreOptionsLine("LanguageData will process Ethnologue, IANA subtag and ISO693-3 data to a single language data index file.");
			help.AddOptions(this);
			//var usage = new StringBuilder();
			//usage.AppendLine("LanguageData (c) 2016 SIL International");
			//usage.AppendLine("LanguageData will process Ethnologue, IANA subtag and ISO693-3 data to a single language data index file.");
			//Console.WriteLine("LastParseState: {0}", this.LastParserState?.ToString());
			/*if (this.LastParserState?.Errors.Any() == true)
			{
				Console.WriteLine ("have some cl parsing errors");
				var errors = help.RenderParsingErrorsText(this, 2); // indent with two spaces

				if (!string.IsNullOrEmpty (errors)) {
					Console.WriteLine ("error list is not empty");
					help.AddPreOptionsLine (string.Concat (Environment.NewLine, "ERROR(S):"));
					help.AddPreOptionsLine (errors);
				} else
					Console.WriteLine ("error list is empty");
			}
			*/
			return help;
		}
	}
}

