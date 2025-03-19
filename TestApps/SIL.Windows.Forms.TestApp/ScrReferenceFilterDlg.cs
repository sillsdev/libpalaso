// Copyright (c) 2025 SIL Global
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using System;
using System.ComponentModel;
using System.Windows.Forms;
using SIL.Scripture;
using SIL.Windows.Forms.Scripture;

namespace SIL.Windows.Forms.TestApp
{
	/// ----------------------------------------------------------------------------------------
	/// <summary>
	/// Dialog to allow user to select a reference range on which to filter questions.
	/// </summary>
	/// ----------------------------------------------------------------------------------------
	public partial class ScrReferenceFilterDlg : Form
	{
		#region Data members
		readonly ScrVers m_versification = ScrVers.English;
		#endregion

		#region Constructor and initialization methods
		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Initializes a new instance of the <see cref="T:ScrReferenceFilterDlg"/> class.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		internal ScrReferenceFilterDlg()
		{
			InitializeComponent();
			scrPsgTo.VerseControl.VerseRefChanged += ScrPassageChanged;
			scrPsgFrom.VerseControl.VerseRefChanged += ScrPassageChanged;

			scrPsgFrom.VerseControl.BooksPresentSet = scrPsgTo.VerseControl.BooksPresentSet =
				new BookSet(1, MultilingScrBooks.LastBook);
			scrPsgFrom.VerseControl.ShowEmptyBooks = false;
			scrPsgTo.VerseControl.ShowEmptyBooks = false;
			scrPsgFrom.VerseControl.VerseRef = new VerseRef(001001001, m_versification);
			scrPsgTo.VerseControl.VerseRef = new VerseRef(066005008, m_versification);
			
			scrPsgFrom.VerseControl.TabKeyPressedInVerseField += HandleTabKeyInVerseField;
			scrPsgTo.VerseControl.TabKeyPressedInVerseField += HandleTabKeyInVerseField;
			scrPsgFrom.VerseControl.ShiftTabPressedInBookField += HandleShiftTabInBookField;
			scrPsgTo.VerseControl.ShiftTabPressedInBookField += HandleShiftTabInBookField;

			scrPsgTo.VerseControl.SetContextMenuLabels("Simulated Paratext Override of Copy",
				"Simulated Paratext Override of Paste");
		}

		protected override void OnShown(EventArgs e)
		{
			base.OnShown(e);

			scrPsgFrom.VerseControl.GotoBookField();
		}

		private void HandleTabKeyInVerseField(object sender, KeyEventArgs e)
		{
			var controlToFocus = sender == scrPsgFrom.VerseControl ? scrPsgTo : scrPsgFrom;
			controlToFocus.Focus();
			e.SuppressKeyPress = true;
		}

		private void HandleShiftTabInBookField(object sender, KeyEventArgs e)
		{
			var controlToFocus = sender == scrPsgFrom.VerseControl ? scrPsgTo : scrPsgFrom;
			controlToFocus.VerseControl.GoToVerseField();
			e.SuppressKeyPress = true;
		}
		#endregion

		#region Event handlers
		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Handles leaving the "to" or "from" passage
		/// </summary>
		/// ------------------------------------------------------------------------------------
		private void OnScrPassageLeave(object sender, EventArgs e)
		{
			var psgCtrl = (ToolStripVerseControl)sender;
			psgCtrl.VerseControl.AcceptData();
			ScrPassageChanged(sender, new PropertyChangedEventArgs(psgCtrl.Name));
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Handles change in the "to" or "from" passage
		/// </summary>
		/// ------------------------------------------------------------------------------------
		private void ScrPassageChanged(object sender, PropertyChangedEventArgs e)
		{
			var fromReference = GetRef(scrPsgFrom.VerseControl.VerseRef);
			var toReference = GetRef(scrPsgTo.VerseControl.VerseRef);
			if (fromReference.CompareTo(toReference) > 0)
			{
				if (sender == scrPsgFrom)
					scrPsgTo.VerseControl.VerseRef = scrPsgFrom.VerseControl.VerseRef.Clone();
				else
					scrPsgFrom.VerseControl.VerseRef = scrPsgTo.VerseControl.VerseRef.Clone();
			}
		}
		#endregion

		#region Private helper methods
		private VerseRef GetRef(IScrVerseRef verseRef) =>
			new VerseRef(verseRef.BookNum, verseRef.ChapterNum, verseRef.VerseNum, m_versification);
		#endregion
	}
}