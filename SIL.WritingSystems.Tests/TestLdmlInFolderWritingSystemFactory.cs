using System.Collections.Generic;
using System.IO;

namespace SIL.WritingSystems.Tests
{
	public class TestLdmlInFolderWritingSystemFactory : LdmlInFolderWritingSystemFactory
	{
		private readonly Dictionary<string, string> _sldrLdmls; 

		public TestLdmlInFolderWritingSystemFactory(LdmlInFolderWritingSystemRepository writingSystemRepository)
			: base(writingSystemRepository)
		{
			_sldrLdmls = new Dictionary<string, string>();
		}

		public IDictionary<string, string> SldrLdmls
		{
			get { return _sldrLdmls; }
		}

		protected override SldrStatus GetLdmlFromSldr(string path, string id, out string filename)
		{
			string contents;
			filename = string.Empty;
			if (_sldrLdmls.TryGetValue(id, out contents))
			{
				File.WriteAllText(path, contents);
				filename = id + ".ldml";
				return SldrStatus.FileFromSldr;
			}

			return SldrStatus.FileNotFound;
		}
	}
}
