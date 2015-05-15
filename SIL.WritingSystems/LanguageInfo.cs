using System.Collections.Generic;
using System.Linq;

namespace SIL.WritingSystems
{
	public class LanguageInfo
	{
		private readonly List<string> _names = new List<string>();
		private readonly HashSet<string> _countries = new HashSet<string>();
		private string _desiredName;

		public string LanguageTag { get; set; }

		public IList<string> Names
		{
			get { return _names; }
		}

		public ISet<string> Countries
		{
			get { return _countries; }
		}

		/// <summary>
		/// Currently, we only have English names in our database. This holds the language name in the language, when we know it
		/// </summary>
		public string LocalName { get; set; }

		/// <summary>
		/// People sometimes don't want use the Ethnologue-supplied name
		/// </summary>
		public string DesiredName
		{
			get
			{
				if (string.IsNullOrEmpty(_desiredName))
					return Names.FirstOrDefault();
				return _desiredName;
			}
			set { _desiredName = string.IsNullOrEmpty(value) ? value : value.Trim(); }
		}
	}
}
