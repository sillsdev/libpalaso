using System;
using System.ComponentModel;
using System.Drawing;
using System.Media;
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
		private readonly Color m_origWarningLabelColor;
		private readonly int m_initialDelay;
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

			//scrPsgFrom.VerseControl.BooksPresentSet = scrPsgTo.VerseControl.BooksPresentSet = bookSet;
			scrPsgFrom.VerseControl.ShowEmptyBooks = false;
			scrPsgTo.VerseControl.ShowEmptyBooks = false;
			scrPsgFrom.VerseControl.VerseRef = new VerseRef(001001001, m_versification);
			scrPsgTo.VerseControl.VerseRef = new VerseRef(066005008, m_versification);

			scrPsgFrom.VerseControl.TabKeyPressedInVerseField += HandleTabKeyInVerseField;
			scrPsgTo.VerseControl.TabKeyPressedInVerseField += HandleTabKeyInVerseField;

			m_origWarningLabelColor = m_lblInvalidReference.ForeColor;
			m_initialDelay = m_timerWarning.Interval;
		}

		private void HandleTabKeyInVerseField(object sender, KeyEventArgs e)
		{
			ToolStripVerseControl controlToFocus = sender == scrPsgFrom.VerseControl ?
				scrPsgTo : scrPsgFrom;
			controlToFocus.Focus();
			e.SuppressKeyPress = true;
		}
		#endregion

		#region Event handlers
		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Handles leaving the to or from passage
		/// </summary>
		/// ------------------------------------------------------------------------------------
		private void OnScrPassageLeave(object sender, EventArgs e)
		{
			var psgCtrl = (ToolStripVerseControl)sender;
			try
			{
				psgCtrl.VerseControl.AcceptData();
				ScrPassageChanged(sender, new PropertyChangedEventArgs(psgCtrl.Name));
			}
			catch (Exception)
			{
				btnOk.DialogResult = DialogResult.None;
				btnOk.Enabled = false;

				m_timerWarning.Stop();
				SystemSounds.Beep.Play();
				psgCtrl.VerseControl.VerseRef = psgCtrl.VerseControl.VerseRef;

				// reset variables and kick off fade operation
				m_lblInvalidReference.ForeColor = m_origWarningLabelColor;
				m_timerWarning.Interval = m_initialDelay;
				m_lblInvalidReference.Show();
				m_timerWarning.Start();
			}
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Handles change in the to or from passage
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

		private void ResetOkButtonAndStartToFadeWarning(object sender, EventArgs e)
		{
			btnOk.DialogResult = DialogResult.OK;
			btnOk.Enabled = true;

			// timer interval set to 10 to ensure smooth fading
			m_timerWarning.Interval = 10;
			m_timerWarning.Tick -= ResetOkButtonAndStartToFadeWarning;
			m_timerWarning.Tick += FadeWarning;

			FadeWarning(sender, e);
		}

		private void FadeWarning(object sender, EventArgs e)
		{
			btnOk.DialogResult = DialogResult.OK;
			btnOk.Enabled = true;

			// timer interval set to 10 to ensure smooth fading
			m_timerWarning.Interval = 10;

			int r = m_lblInvalidReference.ForeColor.R;
			int g = m_lblInvalidReference.ForeColor.G;
			int b = m_lblInvalidReference.ForeColor.B;
			var back = BackColor;

			if (r < back.R)
				r++;
			else if (r > back.R)
				r--;
			if (g < back.G)
				g++;
			else if (g > back.G)
				g--;
			if (b < back.B)
				b++;
			else if (b > back.B)
				b--;

			m_lblInvalidReference.ForeColor = Color.FromArgb(255, r, g, b);

			if (r == back.R && g == back.G && b == back.B) // arrived at target
			{
				// fade is complete
				m_timerWarning.Stop();
				m_timerWarning.Tick -= FadeWarning;
				// For next time...
				m_timerWarning.Tick += ResetOkButtonAndStartToFadeWarning;
				m_lblInvalidReference.Visible = false;
			}
		}
		#endregion

		#region Private helper methods
		private VerseRef GetRef(IScrVerseRef verseRef) =>
			new VerseRef(verseRef.BookNum, verseRef.ChapterNum, verseRef.VerseNum, m_versification);
		#endregion
	}
}