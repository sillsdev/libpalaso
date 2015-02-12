using System.Linq;
using System.Xml.Linq;
using Palaso.Extensions;
using SIL.WritingSystems;

namespace SIL.LexiconUtils
{
	public class UserSettingsWritingSystemDataMapper : ICustomDataMapper
	{
		private readonly ISettingsStore _settingsStore;

		public UserSettingsWritingSystemDataMapper(ISettingsStore settingsStore)
		{
			_settingsStore = settingsStore;
		}

		public void Read(WritingSystemDefinition ws)
		{
			XElement userSettingsElem = _settingsStore.GetSettings();
			if (userSettingsElem == null)
				return;

			XElement wsElem = userSettingsElem.Elements("WritingSystems").Elements("WritingSystem").FirstOrDefault(e => (string) e.Attribute("id") == ws.Id);
			if (wsElem == null)
				return;

			var keyboardId = (string) wsElem.Element("LocalKeyboard");
			if (!string.IsNullOrEmpty(keyboardId))
			{
				IKeyboardDefinition keyboard;
				ws.LocalKeyboard = ws.KnownKeyboards.TryGetItem(keyboardId, out keyboard) ? keyboard
					: Keyboard.Controller.CreateKeyboardDefinition(keyboardId, KeyboardFormat.Unknown, Enumerable.Empty<string>());
			}
			var defaultFontName = (string) wsElem.Element("DefaultFontName");
			if (!string.IsNullOrEmpty(defaultFontName))
			{
				FontDefinition font;
				ws.DefaultFont = ws.Fonts.TryGetItem(defaultFontName, out font) ? font : new FontDefinition(defaultFontName);
			}
			ws.DefaultFontSize = (float?) wsElem.Element("DefaultFontSize") ?? 0f;
			ws.IsGraphiteEnabled = (bool?) wsElem.Element("IsGraphiteEnabled") ?? true;
		}

		public void Write(WritingSystemDefinition ws)
		{
			XElement userSettingsElem = _settingsStore.GetSettings() ?? new XElement("LexiconUserSettings");
			XElement wssElem = userSettingsElem.Element("WritingSystems");
			if (wssElem == null)
			{
				wssElem = new XElement("WritingSystems");
				userSettingsElem.Add(wssElem);
			}
			XElement wsElem = wssElem.Elements("WritingSystem").FirstOrDefault(e => (string) e.Attribute("id") == ws.Id);
			if (wsElem == null)
			{
				wsElem = new XElement("WritingSystem", new XAttribute("id", ws.Id));
				wssElem.Add(wsElem);
			}
			wsElem.RemoveNodes();

			if (ws.LocalKeyboard != null)
				wsElem.Add(new XElement("LocalKeyboard", ws.LocalKeyboard.Id));
			if (ws.DefaultFont != null)
				wsElem.Add(new XElement("DefaultFontName", ws.DefaultFont.Name));
			if (ws.DefaultFontSize != 0)
				wsElem.Add(new XElement("DefaultFontSize", ws.DefaultFontSize));
			if (!ws.IsGraphiteEnabled)
				wsElem.Add(new XElement("IsGraphiteEnabled", ws.IsGraphiteEnabled));

			_settingsStore.SaveSettings(userSettingsElem);
		}

		public void Remove(string wsId)
		{
			XElement userSettingsElem = _settingsStore.GetSettings();
			if (userSettingsElem == null)
				return;

			XElement wssElem = userSettingsElem.Element("WritingSystems");
			if (wssElem == null)
				return;

			XElement wsElem = wssElem.Elements("WritingSystem").FirstOrDefault(e => (string) e.Attribute("id") == wsId);
			if (wsElem != null)
			{
				wsElem.Remove();
				if (!wssElem.HasElements)
					wssElem.Remove();

				_settingsStore.SaveSettings(userSettingsElem);
			}
		}
	}
}
