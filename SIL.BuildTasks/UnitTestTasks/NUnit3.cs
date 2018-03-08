// Copyright (c) 2016 SIL International
// This software is licensed under the MIT license (http://opensource.org/licenses/MIT)

using System;
using System.Text;

namespace SIL.BuildTasks.UnitTestTasks
{
	/// <summary>
	/// Run NUnit3 on a test assembly.
	/// </summary>
	public class NUnit3 : NUnit
	{
		private bool? _useNUnit3Xml;
		private bool _teamCity;

		public bool UseNUnit3Xml
		{
			get { return _useNUnit3Xml.HasValue && _useNUnit3Xml.Value; }
			set { _useNUnit3Xml = value; }
		}

		public bool NoColor { get; set; }

		/// <summary>
		/// Should be set to true if the tests are running on a TeamCity server.
		/// Adds --teamcity which "Turns on use of TeamCity service messages."
		/// </summary>
		public bool TeamCity
		{
			get { return _teamCity; }
			set
			{
				_teamCity = value;

				// According to Eberhard, we don't want this behavior by default on
				// Jenkins, so this is tied to the TeamCity property.
				// REVIEW: This should probably be true for NUnit also, but changing
				// the logic there could potentially cause unexpected results for existing
				// callers whereas the NUnit3 task is new enough, I think we are okay.
				// (And there is no TeamCity property for NUnit, anyway.)
				if (!FailTaskIfAnyTestsFail.HasValue)
					FailTaskIfAnyTestsFail = value;
			}
		}

		/// <summary>
		/// Gets the name (without path) of the NUnit executable. When running on Mono this is
		/// different from ProgramNameAndPath() which returns the executable we'll start.
		/// </summary>
		protected override string RealProgramName => "nunit3-console.exe";

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
			if (TeamCity)
				bldr.Append(" --teamcity");
			return bldr.ToString();
		}

		internal override string AddIncludeAndExcludeArguments()
		{
			var bldr = new StringBuilder();
			string include = null;
			string exclude = null;

			if (!string.IsNullOrWhiteSpace(IncludeCategory))
				include = BuildCategoriesString(IncludeCategory, "=", " or ");

			if (!string.IsNullOrWhiteSpace(ExcludeCategory))
				exclude = BuildCategoriesString(ExcludeCategory, "!=", " and ");

			if (include == null && exclude == null)
				return string.Empty;

			bldr.Append(" --where \"");
			if (include != null && exclude != null)
				bldr.Append(include).Append(" and ").Append(exclude);
			else
				bldr.Append(include).Append(exclude);
			bldr.Append("\"");

			return bldr.ToString();
		}

		private string BuildCategoriesString(string categoryString, string condition, string joiner)
		{
			var bldr = new StringBuilder();

			foreach (var cat in categoryString.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries))
				bldr.Append("cat" + condition + cat + joiner);

			bldr.Length = bldr.Length - joiner.Length; // remove final "or"
			bldr.Insert(0, "(").Append(")");
			return bldr.ToString();
		}
	}
}
