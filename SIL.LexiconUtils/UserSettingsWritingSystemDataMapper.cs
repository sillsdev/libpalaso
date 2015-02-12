using System;
using System.Linq;
using System.Xml.Linq;
using Palaso.Extensions;
using SIL.WritingSystems;

namespace SIL.LexiconUtils
{
	public class UserSettingsWritingSystemDataMapper : ICustomDataMapper
	{
		private readonly Func<string> _getSettings;
		private readonly Action<string> _setSettings;

		public UserSettingsWritingSystemDataMapper(Func<string> getSettings, Action<string> setSettings)
		{
			_getSettings = getSettings;
			_setSettings = setSettings;
		}

		public void Read(WritingSystemDefinition ws)
		{
			string userSettings = (_getSettings() ?? string.Empty).Trim();
			if (string.IsNullOrEmpty(userSettings))
				return;

			XElement userSettingsElem = XElement.Parse(userSettings);
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
			string userSettings = (_getSettings() ?? string.Empty).Trim();
			XElement userSettingsElem = !string.IsNullOrEmpty(userSettings) ? XElement.Parse(userSettings) : new XElement("LexiconUserSettings");
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

			_setSettings(userSettingsElem.ToString());
		}

		public void Remove(string wsId)
		{
			string userSettings = (_getSettings() ?? string.Empty).Trim();
			if (string.IsNullOrEmpty(userSettings))
				return;

			XElement userSettingsElem = XElement.Parse(userSettings);
			XElement wssElem = userSettingsElem.Element("WritingSystems");
			if (wssElem == null)
				return;

			XElement wsElem = wssElem.Elements("WritingSystem").FirstOrDefault(e => (string) e.Attribute("id") == wsId);
			if (wsElem != null)
			{
				wsElem.Remove();
				if (!wssElem.HasElements)
					wssElem.Remove();
				_setSettings(userSettingsElem.ToString());
			}
		}
	}
}
