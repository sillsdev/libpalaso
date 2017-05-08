using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using Enchant;
using SIL.i18n;
using SIL.Reporting;
using SIL.Spelling;
using SIL.Windows.Forms.HotSpot;

namespace SIL.Windows.Forms.Spelling
{
	[ProvideProperty("LanguageForSpellChecking", typeof(TextBoxBase))]
	public class TextBoxSpellChecker : IExtenderProvider, IComponent
	{
		private readonly Dictionary<Control, string> _extendees;
		private readonly HotSpotProvider _hotSpotProvider;
		private ISite _site;
		private readonly Broker _broker;
		private readonly Dictionary<string, Dictionary> _dictionaries;

		[DebuggerStepThrough]
		public TextBoxSpellChecker()
		{
			_extendees = new Dictionary<Control, string>();
			bool brokerSuccessfullyCreated = false;

			try
			{
				_broker = new Broker();
				brokerSuccessfullyCreated = true;
			}
			catch
			{
				//it's okay if we can't create one.
				// probably because Enchant isn't installed on this machine
			}

			if (brokerSuccessfullyCreated)
			{
				_hotSpotProvider = new HotSpotProvider();
				_hotSpotProvider.RetrieveHotSpots += CheckSpelling;
				_dictionaries = new Dictionary<string, Dictionary>();
			}
		}

		#region Enchant Methods

		private void AddToDictionary(string language, string s)
		{
			_dictionaries[language].Add(s);
			_hotSpotProvider.RefreshAll();
		}

		private IEnumerable<string> GetSuggestions(string language, string text)
		{
			try
			{
				return _dictionaries[language].Suggest(text);
			}
			catch (Exception)
			{
				//the actual error messages are always worthless, talking about corrupted memory
				ErrorReport.NotifyUserOfProblem(new ShowOncePerSessionBasedOnExactMessagePolicy(), "There was a problem with the Enchant Spell-Checking system related to {0}", language);
			}
			return new List<string>();
		}

		private bool IsWordSpelledCorrectly(string language, string s)
		{
			try
			{
				return _dictionaries[language].Check(s);
			}
			catch (Exception)
			{
				//the actual error messages are always worthless, talking about corrupted memory
				ErrorReport.NotifyUserOfProblem(new ShowOncePerSessionBasedOnExactMessagePolicy(), "There was a problem with the Enchant Spell-Checking system related to {0}", language);
			}
			return true;//review
		}

		#endregion

		#region IExtenderProvider Members

		public bool CanExtend(object extendee)
		{
			return extendee is TextBoxBase;
		}

		#endregion

		#region IComponent Members

		public event EventHandler Disposed;

		public ISite Site
		{
			get { return _site; }
			set { _site = value; }
		}

		public void Dispose()
		{
			_hotSpotProvider.Dispose();
			_extendees.Clear();
			if (Disposed != null)
			{
				Disposed(this, new EventArgs());
			}
		}

		#endregion

		private void CheckSpelling(object sender, RetrieveHotSpotsEventArgs e)
		{
			string text = e.Text;
			e.Color = Color.DarkSalmon;
			string language = GetLanguageForSpellChecking(e.Control);

			IEnumerable<WordTokenizer.Token> tokens = WordTokenizer.TokenizeText(text);
			foreach (WordTokenizer.Token token in tokens)
			{
				if (!IsWordSpelledCorrectly(language, token.Value))
				{
					HotSpot.HotSpot hotArea =
						new HotSpot.HotSpot(e.Control, token.Offset, token.Length);
					hotArea.MouseLeave += OnMouseLeaveHotSpot;
					hotArea.MouseEnter += OnMouseEnterHotSpot;
					e.AddHotSpot(hotArea);
				}
			}
		}

		private void OnAddToDictionary(object sender, EventArgs e)
		{
			ToolStripMenuItem item = (ToolStripMenuItem)sender;
			HotSpot.HotSpot hotSpot = (HotSpot.HotSpot)item.Tag;

			string language = GetLanguageForSpellChecking(hotSpot.Control);
			AddToDictionary(language, hotSpot.Text);
		}

		private static void OnChooseSuggestedSpelling(object sender, EventArgs e)
		{
			ToolStripMenuItem item = (ToolStripMenuItem)sender;
			ReplaceText((HotSpot.HotSpot)item.Tag, item.Text);
		}

		private static void ReplaceText(HotSpot.HotSpot area, string text)
		{
			TextBoxBase control = area.Control;
			control.SelectionStart = area.Offset;
			control.SelectionLength = area.Text.Length;
			TextBox textBox = control as TextBox;
			if (textBox != null)
			{
				textBox.Paste(text); //allows it to be undone
			}
			else
			{
				control.SelectedText = text;
			}
			control.Invalidate();
		}

		private ContextMenuStrip GetSuggestionContextMenu(string language, HotSpot.HotSpot hotSpot)
		{
			ContextMenuStrip strip = new ContextMenuStrip();
			strip.ShowImageMargin = false;
			strip.ShowCheckMargin = false;

			ToolStripMenuItem item;
			int suggestionCount = 0;
			foreach (string suggestion in GetSuggestions(language, hotSpot.Text))
			{
				if (++suggestionCount > 10)
				{
					break;
				}
				item = new ToolStripMenuItem(suggestion);
				item.Tag = hotSpot;
				item.Click += OnChooseSuggestedSpelling;
				strip.Items.Add(item);
			}
			if (strip.Items.Count == 0)
			{
				item = new ToolStripMenuItem(StringCatalog.Get("(No Spelling Suggestions)"));
				item.Enabled = false;
				strip.Items.Add(item);
			}
			strip.Items.Add(new ToolStripSeparator());
			item = new ToolStripMenuItem(StringCatalog.Get("Add to Dictionary"));
			item.Tag = hotSpot;
			item.Click += OnAddToDictionary;
			strip.Items.Add(item);
			return strip;
		}

		private void OnMouseEnterHotSpot(object sender, EventArgs e)
		{
			HotSpot.HotSpot hotSpot = (HotSpot.HotSpot)sender;
			string language = GetLanguageForSpellChecking(hotSpot.Control);
			hotSpot.Control.ContextMenuStrip = GetSuggestionContextMenu(language, hotSpot);
		}

		private static void OnMouseLeaveHotSpot(object sender, EventArgs e)
		{
			((HotSpot.HotSpot)sender).Control.ContextMenuStrip = null;
		}

		[DefaultValue("")]
		public string GetLanguageForSpellChecking(Control c)
		{
			if (c == null)
			{
				throw new ArgumentNullException();
			}
			if (!CanExtend(c))
			{
				throw new ArgumentException("Control must be derived from TextBoxBase");
			}
			string value;
			if (_extendees.TryGetValue(c, out value))
			{
				return value;
			}
			return string.Empty;
		}

		public void SetLanguageForSpellChecking(Control control, string language)
		{
			if (control == null)
			{
				throw new ArgumentNullException();
			}
			if (!CanExtend(control))
			{
				throw new ArgumentException("Control must be derived from TextBoxBase");
			}
			if (String.IsNullOrEmpty(language))
			{
				if (_hotSpotProvider != null)
				{
					_hotSpotProvider.SetEnableHotSpots(control, false);
				}
				_extendees.Remove(control);
			}
			else
			{
				if (_broker != null)
				{
					try
					{
						if (_broker.DictionaryExists(language))
						{
							if (!_dictionaries.ContainsKey(language))
							{
								_dictionaries.Add(language, _broker.RequestDictionary(language));

							}
							_hotSpotProvider.SetEnableHotSpots(control, true);
						}
					}
					catch (Exception)
					{
						//the actual error messages are always worthless, talking about corrupted memory
						//ErrorReport.NotifyUserOfProblem(new ShowOncePerSessionBasedOnExactMessagePolicy(), "There was a problem with the Enchant Spell-Checking system related to {0}", language);

						//The number of false errors here is so high that for now, let's not bother to scare the user
					}
				}
				_extendees[control] = language;
			}
		}
	}
}