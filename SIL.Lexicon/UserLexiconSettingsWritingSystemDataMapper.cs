using System.Linq;
using System.Xml.Linq;
using SIL.Keyboarding;
using SIL.WritingSystems;

namespace SIL.Lexicon
{
	public class UserLexiconSettingsWritingSystemDataMapper : UserLexiconSettingsWritingSystemDataMapper<WritingSystemDefinition>
	{
		public UserLexiconSettingsWritingSystemDataMapper(ISettingsStore settingsStore)
			: base(settingsStore)
		{
		}
	}

	public class UserLexiconSettingsWritingSystemDataMapper<T> : ICustomDataMapper<T> where T : WritingSystemDefinition
	{
		private readonly ISettingsStore _settingsStore;

		public UserLexiconSettingsWritingSystemDataMapper(ISettingsStore settingsStore)
		{
			_settingsStore = settingsStore;
		}

		public virtual void Read(T ws)
		{
			XElement userSettingsElem = _settingsStore.GetSettings();
			if (userSettingsElem == null)
				return;

			XElement wsElem = userSettingsElem.Elements("WritingSystems").Elements("WritingSystem").FirstOrDefault(e => (string) e.Attribute("id") == ws.LanguageTag);
			if (wsElem == null)
				return;

			var keyboardId = (string) wsElem.Element("LocalKeyboard");
			if (!string.IsNullOrEmpty(keyboardId))
			{
				IKeyboardDefinition keyboard;
				ws.LocalKeyboard = ws.KnownKeyboards.TryGet(keyboardId, out keyboard) ? keyboard
					: Keyboard.Controller.CreateKeyboard(keyboardId, KeyboardFormat.Unknown, Enumerable.Empty<string>());
			}

			XElement knownKeyboardsElem = wsElem.Element("KnownKeyboards");
			if (knownKeyboardsElem != null)
			{
				foreach (XElement knownKeyboard in knownKeyboardsElem.Elements("KnownKeyboard"))
				{
					var id = (string) knownKeyboard;
					IKeyboardDefinition keyboard;
					if (!Keyboard.Controller.TryGetKeyboard(id, out keyboard))
						keyboard = Keyboard.Controller.CreateKeyboard(id, KeyboardFormat.Unknown, Enumerable.Empty<string>());
					// Check KnownKeyboards for a keyboard with the same identifier, not for the object we just created
					if (!ws.KnownKeyboards.Contains(id))
						ws.KnownKeyboards.Add(keyboard);
				}
			}

			var defaultFontName = (string) wsElem.Element("DefaultFontName");
			if (!string.IsNullOrEmpty(defaultFontName))
			{
				FontDefinition font;
				ws.DefaultFont = ws.Fonts.TryGet(defaultFontName, out font) ? font : new FontDefinition(defaultFontName);
			}
			ws.DefaultFontSize = (float?) wsElem.Element("DefaultFontSize") ?? 0f;
			ws.IsGraphiteEnabled = (bool?) wsElem.Element("IsGraphiteEnabled") ?? true;
			ws.AcceptChanges();
		}

		public virtual void Write(T ws)
		{
			XElement userSettingsElem = _settingsStore.GetSettings() ?? new XElement("UserLexiconSettings");
			XElement wssElem = userSettingsElem.Element("WritingSystems");
			if (wssElem == null)
			{
				wssElem = new XElement("WritingSystems");
				userSettingsElem.Add(wssElem);
			}
			XElement wsElem = wssElem.Elements("WritingSystem").FirstOrDefault(e => (string) e.Attribute("id") == ws.LanguageTag);
			if (wsElem == null)
			{
				wsElem = new XElement("WritingSystem", new XAttribute("id", ws.LanguageTag));
				wssElem.Add(wsElem);
			}
			wsElem.RemoveNodes();

			if (ws.LocalKeyboard != null)
				wsElem.Add(new XElement("LocalKeyboard", ws.LocalKeyboard.Id));
			if (ws.KnownKeyboards.Count > 0)
				wsElem.Add(new XElement("KnownKeyboards", ws.KnownKeyboards.Select(k => new XElement("KnownKeyboard", k.Id))));
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
