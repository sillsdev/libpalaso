using System.Collections.Generic;

namespace SIL.WritingSystems
{
	public class LanguageInfo
	{
		private readonly List<string> _names = new List<string>();
		private readonly HashSet<string> _countries = new HashSet<string>();

		public string LanguageTag { get; set; }

		public IList<string> Names
		{
			get { return _names; }
		}

		public ISet<string> Countries
		{
			get { return _countries; }
		}
	}
}
