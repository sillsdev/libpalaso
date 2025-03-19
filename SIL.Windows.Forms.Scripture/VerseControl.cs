using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using L10NSharp;
using SIL.PlatformUtilities;
using SIL.Scripture;
using SIL.Extensions;
using SIL.Linq;
using SIL.Windows.Forms.Miscellaneous;
using SIL.Windows.Forms.Widgets;
using static System.Char;

namespace SIL.Windows.Forms.Scripture
{
	public delegate void NoArgsDelegate();
	public delegate void OneArgDelegate(Object s);

	/// <summary>
	/// Control that allows the specifying of a book, chapter and verse
	/// in the same style as Paratext.
	/// </summary>
	public partial class VerseControl : UserControl
	{
		#region Fields and Constructors

		private const FontStyle searchTextStyle = FontStyle.Bold;
		private const string rtlMark = "\u200f";
		private const string ltrMark = "\u200e";
		private const int AbbreviationLength = 3; // length of book abbreviation
		private BookSet booksPresentSet = new BookSet();
		private string abbreviations = "";
		private string searchText = "";
		private bool allowVerseSegments = true; //Allow LXX style lettered verse segments
		private bool showEmptyBooks = true;
		private IScrVerseRef curRef;
		private float abbreviationWidth = -1.0f; //starting width of book abbreviation, <0 signifies to recalculate
		private readonly BookListItem[] allBooks;
		private Font emptyBooksFont = SystemFonts.DefaultFont;

		/// <summary>Function that can be set to allow the control to get localized names for the books.</summary>
		public Func<string, string> GetLocalizedBookName = Canon.BookIdToEnglishName;

		// change events
		/// <summary>Fired when the reference is changed</summary>
		public event PropertyChangedEventHandler VerseRefChanged;
		/// <summary>Fired when the books listed in the control change</summary>
		public event PropertyChangedEventHandler BooksPresentChanged;
		/// <summary>Fired when an invalid book is entered</summary>
		public event EventHandler InvalidBookEntered;
		/// <summary>Fired when any part of the reference is invalid</summary>
		public event EventHandler InvalidReferenceEntered;
		/// <summary>Fired when any part of the pasted reference is invalid</summary>
		public event EventHandler InvalidReferencePasted;
		/// <summary>Fired when any textbox for the reference gets focus</summary>
		public event EventHandler TextBoxGotFocus;
		/// <summary>
		/// Fired when the user presses Tab while in the verse portion of the control. This will
		/// fire even if the chapter or verse fields have values that are out of range for the
		/// selected book, but if the handler causes this control to lose focus, those values will
		/// be constrained to the valid ranges for the book/chapter.
		/// </summary>
		public event EventHandler<KeyEventArgs> TabKeyPressedInVerseField;
		/// <summary>
		/// Fired when the user presses Shift-Tab while in the book portion of the control iff book
		/// is valid.
		/// </summary>
		public event EventHandler<KeyEventArgs> ShiftTabPressedInBookField;

		// Used to temporarily ignore changes in the index of the book control
		// when it is being updated internally
		private bool isUpdating;

		public VerseControl()
		{
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();

			if (Platform.IsLinux)
			{
				// Set a smaller font on Linux. (Stops 'J''s being clipped)
				uiBook.Font = new Font("Segoe UI", 8F, FontStyle.Regular, GraphicsUnit.Point, 0);

				// Also increase the ItemHeight as to stop clipping of items in the dropdown menu.
				uiBook.ItemHeight += 2;
			}

			// Create reference and set to English versification
			curRef = new VerseRef(ScrVers.English);

			// Add books...
			allBooks = new BookListItem[Canon.LastBook];
			InitializeBooks(); //...to internal data structure
			PopulateBooks(); //...and to combobox list
			
			uiBook.GotFocus += control_GotFocus;
			uiChapter.GotFocus += control_GotFocus;
			uiVerse.GotFocus += control_GotFocus;

			// Cause Drop down list to be initially populated.
			// (stop initial click on Combo box drop down being ignored on mono)
			// TODO: fix mono bug where dropdown isn't shown if list is empty even
			// if dropdown event handlers populate list.
			if (Platform.IsMono)
				uiBook.BeforeMouseDown += (sender, e) =>
				{
					if (uiBook.Items.Count == 0) PopulateBooks();
				};

		}

		#endregion

		#region Helpers and Initializers

		private void InitializeBooks()
		{
			int i = 0;
			StringBuilder abbrevs = new StringBuilder();
			foreach (string abbrev in Canon.AllBookIds)
			{
				abbrevs.Append(abbrev);
				abbrevs.Append(",");
				BookListItem item = new BookListItem(abbrev);
				item.isPresent = booksPresentSet.IsSelected(i + Canon.FirstBook);
				allBooks[i] = item;
				i++;
			}
			abbreviations = abbrevs.ToString();
			RelocalizeBooks();
		}

		private void RelocalizeBooks()
		{
			foreach (BookListItem item in allBooks)
			{
				item.Name = GetLocalizedBookName(item.Abbreviation);
				item.BaseName = item.Name.ToUpperInvariant().RemoveDiacritics();
			}
		}

		private void PopulateBooks()
		{
			Unfilter();
		}

		#endregion

		#region Properties

		[Browsable(true)]
		[PublicAPI]
		public bool AllowVerseSegments
		{
			get => allowVerseSegments;
			set
			{
				if (allowVerseSegments == value)
					return; //no op
				//FUTURE: if (!value) //turning off segments, remove segment portion, fire event if change ref
				allowVerseSegments = value;
			}
		}

		private bool VerseSegmentsAvailable => curRef.VersificationHasVerseSegments;

		/// <summary>
		/// Gets or sets the current verse reference
		/// NOTES ON VERSE SEGMENT HANDLING:
		/// April 2009. We're adding segmented verse handling late in the PT7 development cycle. PT-1876.
		/// The goal is to give the functionality to SLT users while minimizing chance of breakage elsewhere.
		/// So, even though the right place for some of this logic is probably in VerseRef, for PT7 we'll keep it
		/// here and offer this extra property that segment-aware tools can check.
		/// 
		/// Basic design:
		/// 1. VerseControl now has a property AllowVerseSegments that turns on segment aware behavior.
		/// 2. Phase 1 implementation: allow setting a segmented verse ref to the control and inputting
		///	segments in the verse field. VerseRef property may now return segmented verses.
		/// 3. Phase 2: allow prev/next verse to be segment aware. Support in LXX edit window.
		/// </summary>
		[Browsable(false), ReadOnly(true)]
		public IScrVerseRef VerseRef
		{
			get
			{
				// FB 21930 partially entered data could be in control, so need to call AcceptData before
				// returning verse
				try
				{
					// In tests, the handle may not be created. Attempting to check for updated data can cause
					// a SendMessage, which can result in a hang.
					if (IsHandleCreated && !InvokeRequired)
						AcceptData();
				}
				catch (Exception)
				{
					// ignore errors at this point
				}
				// old way, before segmented references allowed
				// return new VerseRef(curRef).Simplify(); 
				return curRef.UnBridge();
			}
			set
			{
				if (value == null || value.BookNum < 1 || value.ChapterNum < 1 || value.VerseNum < 0)
				{
					Trace.TraceWarning("Invalid VerseRef passed to VerseControl: " + value);
					// Ignore it for now (see PT-1310)
					return;
				}

				curRef = value.Clone();
				if (!allowVerseSegments)
					curRef.Simplify();

				if (AdvanceToEnd)
					curRef.AdvanceToLastSegment();

				UpdateControls();
			}
		}

		[Browsable(false)]
		private string BookAbbreviation => curRef.Book;

		[Browsable(false), ReadOnly(true)]
		[PublicAPI]
		public BookSet BooksPresentSet
		{
			set
			{
				booksPresentSet = value;
				OnBooksPresentChanged(new PropertyChangedEventArgs("BooksPresent"));
			}
		}

		[PublicAPI]
		public Font EmptyBooksFont
		{
			get => emptyBooksFont;
			set
			{
				if (Equals(value, emptyBooksFont))
					return;
				emptyBooksFont = value ?? SystemFonts.DefaultFont;
				abbreviationWidth = -1.0f;
			}
		}

		[PublicAPI]
		public Color EmptyBooksColor { get; set; } = SystemColors.GrayText;

		public bool ShowEmptyBooks
		{
			get => showEmptyBooks;
			set
			{
				if (showEmptyBooks == value) return;
				showEmptyBooks = value;
				PopulateBooks();
			}
		}

		/// <summary>
		/// When using a verse control to indicate the end of a range, choosing
		/// a new book should go to the last verse of the last chapter. Choosing
		/// a new chapter should go to the last verse of that chapter.
		/// </summary>
		public bool AdvanceToEnd { get; set; }

		/// <summary> 
		/// Set tooltip for the verse spinner and text field.
		/// </summary>
		[PublicAPI]
		public string ToolTipVerseSelector
		{
			set
			{
				this.uiToolTip.SetToolTip(this.uiVerseSpinner, value);
				this.uiToolTip.SetToolTip(this.uiVerse, value);
			}
		}

		/// <summary> 
		/// Set tooltip for the chapter spinner and text field.
		/// </summary>
		[PublicAPI]
		public string ToolTipChapterSelector
		{
			set
			{
				this.uiToolTip.SetToolTip(this.uiChapterSpinner, value);
				this.uiToolTip.SetToolTip(this.uiChapter, value);
			}
		}

		/// <summary> 
		/// Set tooltip for the book selector.
		/// </summary>
		[PublicAPI]
		public string ToolTipBookSelector
		{
			set => this.uiToolTip.SetToolTip(this.uiBook, value);
		}

		[PublicAPI]
		public void SetContextMenuLabels(string copyLabel, string pasteLabel)
		{
			uiBook.SetContextMenuLabels(copyLabel, pasteLabel);
			uiChapter.SetContextMenuLabels(copyLabel, pasteLabel);
			uiVerse.SetContextMenuLabels(copyLabel, pasteLabel);
		}

		#endregion

		#region Internal Updating Methods

		/// <summary>
		/// (Call whenever reference has been changed internally.) Update contents of controls.
		/// </summary>
		private void UpdateControls()
		{
			try
			{
				isUpdating = true;
				uiBook.Text = curRef.Book;
				// does Book=0 have any meaning now? With new VerseRef class, how access intro material?
				uiChapter.Text = curRef.Chapter;
				uiVerse.Text = allowVerseSegments && VerseSegmentsAvailable ? curRef.UnBridge().Verse : curRef.VerseNum.ToString();
			}
			finally
			{
				isUpdating = false;
			}
		}

		/// <summary>
		/// Call whenever controls have changed to keep internal reference in sync with UI.
		/// </summary>
		private void UpdateReference()
		{
			UpdateControls(); // Many controls change themselves by changing internal reference. Nice and clean.
			OnVerseRefChanged(new PropertyChangedEventArgs("VerseRef"));
		}

		#endregion

		#region API Methods

		/// <summary>
		/// Move cursor to book field and select all text within it.
		/// </summary>
		public void GotoBookField()
		{
			uiBook.Focus();
			uiBook.SelectAll();
		}

		/// <summary>
		/// Move cursor to verse field.
		/// </summary>
		[PublicAPI]
		public void GoToVerseField()
		{
			uiVerse.Focus();
		}

		/// <summary>
		/// Tries to move to Chapter 1 verse 1 of the previous book.
		/// </summary>
		/// <returns>true if the move was possible</returns>
		[PublicAPI]
		public void PrevBook()
		{
			bool result = showEmptyBooks ? curRef.PreviousBook() : curRef.PreviousBook(booksPresentSet);
			if (result)
				UpdateReference();
		}

		[PublicAPI]
		public void NextBook()
		{
			bool result = showEmptyBooks ? curRef.NextBook() : curRef.NextBook(booksPresentSet);
			if (result)
				UpdateReference();
		}

		public void PrevChapter()
		{
			bool result = showEmptyBooks ? curRef.PreviousChapter() : curRef.PreviousChapter(booksPresentSet);
			if (result)
				UpdateReference();
		}

		public void NextChapter()
		{
			bool result = (showEmptyBooks) ? curRef.NextChapter() : curRef.NextChapter(booksPresentSet);
			if (result)
				UpdateReference();
		}

		public void PrevVerse()
		{
			bool result = (showEmptyBooks) ? curRef.PreviousVerse() : curRef.PreviousVerse(booksPresentSet);
			if (result)
				UpdateReference();
		}

		public void NextVerse()
		{
			bool result = (showEmptyBooks) ? curRef.NextVerse() : curRef.NextVerse(booksPresentSet);
			if (result)
				UpdateReference();
		}

		#endregion

		#region Control Event Methods
		/// <summary>
		/// The verse control may be used in forms that have Ctrl-V or Ctrl-C as a shortcut key on
		/// a menu item. Since the shortcut keys are processed before the control will get a
		/// KeyDown, ProcessCmdKey needs to be overwritten to get the key first. Also, some clients
		/// may need to get a chance to handle a Shift-Tab in the book field (e.g., to navigate out
		/// of this control).
		/// </summary>
		/// <param name="msg">A <see cref="T:System.Windows.Forms.Message" />, passed by reference,
		/// that represents the window message to process.</param>
		/// <param name="keyData">One of the <see cref="T:System.Windows.Forms.Keys" /> values that
		/// represents the key to process.</param>
		/// <returns><see langword="true" /> if the character was processed by the control;
		/// otherwise, <see langword="false" />.</returns>
		protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
		{
			const int WM_KEYDOWN = 0x100;
			// check to see if values can be pasted on KeyDown for CTRL-V
			if (msg.Msg == WM_KEYDOWN && keyData == (Keys.Control | Keys.V))
			{
				HandlePasteScriptureRef();
				return true; // may not have updated verse control, but treat CTRL-V as handled
			}
			// copy values to clipboard on KeyDown for CTRL-C
			if (msg.Msg == WM_KEYDOWN && keyData == (Keys.Control | Keys.C))
			{
				PortableClipboard.SetText(VerseRef.ToString());
				return true;
			}

			if (keyData == (Keys.Shift | Keys.Tab) && uiBook.ContainsFocus &&
			    ShiftTabPressedInBookField != null && IsBook())
			{
				ShiftTabPressedInBookField.Invoke(this, new KeyEventArgs(keyData));
				return true; // Mark as handled
			}

			return base.ProcessCmdKey(ref msg, keyData);
		}

		private void HandleCopy()
		{
			Clipboard.SetText(VerseRef.ToString());
		}

		private void HandlePaste()
		{
			HandlePasteScriptureRef();
		}

		private void HandlePopUpContextMenu(Object s)
		{
			// Enable paste option if needed
			if (IsValidReference(GetCleanClipboardText(), out _, out _, out _))
			{
				if (s is VCSafeComboBox comboBox)
					comboBox.EnablePaste();
				if (s is VCEnterTextBox textBox)
					textBox.EnablePaste();
			}

			// Disable tooltips when context menu pops up
			this.uiToolTip.Active = false;
		}

		private void HandleCollapseContextMenu()
		{
			// Enable tooltips when context menu collapses
			this.uiToolTip.Active = true;
		}


		/// <summary>
		/// Fires a VerseRefChangedEvent
		/// </summary>
		private void OnVerseRefChanged(PropertyChangedEventArgs e)
		{
			VerseRefChanged?.Invoke(this, e);
		}

		private void OnBooksPresentChanged(PropertyChangedEventArgs e)
		{
			InitializeBooks();
			BooksPresentChanged?.Invoke(this, e);
		}

		private void OnInvalidBook(EventArgs e)
		{
			InvalidBookEntered?.Invoke(this, e);
		}

		private void OnInvalidReference()
		{
			InvalidReferenceEntered?.Invoke(this, EventArgs.Empty);
		}

		private void OnInvalidPastedReference()
		{
			InvalidReferencePasted?.Invoke(this, EventArgs.Empty);
		}

		private void OnTextBoxGotFocus()
		{
			TextBoxGotFocus?.Invoke(this, EventArgs.Empty);
		}
		#endregion

		#region Book Owner Draw

		private void uiBook_FontChanged(object sender, EventArgs e)
		{
			abbreviationWidth = -1.0f; //signal to recalculate default width for book
		}

		//move to events section after debugging nov2007
		private void uiBook_DropDown(object sender, EventArgs e)
		{
			uiBook.SelectAll();
			Unfilter();
		}

		private void uiBook_DrawItem(object sender, DrawItemEventArgs e)
		{
			// Get the data 
			if (e.Index < 0)
				return;
			BookListItem book = uiBook.Items[e.Index] as BookListItem;
			if (book == null)
				return;
			string[] parts = book.PartsFound(searchText);

			// Get the drawing tools
			Font theFont = GetBookFont(book);
			Color theColor = book.isPresent ? e.ForeColor : EmptyBooksColor;

			// Calculate field widths if necessary
			CalcFieldWidths(e.Graphics);

			// Draw ListBox Item
			using (Font searchFont = new Font(theFont, searchTextStyle))
			{
				e.DrawBackground();
				e.DrawFocusRectangle();
				DrawBookParts(parts, e.Graphics, e.Bounds, theColor, searchFont, theFont);
			}
		}

		private Font GetBookFont(BookListItem book)
		{
			return book.isPresent ? Font : emptyBooksFont;
		}

		private void CalcFieldWidths(Graphics g)
		{
			const string measureText = "GEN";
			if (abbreviationWidth >= 1f)
				return;
			// UserControl does not have UseCompatibleTextRendering.
			var normalWidth = TextRenderer.MeasureText(g, measureText, Font).Width;
			var notPresentWidth = TextRenderer.MeasureText(g, measureText, emptyBooksFont).Width;
			abbreviationWidth = Math.Max(normalWidth, notPresentWidth) * 2.2f;
		}

		private void DrawBookParts(string[] parts, Graphics gr, Rectangle bounds, Color theColor, Font font1, Font font2)
		{
			Font theFont = font1;
			int right = bounds.Right;
			bool rtl = RightToLeft == RightToLeft.Yes;
			if (!rtl)
				bounds.X += 2;
			bounds.Width -= 2;
			for (int i = 0; i < parts.Length; i++)
			{
				string theText = parts[i];
				if (i == 2)
				{
					if (rtl)
						bounds.Width = right - (int)abbreviationWidth;
					else
						bounds.X = (int)abbreviationWidth; // tab over to name column
				}

				if (theText != "")
				{
					// UserControl does not have UseCompatibleTextRendering.
					TextFormatFlags flags = TextFormatFlags.NoPadding | TextFormatFlags.SingleLine;
					if (rtl)
						flags |= TextFormatFlags.Right;
					var size = TextRenderer.MeasureText(gr, theText, theFont, bounds.Size, flags);
					TextRenderer.DrawText(gr, theText, theFont, bounds, theColor, flags);
					if (!rtl)
						bounds.X += size.Width;
					bounds.Width -= size.Width;
				}
				theFont = Equals(theFont, font1) ? font2 : font1; // flip-flop font
			}
		}
		#endregion

		#region Autosuggest
		private void Filter()
		{
			isUpdating = true;
			//Debug.Print(string.Format("While filtering, filter = '{0}'.", searchText));
			int selectionStart = uiBook.SelectionStart;
			int selectionLength = uiBook.SelectionLength;
			// Using Items.AddRange instead of Items.Add for performance reasons on mono.
			List<BookListItem> tempList = new List<BookListItem>();
			foreach (BookListItem book in allBooks)
			{
				if (book.PassesFilter(searchText, showEmptyBooks))
					tempList.Add(book);
			}

			// Only update list if we need too. (Improves display of Dropdown list on Linux)
			if (!tempList.SequenceEqual(uiBook.Items.Cast<BookListItem>(), new BookListItemComparer()))
			{
				uiBook.BeginUpdate();
				uiBook.Items.Clear();
				uiBook.Items.AddRange(tempList.Cast<object>().ToArray());
				//ensure correct list selection?
				uiBook.EndUpdate();
				uiBook.Refresh();

				// TODO: fix mono bug where Refresh doesn't invalidate and redraw dropdown list box.
				// The following is a workaround to force mono to redraw the list box.
				if (Platform.IsMono && uiBook.DroppedDown)
				{
					FieldInfo listBoxControlField = typeof(ComboBox).GetField("listbox_ctrl", BindingFlags.Instance | BindingFlags.NonPublic);
					Debug.Assert(listBoxControlField != null, "listbox_ctrl maybe got renamed!");
					Control listBoxControl = (Control)listBoxControlField.GetValue(uiBook);
					MethodInfo setTopItemMethod = listBoxControl.GetType().GetMethod("SetTopItem", BindingFlags.Instance | BindingFlags.Public);
					setTopItemMethod.Invoke(listBoxControl, new object[] { 0 });
				}
			}



			uiBook.Select(selectionStart, selectionLength);
			isUpdating = false;
		}

		private void Unfilter()
		{
			searchText = "";
			Filter(); //removes books not present if hidden
		}

		private void uiBook_TextUpdate(object sender, EventArgs e)
		{
			if (isUpdating) return;
			if (searchText != uiBook.Text)
			{
				searchText = uiBook.Text;
				//Debug.Print(String.Format("In TextUpdate, books filter now = '{0}'.", searchText));
				Filter();
			}
			uiChapter.Text = "1";
			uiVerse.Text = "1";
		}

		#endregion

		#region Book Events

		/// <summary>
		/// Enter key updates scripture ref.
		/// </summary>
		/// <param name="e">Event args received by event handler.</param>
		private bool AcceptOnEnter(KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Enter)
			{
				uiBook.DroppedDown = false;
				Unfilter();
				try
				{
					AcceptData();

					OnVerseRefChanged(new PropertyChangedEventArgs("VerseRef"));
					return true;
				}
				catch
				{
				}
			}
			return false;
		}

		/// <summary>
		/// Call to accept data currently in the verse controls and set the current
		/// reference based on it.
		/// </summary>
		public void AcceptData()
		{
			curRef = curRef.Create(uiBook.Text, uiChapter.Text, uiVerse.Text);

			// Prevent going past last chapter/verse
			if (curRef.ChapterNum > curRef.LastChapter)
				curRef.ChapterNum = curRef.LastChapter;
			if (curRef.VerseNum > curRef.LastVerse)
				curRef.VerseNum = curRef.LastVerse;
			UpdateControls();
		}

		/// <summary>
		/// Updates the current verse ref with using the current text in the control. Will display a
		/// warning if the text cannot be parsed.
		/// </summary>
		/// <returns>true if a warning was displayed</returns>
		private bool AcceptDataWithWarning()
		{
			try
			{
				AcceptData();
				return false;
			}
			catch (VerseRefException)
			{
				OnInvalidReference();
			}
			return true;
		}

		private bool JumpOnSpace(KeyPressEventArgs e, Control nextField)
		{
			const string jumpCharacters = " :.";
			if (e.KeyChar == 13)
			{
				// Enter
				e.Handled = true; // Bell rings if we don't do this. 
			}
			else if (e.KeyChar == 27)
			{
				// Escape
				uiBook.DroppedDown = false;
				uiBook.Text = BookAbbreviation;
				uiBook.SelectAll();
			}
			else if (jumpCharacters.Contains(e.KeyChar.ToString()))
			{
				e.Handled = true;
				nextField.Focus();
				return true;
			}
			return false;
		}

		private void uiBook_SelectionChangeCommitted(object sender, EventArgs e)
		{
			if (isUpdating)
				return;
			try
			{
				if (uiBook.SelectedItem == null)
					return;
			}
			catch (ArgumentOutOfRangeException)
			{
				return;
			}

			VerseRef = curRef.Create(((BookListItem)uiBook.SelectedItem).Abbreviation, "1", "1");

			if (AdvanceToEnd)
				AdvanceToLastChapterLastVerse();
			// Save book (as focus stealing can blank this control)
			string book = uiBook.Text;
			OnVerseRefChanged(new PropertyChangedEventArgs("VerseRef"));
			uiBook.Text = book;
		}

		private void uiBook_Enter(object sender, EventArgs e)
		{
			// BeginInvoke helps Linux - otherwise clicking
			// Text in book combo doesn't select all.
			BeginInvoke(new Action(() =>
			{
				uiBook.SelectAll();
				Unfilter();
			}));
		}

		private void uiBook_KeyDown(object sender, KeyEventArgs e)
		{
			if (IsBook())
			{
				// Don't select all On Linux on Enter as combo text remains selected
				// when focus moved to the TextForm.
				if (AcceptOnEnter(e) && !Platform.IsLinux)
					uiBook.SelectAll();
			}
		}

		private string GetCleanClipboardText()
		{
			// return empty string if clipboard is empty
			if (!PortableClipboard.ContainsText())
				return "";

			// if pasting text, check to see if clipboard contain verse reference. Remove RTL and LTR marks that may be there
			// for punctuation to display in correct order.
			string text = PortableClipboard.GetText().Trim().Replace(rtlMark, "").Replace(ltrMark, "");
			// diacritics seem to have caused problems in Regex on Linux, so remove them before processing text
			return text.RemoveDiacritics();
		}

		/// <summary>
		/// Updates verse control with pasted verse reference, if pasted reference is valid.
		/// </summary>
		private void HandlePasteScriptureRef()
		{
			if (!IsValidReference(GetCleanClipboardText(), out var book, out var chapter, out var verse))
			{
				OnInvalidPastedReference();
				return;
			}

			uiBook.Text = book;
			uiChapter.Text = chapter;
			uiVerse.Text = verse;

			AcceptOnEnter(new KeyEventArgs(Keys.Enter));
		}

		/// <summary>
		/// Check to see if text is a valid reference - either using GEN 1:1 format or localized book names
		/// </summary>
		/// <param name="text"></param>
		/// <param name="book"></param>
		/// <param name="chapter"></param>
		/// <param name="verse"></param>
		/// <returns>True if string is a valid reference. If valid, it also returns the related book, chapter and verse as string output parameters.</returns>
		private bool IsValidReference(string text, out string book, out string chapter,
			out string verse)
		{
			book = chapter = verse = null;

			// check for standard reference in form: GEN 1:27
			var match = Regex.Match(text, MultilingScrBooks.VerseRefRegex);
			if (!match.Success)
				return false;

			var searchBook = match.Groups["book"].Value;
			chapter = match.Groups["chapter"].Value;
			verse = match.Groups["verse"].Value;
			// chapter and verse is optional in regex, make it 1 if not given
			if (string.IsNullOrEmpty(chapter))
				chapter ="1";
			else if (chapter.Length > 3)
				return false;
			if (string.IsNullOrEmpty(verse))
				verse = "1";
			else if (verse.Length > 3)
				return false;
			if (Canon.IsBookIdValid(searchBook))
			{
				book = searchBook;
				return true;
			}

			// search for unique entry using base name of book
			var bookItem = allBooks.OnlyOrDefault(b => b.BookMatchesSearch(searchBook, -1, VerseRef, false));
			if (bookItem == null)
			{
				int chapterNum = int.Parse(chapter);
				// take first book that starts with the search text and has the right number of chapters
				bookItem = allBooks.FirstOrDefault(b => b.BookMatchesSearch(searchBook, chapterNum, VerseRef, true));
				// take the first book that starts with the search text and has any number of chapters
				bookItem ??= allBooks.FirstOrDefault(b => b.BookMatchesSearch(searchBook, -1, VerseRef, true));
				// take the first book contains any match of search text and has any number of chapters
				bookItem ??= allBooks.FirstOrDefault(b => b.BookMatchesSearch(searchBook, -1, VerseRef, false));
			}

			if (bookItem == null)
				return false;

			book = bookItem.Abbreviation;
			return true;
		}

		/// <summary>
		/// Check whether it's okay to accept book field.
		/// </summary>
		/// <returns>True if current book field uniquely identifies a book.</returns>
		private bool IsBook()
		{
			bool result = false;
			try
			{
				string validAbbreviations = abbreviations; // "GEN.EXO.LEV.NUM.DEU...REV";
				string text = uiBook.Text;
				if (text.Length >= AbbreviationLength && validAbbreviations.Contains(text.Substring(0, 3)))
					result = true;
			}
			catch (ArgumentException)
			{
			}
			if (!result)
				OnInvalidBook(EventArgs.Empty);
			return result;
		}

		private bool IsBookChar(char c)
		{
			try
			{
				// True if character should be accepted
				if (c == 8)
					return true; //allow backspace
				if (uiBook.Text.Length >= AbbreviationLength && uiBook.SelectionLength == 0)
					return false;
				if (IsLetterOrDigit(c))
					return true;
			}
			catch (ArgumentException)
			{
			}
			return false;
		}

		private void uiBook_KeyPress(object sender, KeyPressEventArgs e)
		{
			// FUTURE: accept menu and system key shortcuts (right now masks Alt-F4 for instance
			e.KeyChar = ToUpperInvariant(e.KeyChar); // force book names uppercase
			if (IsBook()) // Only allow movement to next field if book ok
				if (JumpOnSpace(e, uiChapter))
				{
					if (AdvanceToEnd)
						AdvanceToLastChapterLastVerse();
					return;
				}
			if (uiBook.Items.Count == 1 && e.KeyChar == ' ') // Only allow movement to next field if book is only choice
			{
				uiBook.Text = ((BookListItem)uiBook.Items[0]).Abbreviation;
				uiBook.DroppedDown = false;
				e.Handled = true;
				if (AdvanceToEnd)
					AdvanceToLastChapterLastVerse();
				uiChapter.Focus();
				return;
			}

			if (!IsBookChar(e.KeyChar))
			{
				//Don't allow typing if too long or not alphanumeric
				e.Handled = true;
				return;
			}

			if (uiBook.Items.Count != 0)
				uiBook.DroppedDown = true;
		}

		private void AdvanceToLastChapterLastVerse()
		{
			IScrVerseRef vRef = curRef.Create(uiBook.Text, "1", "1");
			vRef.ChapterNum = vRef.LastChapter;
			vRef.VerseNum = vRef.LastVerse;
			vRef.AdvanceToLastSegment();
			uiChapter.Text = vRef.Chapter;
			uiVerse.Text = vRef.Verse;
			curRef = vRef;
		}

		private void AdvanceToLastVerse()
		{
			IScrVerseRef vRef = curRef.Create(uiBook.Text, uiChapter.Text, "1");
			vRef.VerseNum = vRef.LastVerse;
			vRef.AdvanceToLastSegment();
			uiVerse.Text = vRef.Verse;
			curRef = vRef;
		}

		private void Revert()
		{
			// revert book to previous value
			UpdateControls();
		}

		private void uiBook_Leave(object sender, EventArgs e)
		{
			Unfilter();
			if (!IsBook())
				Revert();
		}

		/// <summary>
		/// Leave events don't arrive reliability on Linux.
		/// So we listen for the LostFocus event too.
		/// </summary>
		private void uiBook_LostFocus(object sender, EventArgs e)
		{
			if (Platform.IsWindows)
				return;

			uiBook_Leave(sender, e);
		}

		#endregion

		#region Navigation Events (Chapter and Verse Event Handling)

		private void uiChapterPrev_Click(object sender, EventArgs e)
		{
			if (AcceptDataWithWarning())
				return;
			PrevChapter();
			if (AdvanceToEnd)
				AdvanceToLastVerse();
		}

		private void uiChapterNext_Click(object sender, EventArgs e)
		{
			if (AcceptDataWithWarning())
				return;
			NextChapter();
			if (AdvanceToEnd)
				AdvanceToLastVerse();
		}

		private void uiChapter_Enter(object sender, EventArgs e)
		{
			uiChapter.SelectAll();
		}

		private void uiChapter_KeyDown(object sender, KeyEventArgs e)
		{
			if (AcceptOnEnter(e))
			{
				uiChapter.SelectAll();
				if (AdvanceToEnd)
					AdvanceToLastVerse();
			}
		}

		private void uiChapter_KeyPress(object sender, KeyPressEventArgs e)
		{
			if (!OkChapterInput(uiChapter, e, uiVerse))
				e.Handled = true;
			//if (JumpOnSpace(e, uiVerse)) return;
			//if (uiChapter.SelectionLength > 0) return; //allow overtype
			//if (!OkNNN( e.KeyChar, uiChapter.Text)) e.Handled = true; //swallow key if not valid for chapter
		}

		private void control_GotFocus(object sender, EventArgs e)
		{
			OnTextBoxGotFocus();
		}

		/// <summary>
		/// Returns false if we ought to ignore the keypress because it will do bad things to the verse ref
		/// </summary>
		/// <param name="textbox"></param>
		/// <param name="e"></param>
		/// <param name="nextField"></param>
		/// <returns></returns>
		private bool OkChapterInput(TextBox textbox, KeyPressEventArgs e, Control nextField)
		{
			char c = e.KeyChar;
			if (JumpOnSpace(e, nextField))
				return true; // handle jump on space behavior
			if (c == 8)
				return true; //always allow backspace
			//TODO: segmented verses allow letters as last char of verse
			if (!IsDigit(c))
				return false; //expect digits from here on out
			if (textbox.SelectionLength > 0)
				return true; //allow overtype
			if (textbox.Text.Length >= 3)
				return false; // limit to 3 digits for ch and vs
			return true;
		}

		private void uiVersePrev_Click(object sender, EventArgs e)
		{
			if (AcceptDataWithWarning())
				return;
			PrevVerse();
		}

		private void uiVerseNext_Click(object sender, EventArgs e)
		{
			if (AcceptDataWithWarning())
				return;
			NextVerse();
		}

		private void uiVerse_Enter(object sender, EventArgs e)
		{
			uiVerse.SelectAll();
			uiVerse.OtherKeysToTreatAsInputKeys = TabKeyPressedInVerseField == null ? null :
					new [] {Keys.Tab};
		}

		private void uiVerse_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Tab)
			{
				if (TabKeyPressedInVerseField != null)
				{
					TabKeyPressedInVerseField.Invoke(this, e);
					if (e.SuppressKeyPress)
						e.Handled = true;
				}
			}
			else if (AcceptOnEnter(e))
				uiVerse.SelectAll();
		}

		private void uiVerse_KeyPress(object sender, KeyPressEventArgs e)
		{
			if (!OkVerseInput(uiVerse, e, uiBook))
				e.Handled = true; //swallow key if not valid for verse
		}

		private bool OkVerseInput(TextBox textbox, KeyPressEventArgs e, Control nextField)
		{
			char c = e.KeyChar;
			if (JumpOnSpace(e, nextField))
				return true; // handle jump on space behavior
			if (c == 8)
				return true; //always allow backspace
			if (allowVerseSegments && VerseSegmentsAvailable)
			{
				if (!IsLetterOrDigit(c))
					return false;
			}
			else if (!IsDigit(c))
				return false;

			if (textbox.SelectionLength > 0)
				return true; //allow overtype
			if (textbox.Text.Length >= 3)
				return false; // limit to 3 digits for ch and vs
			if (textbox.Text.Length > 0)
			{
				if (IsLetter(textbox.Text[textbox.Text.Length - 1]))
					return false; //no typing if last char is a letter
			}
			else if (IsLetter(c))
				return false; //can't start with letter
			return true;
		}


		#endregion

		#region BookListItem

		/// <summary>
		/// Holds book information in a way that supports the verse control.
		/// </summary>
		private sealed class BookListItem
		{
			// Instance Fields
			public readonly string Abbreviation; // standard 3-letter all caps abbreviation
			public string Name; // localized name
			public string BaseName; // upper-case localized name without diacritics
			public bool isPresent; // true if book has data
			private int lastChapter = -1;

			public override string ToString()
			{
				return Abbreviation;
			}

			public BookListItem(string abbreviation)
			{
				Abbreviation = abbreviation;
			}

			/// <summary>
			/// Split book description and name into 4 parts (2 each):
			/// The first of each pair including any text that matches the current search string
			/// (What the user has typed into the book control).
			/// 
			/// Example: 
			/// Current Book: Genesis
			/// Current Search Text: GE (the user has typed the first two letters of the book into the text box)
			/// Returns: {"GE", "N", "Ge", "nesis"}
			/// 
			/// Purpose:
			/// To simplify displaying highlight search text in the list of books.
			/// </summary>
			/// <param name="searchText">0-3 letters indicating current letters we ar searching for among books</param>
			/// <returns>String[4] = {Abbreviation Found Part, Abbreviation Rest, Name </returns>
			public string[] PartsFound(string searchText)
			{
				string[] result = { "", "", "", "" };
				result[1] = Abbreviation; // default to nothing found, so [0] = "" and whole abbreviation is in [1]
				result[3] = Name;
				int len = searchText.Length;
				if (len < 1)
					return result;
				if (Abbreviation.StartsWith(searchText, StringComparison.Ordinal))
				{
					// break out matching initial letters
					result[0] = Abbreviation.Substring(0, len);
					result[1] = Abbreviation.Substring(len);
				}
				if (BaseName.StartsWith(searchText, StringComparison.Ordinal))
				{
					//Note the subtle swap: we test on baseName, but we return the name with diacritics.
					//Therefore, the names have to be fully composed.
					result[2] = Name.Substring(0, len);
					result[3] = Name.Substring(len);
				}
				return result;
			}

			public bool PassesFilter(string startsWith, bool showEmptyBooks)
			{
				if (!showEmptyBooks && !isPresent)
					return false;
				if (startsWith == "")
					return true; //everything passes a blank filter
				if (Abbreviation.StartsWith(startsWith, StringComparison.Ordinal))
					return true;
				if (BaseName.StartsWith(startsWith, StringComparison.Ordinal))
					return true;
				return false;
			}

			internal bool BookMatchesSearch(string searchText, int chapterNum, IScrVerseRef verseRef, bool mustStartWithText)
			{
				if (BaseName.Length < searchText.Length)
					return false;
				if (!Regex.IsMatch(BaseName, $"{(mustStartWithText ? "^" : "\\b")}{Regex.Escape(searchText)}", RegexOptions.IgnoreCase))
					return false;

				if (chapterNum == -1)
					return true;

				if (lastChapter == -1)
				{
					var lastChapterRef = verseRef.Clone();
					lastChapterRef.BookNum = Canon.BookIdToNumber(Abbreviation);
					lastChapter = lastChapterRef.LastChapter;
				}

				return lastChapter >= chapterNum;
			}
		}
		#endregion

		#region BookListItemComparer

		private class BookListItemComparer : IEqualityComparer<BookListItem>
		{
			#region IEqualityComparer[BookListItem] implementation
			public bool Equals(BookListItem x, BookListItem y)
			{
				return x.Abbreviation == y.Abbreviation && x.BaseName == y.BaseName;
			}

			public int GetHashCode(BookListItem obj)
			{
				return obj.BaseName.GetHashCode();
			}
			#endregion

		}

		#endregion

		private void uiChapter_MouseDown(object sender, MouseEventArgs e)
		{
			uiChapter.SelectAll();
		}

		private void uiVerse_MouseDown(object sender, MouseEventArgs e)
		{
			uiVerse.SelectAll();
		}

		/// <summary>
		/// ContextMenu with internationalized copy and paste menu items.
		/// </summary>
		private class CopyPasteContextMenu : ContextMenu
		{
			private const int kCopy = 0;
			private const int kPaste = 1;

			public CopyPasteContextMenu(EventHandler copyHandler, EventHandler pasteHandler)
			{
				MenuItems.Add(new MenuItem(GetLocalizedMenuText(kCopy), copyHandler));
				MenuItems.Add(new MenuItem(GetLocalizedMenuText(kPaste), pasteHandler));
			}

			public MenuItem PasteMenu => MenuItems[kPaste];

			private static string GetLocalizedMenuText(int menu)
			{
				// Don't want to break Paratext, which uses its own localization strategy.
				var restore = LocalizationManager.StrictInitializationMode;
				LocalizationManager.StrictInitializationMode = false;
				string text;
				try
				{
					switch (menu)
					{
						case kCopy:
							text = LocalizationManager.GetString("VerseControl.ContextMenu.Copy",
								"Copy");

							break;
						case kPaste:
							text = LocalizationManager.GetString("VerseControl.ContextMenu.Paste",
								"Paste");
							break;
						default:
							throw new ArgumentOutOfRangeException(nameof(menu));
					}
				}
				finally
				{
					LocalizationManager.StrictInitializationMode = restore;
				}

				return text;
			}

			/// <summary>
			/// Gets or sets the text for the copy and paste menu items. This is mainly intended to
			/// allow for localization for clients that do not use L10nSharp.
			/// </summary>
			public void SetMenuLabels(string copyLabel, string pasteLabel)
			{
				MenuItems[kCopy].Text = copyLabel;
				PasteMenu.Text = pasteLabel;
			}
		}

		/// <summary>
		/// Variant of the SafeComboBox that has a custom copy/paste context menu,
		/// and that fires events when this context menu is opened,
		/// and when a copy/paste action is triggered from it.
		/// </summary>
		private class VCSafeComboBox : SafeComboBox
		{
			public event NoArgsDelegate CopyEvent;
			public event NoArgsDelegate PasteEvent;
			public event OneArgDelegate PopUpEvent;
			public event NoArgsDelegate CollapseEvent;

			private readonly CopyPasteContextMenu _contextMenu;

			public VCSafeComboBox()
			{
				_contextMenu = new CopyPasteContextMenu((s, e) => CopyEvent?.Invoke(),
					(s, e) => PasteEvent?.Invoke());
				_contextMenu.Popup += PopUpContextMenu;
				_contextMenu.Collapse += (s, e) => CollapseEvent?.Invoke();
			}

			protected override void OnHandleCreated(EventArgs e)
			{
				base.OnHandleCreated(e);
				ContextMenu = _contextMenu;
			}

			private void PopUpContextMenu(Object s, EventArgs e)
			{
				_contextMenu.PasteMenu.Enabled = false;

				PopUpEvent?.Invoke(this);
			}

			public void EnablePaste()
			{
				_contextMenu.PasteMenu.Enabled = true;
			}

			/// <summary>
			/// Gets or sets the text for the copy and paste menu items in the context menu. This
			/// is mainly intended to allow for localization for clients that do not use L10nSharp.
			/// </summary>
			public void SetContextMenuLabels(string copyLabel, string pasteLabel)
			{
				_contextMenu.SetMenuLabels(copyLabel, pasteLabel);
			}
		}

		/// <summary>
		/// Variant of the EnterTextBox that has a custom copy/paste context menu,
		/// and that fires events when this context menu is opened,
		/// and when a copy/paste action is triggered from it.
		/// </summary>
		private class VCEnterTextBox : EnterTextBox
		{
			public event NoArgsDelegate CopyEvent;
			public event NoArgsDelegate PasteEvent;
			public event OneArgDelegate PopUpEvent;
			public event NoArgsDelegate CollapseEvent;

			private readonly CopyPasteContextMenu _contextMenu;

			public VCEnterTextBox()
			{
				_contextMenu = new CopyPasteContextMenu((s, e) => CopyEvent?.Invoke(),
					(s, e) => PasteEvent?.Invoke());
				_contextMenu.Popup += PopUpContextMenu;
				_contextMenu.Collapse += (s, e) => CollapseEvent?.Invoke();
			}

			protected override void OnHandleCreated(EventArgs e)
			{
				base.OnHandleCreated(e);
				ContextMenu = _contextMenu;
			}


			private void PopUpContextMenu(Object s, EventArgs e)
			{
				_contextMenu.PasteMenu.Enabled = false;

				PopUpEvent?.Invoke(this);
			}

			public void EnablePaste()
			{
				_contextMenu.PasteMenu.Enabled = true;
			}

			public void SetContextMenuLabels(string copyLabel, string pasteLabel)
			{
				_contextMenu.SetMenuLabels(copyLabel, pasteLabel);
			}
		}
	}
}
