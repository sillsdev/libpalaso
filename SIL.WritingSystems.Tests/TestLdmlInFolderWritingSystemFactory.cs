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
			if (_sldrLdmls.TryGetValue(id, out contents))
			{

				filename = id + ".ldml";
				File.WriteAllText(Path.Combine(path, filename), contents);
				return SldrStatus.FromSldr;
			}

			filename = null;
			return SldrStatus.UnableToConnectToSldr;
		}
	}
}
