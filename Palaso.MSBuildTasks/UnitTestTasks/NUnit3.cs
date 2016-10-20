// Copyright (c) 2016 SIL International
// This software is licensed under the MIT license (http://opensource.org/licenses/MIT)
using System.Text;

namespace Palaso.BuildTasks.UnitTestTasks
{
	/// <summary>
	/// Run NUnit on a test assembly.
	/// </summary>
	public class NUnit3 : NUnit
	{
		private bool? _useNUnit3Xml;

		public bool UseNUnit3Xml
		{
			get { return _useNUnit3Xml.HasValue && _useNUnit3Xml.Value; }
			set { _useNUnit3Xml = value; }
		}

		public bool NoColor { get; set; }

		/// <summary>
		/// Gets the name (without path) of the NUnit executable. When running on Mono this is
		/// different from ProgramNameAndPath() which returns the executable we'll start.
		/// </summary>
		protected override string RealProgramName
		{
			get
			{
				return "nunit3-console.exe";
			}
		}

		protected override string AddAdditionalProgramArguments()
		{
			var bldr = new StringBuilder();
			//bldr.Append(" --noheader");
			// We don't support TestInNewThread for now
			if (!string.IsNullOrEmpty(OutputXmlFile))
			{
				bldr.AppendFormat(" \"--result:{0};format={1}\"", OutputXmlFile,
					UseNUnit3Xml ? "nunit3" : "nunit2");
			}
			bldr.Append(" --labels=All");
			if (NoColor)
				bldr.Append(" --nocolor");
			if (Force32Bit)
				bldr.Append(" --x86");
			return bldr.ToString();
		}
	}
}
