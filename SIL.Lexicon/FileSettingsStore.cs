using System.IO;
using System.Xml.Linq;

namespace SIL.Lexicon
{
	public class FileSettingsStore : ISettingsStore
	{
		private readonly string _settingsFilePath;

		private XDocument _settingsDoc;

		public FileSettingsStore(string settingsFilePath)
		{
			_settingsFilePath = settingsFilePath;
		}

		public XElement GetSettings()
		{
			if (_settingsDoc == null && File.Exists(_settingsFilePath))
				_settingsDoc = XDocument.Load(_settingsFilePath);

			return _settingsDoc == null ? null : _settingsDoc.Root;
		}

		public void SaveSettings(XElement userSettingsElem)
		{
			if (_settingsDoc == null)
				_settingsDoc = new XDocument();
			_settingsDoc.ReplaceNodes(userSettingsElem);
			_settingsDoc.Save(_settingsFilePath);
		}
	}
}
