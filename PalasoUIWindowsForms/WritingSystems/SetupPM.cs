using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;

using System.Windows.Forms;

using Palaso.WritingSystems;

namespace Palaso.UI.WindowsForms.WritingSystems
{
	public class SetupPM
	{

		#region Properties
		private IWritingSystemStore _writingSystemStore;
		public IWritingSystemStore WritingSystemStore
		{
			get { return _writingSystemStore; }
			set { _writingSystemStore = value; }
		}

		public IEnumerable<String> KeyboardNames
		{
			get
			{
				List<String> keyboards = new List<string>();
				keyboards.Add("(default)");

				KeymanLink.KeymanLink keymanLink = new KeymanLink.KeymanLink();
				if (keymanLink.Initialize(false))
				{
					foreach (KeymanLink.KeymanLink.KeymanKeyboard keyboard in keymanLink.Keyboards)
					{
						keyboards.Add(keyboard.KbdName);
					}
				}

				foreach (InputLanguage lang in InputLanguage.InstalledInputLanguages)
				{
					keyboards.Add(lang.LayoutName);
				}
				return keyboards;
			}
		}

		public IEnumerable<FontFamily> FontFamilies
		{
			get
			{
				foreach (FontFamily family in System.Drawing.FontFamily.Families)
				{
					yield return family;
				}
			}
		}

		public IEnumerable<WritingSystemDefinition> WritingSystemsDefinitions
		{
			get
			{
				return _writingSystemStore.WritingSystemDefinitions;
			}
		}

		public WritingSystemDefinition Current
		{
			get
			{
				return null;
			}
		}

		#endregion

		#region Events

		public event EventHandler EventAddOrDelete;

		#endregion

		public InputLanguage FindInputLanguage(string name)
		{
			if (InputLanguage.InstalledInputLanguages != null) // as is the case on Linux
			{
				foreach (InputLanguage l in InputLanguage.InstalledInputLanguages)
				{
					if (l.LayoutName == name)
					{
						return l;
					}
				}
			}
			return null;
		}

		public void SelectCurrentByStoreID(string storeID)
		{
		}

		public void SelectCurrentByIndex(int index)
		{
		}

		public void DeleteCurrent()
		{
			OnAddOrDelete();

		}

		public void DuplicateCurrent()
		{
			OnAddOrDelete();

		}

		public WritingSystemDefinition AddNew()
		{
			WritingSystemDefinition ws = _writingSystemStore.AddNew();
			ws.Abbreviation = "New";
			OnAddOrDelete();
			return ws;
		}

		private void OnAddOrDelete()
		{
			if (EventAddOrDelete != null)
			{
				EventAddOrDelete(this, new EventArgs());
			}
		}

		public void RenameCurrent(string newShortName)
		{
		}

	}
}
