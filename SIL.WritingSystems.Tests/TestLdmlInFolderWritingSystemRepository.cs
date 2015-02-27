using System.Collections.Generic;
using System.IO;

namespace SIL.WritingSystems.Tests
{
	public class TestLdmlInFolderWritingSystemRepository : LdmlInFolderWritingSystemRepository
	{
		private readonly Dictionary<string, string> _sldrLdmls; 

		public TestLdmlInFolderWritingSystemRepository(string basePath)
			: base(basePath)
		{
			_sldrLdmls = new Dictionary<string, string>();
		}

		public IDictionary<string, string> SldrLdmls
		{
			get { return _sldrLdmls; }
		}

		protected override bool GetLdmlFromSldr(string path, string id)
		{
			string contents;
			if (_sldrLdmls.TryGetValue(id, out contents))
			{
				File.WriteAllText(path, contents);
				return true;
			}

			return false;
		}
	}
}
