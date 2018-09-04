using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using SIL.PlatformUtilities;
using SIL.Scripture;
using SIL.Extensions;

namespace SIL.Windows.Forms.Scripture
{
	/// <summary>
	/// Control that allows the specifying of a book, chapter and verse
	/// in the same style as Paratext.
	/// </summary>
	public partial class VerseControl : UserControl
	{
		#region Fields and Constructors

		private const FontStyle searchTextStyle = FontStyle.Bold;
		const int AbbreviationLength = 3; // length of book abbreviation
		BookSet booksPresentSet = new BookSet();
		string abbreviations = "";
		string searchText = "";
		bool allowVerseSegments = true; //Allow LXX style lettered verse segments
		bool showEmptyBooks = true;
		VerseRef curRef;
		float abbreviationWidth = -1.0f; //starting width of book abbreviation, <0 signifies to recalculate
		readonly BookListItem[] allBooks;
		Font emptyBooksFont = SystemFonts.DefaultFont;
		Color emptyBooksColor = SystemColors.GrayText;

		bool advanceToEnd;

		/// <summary>Function that can be set to allow the control to get localized names for the books.</summary>
		public Func<string, string> GetLocalizedBookName = Canon.BookIdToEnglishName;

		// change events
		/// <summary>Fired when the reference is changed</summary>
		public event PropertyChangedEventHandler VerseRefChanged;
		/// <summary>Fired when the books listed in the control change</summary>
		public event PropertyChangedEventHandler BooksPresentChanged;
		/// <summary>Fired when the versification is changed</summary>
		public event PropertyChangedEventHandler VersificationChanged;
		/// <summary>Fired when an invalid book is entered</summary>
		public event EventHandler InvalidBookEntered;
		/// <summary>Fired when any part of the reference is invalid</summary>
		public event EventHandler InvalidReferenceEntered;
		/// <summary>Fired when any textbox for the reference gets focus</summary>
		public event EventHandler TextBoxGotFocus;

		// Used to temporarily ignore changes in the index of the book control
		// when it is being updated internally
		bool isUpdating;

		public VerseControl()
		{
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();

			if (Platform.IsLinux)
			{
				// Set a smaller font on Linux. (Stops 'J''s being clipped)
				uiBook.Font = new Font("Segoe UI", 8F, FontStyle.Regular, GraphicsUnit.Point, 0);

				// Also increase the ItemHeight as to stop clipping of items in the drop down menu..
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

			// Cause Drop down list to be initally populated.
			// (stop intial click on Combo box drop down beinging ignored on mono)
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

		void InitializeBooks()
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

		void PopulateBooks()
		{
			Unfilter();
		}

		#endregion

		#region Properties

		[Browsable(true)]
		public bool AllowVerseSegments
		{
			get { return allowVerseSegments; }
			set
			{
				if (allowVerseSegments == value)
					return; //no op
				//FUTURE: if (!value) //turning off segments, remove segment portion, fire event if change ref
				allowVerseSegments = value;
			}
		}

		bool VerseSegmentsAvailable
		{
			get { return curRef.Versification != null && curRef.Versification.HasVerseSegments; }
		}

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
		public VerseRef VerseRef
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
				return new VerseRef(curRef).UnBridge();
			}
			set
			{
				// We really can't use Valid here because it checks for maximum book number
				// and that is depenedent on the versification and when the versification of
				// the control and the text are different we can end up ignoring values
				// that are in fact ok. So we just check for empty references and bail out
				// if we get one.
				if (value.BookNum < 1 || value.ChapterNum < 1 || value.VerseNum < 0)
				{
					Trace.TraceWarning("Invalid VerseRef passed to VerseControl: " + value);
					// Ignore it for now (see PT-1310)
					return;
				}

				curRef.CopyFrom(value);
				if (!allowVerseSegments)
					curRef.Simplify();

				if (advanceToEnd)
					AdvanceToLastSegment(curRef);

				UpdateControls();
			}
		}

		[Browsable(false)]
		private string BookAbbreviation
		{
			get { return curRef.Book; }
		}

		[Browsable(false), ReadOnly(true)]
		public ScrVers Versification
		{
			set
			{
				if (curRef.Versification == value)
					return;
				curRef.Versification = value;
				OnVersificationChanged(new PropertyChangedEventArgs("Versification"));
			}
		}

		[Browsable(false), ReadOnly(true)]
		public BookSet BooksPresentSet
		{
			set
			{
				booksPresentSet = value;
				OnBooksPresentChanged(new PropertyChangedEventArgs("BooksPresent"));
			}
		}

		public Font EmptyBooksFont
		{
			get { return emptyBooksFont; }
			set
			{
				if (Equals(value, emptyBooksFont))
					return;
				emptyBooksFont = value ?? SystemFonts.DefaultFont;
				abbreviationWidth = -1.0f;
			}
		}

		public Color EmptyBooksColor
		{
			get { return emptyBooksColor; }
			set { emptyBooksColor = value; }
		}

		public bool ShowEmptyBooks
		{
			get { return showEmptyBooks; }
			set
			{
				if (showEmptyBooks == value) return;
				showEmptyBooks = value;
				PopulateBooks();
			}
		}

		/// <summary>
		/// When using a verse control to indicate the end of a range, chosing
		/// a new books should go to the last verse of the last chapter. Chosing
		/// a new chapter should go to the last verse of that chapter.
		/// </summary>
		public bool AdvanceToEnd
		{
			get { return advanceToEnd; }
			set { advanceToEnd = value; }
		}

		#endregion

		#region Internal Updating Methods

		/// <summary>
		/// (Call whenever reference has been changed internally.) Update contents of controls.
		/// </summary>
		void UpdateControls()
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
		void UpdateReference()
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
		/// Tries to move to Chapter 1 verse 1 of the previous book.
		/// </summary>
		/// <returns>true if the move was possible</returns>
		public void PrevBook()
		{
			bool result = (showEmptyBooks) ? curRef.PreviousBook() : curRef.PreviousBook(booksPresentSet);
			if (result)
				UpdateReference();
		}

		public void NextBook()
		{
			bool result = (showEmptyBooks) ? curRef.NextBook() : curRef.NextBook(booksPresentSet);
			if (result)
				UpdateReference();
		}

		public void PrevChapter()
		{
			bool result = (showEmptyBooks) ? curRef.PreviousChapter() : curRef.PreviousChapter(booksPresentSet);
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
		/// Fires a VerseRefChangedEvent
		/// </summary>
		private void OnVerseRefChanged(PropertyChangedEventArgs e)
		{
			if (VerseRefChanged != null)
				VerseRefChanged(this, e);
		}

		private void OnBooksPresentChanged(PropertyChangedEventArgs e)
		{
			InitializeBooks();
			if (BooksPresentChanged != null)
				BooksPresentChanged(this, e);
		}

		private void OnVersificationChanged(PropertyChangedEventArgs e)
		{
			if (VersificationChanged != null)
				VersificationChanged(this, e);
		}

		private void OnInvalidBook(EventArgs e)
		{
			if (InvalidBookEntered != null)
				InvalidBookEntered(this, e);
		}

		private void OnInvalidReference()
		{
			if (InvalidReferenceEntered != null)
				InvalidReferenceEntered(this, EventArgs.Empty);
		}

		private void OnTextBoxGotFocus()
		{
			if (TextBoxGotFocus != null)
				TextBoxGotFocus(this, EventArgs.Empty);
		}
		#endregion

		#region Book Owner Draw

		void uiBook_FontChanged(object sender, EventArgs e)
		{
			abbreviationWidth = -1.0f; //signal to recalculate default width for book
		}

		//move to events section after debugging nov2007
		void uiBook_DropDown(object sender, EventArgs e)
		{
			uiBook.SelectAll();
			Unfilter();
		}

		void uiBook_DrawItem(object sender, DrawItemEventArgs e)
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
			Color theColor = book.isPresent ? e.ForeColor : emptyBooksColor;

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

		Font GetBookFont(BookListItem book)
		{
			return book.isPresent ? Font : emptyBooksFont;
		}

		void CalcFieldWidths(Graphics g)
		{
			const string measureText = "GEN";
			if (abbreviationWidth >= 1f)
				return;
			// UserControl does not have UseCompatibleTextRendering.
			var normalWidth = TextRenderer.MeasureText(g, measureText, Font).Width;
			var notPresentWidth = TextRenderer.MeasureText(g, measureText, emptyBooksFont).Width;
			abbreviationWidth = Math.Max(normalWidth, notPresentWidth) * 2.2f;
		}

		void DrawBookParts(string[] parts, Graphics gr, Rectangle bounds, Color theColor, Font font1, Font font2)
		{
			Font theFont = font1;
			for (int i = 0; i < parts.Length; i++)
			{
				string theText = parts[i];
				if (i == 2)
					bounds.X = (int)abbreviationWidth; // tab over to name column
				if (theText != "")
				{
					// UserControl does not have UseCompatibleTextRendering.
					var size = TextRenderer.MeasureText(gr, theText, theFont, bounds.Size, TextFormatFlags.NoPadding|TextFormatFlags.WordBreak);
					TextRenderer.DrawText(gr, theText, theFont, bounds, theColor, TextFormatFlags.NoPadding|TextFormatFlags.WordBreak);
					// REVIEW/TEST: does using TextRenderer instead of Graphics remove the need for this fudge factor?
					int width = size.Width - (Platform.IsWindows ? 3 : 0); //Nudge factor: width seems to come off too big (Not on Linux)
					bounds.X += width;
					bounds.Width -= width;
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

			var currentItems = uiBook.Items.Cast<BookListItem>();
			// Only update list if we need too. (Impoves display of Dropdown list on Linux)
			if (tempList.Except(currentItems, new BookListItemComparer()).Any() || currentItems.Except(tempList, new BookListItemComparer()).Any())
			{
				uiBook.BeginUpdate();
				uiBook.Items.Clear();
				uiBook.Items.AddRange(tempList.ToArray());
				//ensure correct list selection?
				uiBook.EndUpdate();
				uiBook.Refresh();

				// TODO: fix mono bug where Refresh doesn't invalidate and redraw dropdown list box.
				// The following is a work around to force mono to redraw the list box.
				if (Platform.IsMono && uiBook.DroppedDown)
				{
					FieldInfo listBoxControlField = typeof(ComboBox).GetField("listbox_ctrl", BindingFlags.Instance | BindingFlags.NonPublic);
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

		void uiBook_TextUpdate(object sender, EventArgs e)
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
		bool AcceptOnEnter(KeyEventArgs e)
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
			curRef = new VerseRef(uiBook.Text + " " + uiChapter.Text + ":" + uiVerse.Text, curRef.Versification);

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

		bool JumpOnSpace(KeyPressEventArgs e, Control nextField)
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

		void uiBook_SelectionChangeCommitted(object sender, EventArgs e)
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
			ScrVers versif = ScrVers.English;
			if (!curRef.IsDefault)
				versif = curRef.Versification;

			VerseRef = new VerseRef(((BookListItem)uiBook.SelectedItem).Abbreviation + " 1:1", versif);

			if (advanceToEnd)
				AdvanceToLastChapterLastVerse();
			// Save book (as focus stealing can blank this control)
			string book = uiBook.Text;
			OnVerseRefChanged(new PropertyChangedEventArgs("VerseRef"));
			uiBook.Text = book;
		}

		void uiBook_Enter(object sender, EventArgs e)
		{
			// BeginInvoke helps Linux - otherwise clicking
			// Text in book combo doesn't select all.
			BeginInvoke(new Action(() =>
			{
				uiBook.SelectAll();
				Unfilter();
			}));
		}

		void uiBook_KeyDown(object sender, KeyEventArgs e)
		{
			// Don't select all On Linux on Enter as combo text remains selected
			// when focus moved to the TextForm.
			if (IsBook())
				if (AcceptOnEnter(e) && !Platform.IsLinux)
					uiBook.SelectAll();
		}

		/// <summary>
		/// Check whether it's okay to accept book field.
		/// </summary>
		/// <returns>True if current book field uniquely identifies a book.</returns>
		bool IsBook()
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

		bool IsBookChar(char c)
		{
			try
			{
				// True if character should be accepted
				if (c == 8)
					return true; //allow backspace
				if (uiBook.Text.Length >= AbbreviationLength && uiBook.SelectionLength == 0)
					return false;
				if (Char.IsLetterOrDigit(c))
					return true;
			}
			catch (ArgumentException)
			{
			}
			return false;
		}

		void uiBook_KeyPress(object sender, KeyPressEventArgs e)
		{
			// FUTURE: accept menu and system key shortcuts (right now masks Alt-F4 for instance
			e.KeyChar = Char.ToUpperInvariant(e.KeyChar); // force book names uppercase
			if (IsBook()) // Only allow movement to next field if book ok
				if (JumpOnSpace(e, uiChapter))
				{
					if (advanceToEnd)
						AdvanceToLastChapterLastVerse();
					return;
				}
			if (uiBook.Items.Count == 1 && e.KeyChar == ' ') // Only allow movement to next field if book is only choice
			{
				uiBook.Text = ((BookListItem)uiBook.Items[0]).Abbreviation;
				uiBook.DroppedDown = false;
				e.Handled = true;
				if (advanceToEnd)
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

		void AdvanceToLastChapterLastVerse()
		{
			VerseRef vref = new VerseRef(uiBook.Text + " 1:1");
			vref.Versification = curRef.Versification;
			vref.ChapterNum = vref.LastChapter;
			vref.VerseNum = vref.LastVerse;
			AdvanceToLastSegment(vref);
			uiChapter.Text = vref.Chapter;
			uiVerse.Text = vref.Verse;
			curRef.CopyFrom(vref);
		}

		private static void AdvanceToLastSegment(VerseRef vref)
		{
			string[] segments = vref.GetSegments(null);
			if (segments != null && segments.Length > 0)
				vref.Verse += segments[segments.Length - 1];
		}

		void AdvanceToLastVerse()
		{
			VerseRef vref = new VerseRef(uiBook.Text + " " + uiChapter.Text + ":1");
			vref.Versification = curRef.Versification;
			vref.VerseNum = vref.LastVerse;
			AdvanceToLastSegment(vref);
			uiVerse.Text = vref.Verse;
			curRef.CopyFrom(vref);
		}

		void Revert()
		{
			// revert book to previous value
			UpdateControls();
		}

		void uiBook_Leave(object sender, EventArgs e)
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

		void uiChapterPrev_Click(object sender, EventArgs e)
		{
			if (AcceptDataWithWarning())
				return;
			PrevChapter();
			if (advanceToEnd)
				AdvanceToLastVerse();
		}

		void uiChapterNext_Click(object sender, EventArgs e)
		{
			if (AcceptDataWithWarning())
				return;
			NextChapter();
			if (advanceToEnd)
				AdvanceToLastVerse();
		}

		void uiChapter_Enter(object sender, EventArgs e)
		{
			uiChapter.SelectAll();
		}

		void uiChapter_KeyDown(object sender, KeyEventArgs e)
		{
			if (AcceptOnEnter(e))
			{
				uiChapter.SelectAll();
				if (advanceToEnd)
					AdvanceToLastVerse();
			}
		}

		void uiChapter_KeyPress(object sender, KeyPressEventArgs e)
		{
			if (!OkChapterInput(uiChapter, e, uiVerse))
				e.Handled = true;
			//if (JumpOnSpace(e, uiVerse)) return;
			//if (uiChapter.SelectionLength > 0) return; //allow overtype
			//if (!OkNNN( e.KeyChar, uiChapter.Text)) e.Handled = true; //swallow key if not valid for chapter
		}

		void control_GotFocus(object sender, EventArgs e)
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
		bool OkChapterInput(TextBox textbox, KeyPressEventArgs e, Control nextField)
		{
			char c = e.KeyChar;
			if (JumpOnSpace(e, nextField))
				return true; // handle jump on space behavior
			if (c == 8)
				return true; //always allow backspace
			//TODO: segmented verses allow letters as last char of verse
			if (!Char.IsDigit(c))
				return false; //expect digits from here on out
			if (textbox.SelectionLength > 0)
				return true; //allow overtype
			if (textbox.Text.Length >= 3)
				return false; // limit to 3 digits for ch and vs
			return true;
		}

		void uiVersePrev_Click(object sender, EventArgs e)
		{
			if (AcceptDataWithWarning())
				return;
			PrevVerse();
		}

		void uiVerseNext_Click(object sender, EventArgs e)
		{
			if (AcceptDataWithWarning())
				return;
			NextVerse();
		}

		void uiVerse_Enter(object sender, EventArgs e)
		{
			uiVerse.SelectAll();
		}

		void uiVerse_KeyDown(object sender, KeyEventArgs e)
		{
			if (AcceptOnEnter(e)) uiVerse.SelectAll();
		}

		void uiVerse_KeyPress(object sender, KeyPressEventArgs e)
		{
			if (!OkVerseInput(uiVerse, e, uiBook))
				e.Handled = true; //swallow key if not valid for verse
		}

		bool OkVerseInput(TextBox textbox, KeyPressEventArgs e, Control nextField)
		{
			char c = e.KeyChar;
			if (JumpOnSpace(e, nextField))
				return true; // handle jump on space behavior
			if (c == 8)
				return true; //always allow backspace
			if (allowVerseSegments && VerseSegmentsAvailable)
			{
				if (!Char.IsLetterOrDigit(c))
					return false;
			}
			else if (!Char.IsDigit(c))
				return false;

			if (textbox.SelectionLength > 0)
				return true; //allow overtype
			if (textbox.Text.Length >= 3)
				return false; // limit to 3 digits for ch and vs
			if (textbox.Text.Length > 0)
			{
				if (Char.IsLetter(textbox.Text[textbox.Text.Length - 1]))
					return false; //no typing if last char is a letter
			}
			else if (Char.IsLetter(c))
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
		}
		#endregion

		#region BookListItemComparer

		class BookListItemComparer : IEqualityComparer<BookListItem>
		{
			#region IEqualityComparer[BookListItem] implementation
			public bool Equals(BookListItem x, BookListItem y)
			{
				return (x.Abbreviation == y.Abbreviation && x.BaseName == y.BaseName);
			}

			public int GetHashCode(BookListItem obj)
			{
				return obj.BaseName.GetHashCode();
			}
			#endregion

		}

		#endregion

		void uiChapter_MouseDown(object sender, MouseEventArgs e)
		{
			uiChapter.SelectAll();
		}

		void uiVerse_MouseDown(object sender, MouseEventArgs e)
		{
			uiVerse.SelectAll();
		}

	}
}
