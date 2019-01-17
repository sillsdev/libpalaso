using System;
using System.Drawing;
using System.Windows.Forms;
using NUnit.Framework;
using SIL.WritingSystems;

namespace SIL.Windows.Forms.WritingSystems.Tests
{
	[TestFixture]
	public class LanguageLookupControlTests
	{
		private LanguageLookupControl _control;
		private bool _ready;
		private Form _testForm;

		[SetUp]
		public void Setup()
		{
			_ready = false;
			_control = new LanguageLookupControl();
			_control.ReadinessChanged += _control_ReadinessChanged;
			_testForm = new Form();
			_testForm.Controls.Add(_control);
		}

		private void _control_ReadinessChanged(object sender, EventArgs e)
		{
			if (_control.SelectedLanguage != null)
				_ready = true;
		}

		private void AddOKButtonToTestForm()
		{
			var okButton = new Button
			{
				Text = "OK",
				DialogResult = DialogResult.OK
			};
			_testForm.Controls.Add(okButton);
			_testForm.AcceptButton = okButton;
			var otherCtrl = _testForm.Controls[0];
			var bottomRight = new Point(otherCtrl.Width + otherCtrl.Location.X, otherCtrl.Height + otherCtrl.Location.Y);
			okButton.Location = new Point(bottomRight.X - (okButton.Width + 5), bottomRight.Y - 20);
			okButton.BringToFront();
			okButton.Visible = true;
		}

		private void WaitForControl()
		{
			while (!_ready)
			{
				Application.DoEvents();
			}
			_ready = false;
		}

		[Test]
		public void AkanSearchDoesNotCrash()
		{
			_control.SearchText = "a";
			_testForm.Show();
			WaitForControl();
			_control.SearchText = "ak";
			WaitForControl();
			Assert.AreEqual("akq", _control.SelectedLanguage.LanguageTag);
			Assert.AreEqual("Ak", _control.DesiredLanguageName);	// name matches by preference to tag (except for "en")
			_control.SearchText = "akq";
			WaitForControl();
			Assert.AreEqual("akq", _control.SelectedLanguage.LanguageTag);	// tag can also match
			Assert.AreEqual("Ak", _control.DesiredLanguageName);
		}

		[Test, Ignore("By Hand")]
		[Category("SkipOnTeamCity")]
		public void TestLanguageLookupControl_manualTest()
		{
			_control.SearchText = "";
			_control.IsScriptAndVariantLinkVisible = true; // comment this line to verify control w/o Script link
			_control.IsShowRegionalDialectsCheckBoxVisible = true;
			_testForm.Width = 613;
			_testForm.Height = 425;
			_testForm.AutoSize = false;
			_testForm.MaximizeBox = false;
			_testForm.MinimizeBox = false;
			_testForm.SizeGripStyle = SizeGripStyle.Hide;
			_testForm.FormBorderStyle = FormBorderStyle.FixedDialog;
			var initialControlLoc = _testForm.Controls[0].Location;
			_testForm.Controls[0].Location = new Point(initialControlLoc.X + 10, initialControlLoc.Y);
			AddOKButtonToTestForm();

			MessageBox.Show("Attach debugger","Debug");
			_testForm.ShowDialog();
			MessageBox.Show("Got '" + _control.SelectedLanguage.LanguageTag + "'", "Test Result");
		}

		[Test, Ignore("By Hand")]
		[Category("SkipOnTeamCity")]
		public void TestLanguageLookupDialog_manualTest()
		{
			const string testLangCode = "sok-x-easy";
			const string testLangThreeLetterCode = "sok";
			const string testLangName = "Sokoro";
			using (var dlg = new LanguageLookupDialog())
			{
				dlg.IsDesiredLanguageNameFieldVisible = true;
				dlg.IsShowRegionalDialectsCheckBoxVisible = true;
				dlg.IsScriptAndVariantLinkVisible = true;

				var language = new LanguageInfo() { LanguageTag = testLangCode };
				language.DesiredName = testLangName;
				dlg.SelectedLanguage = language;
				dlg.SearchText = testLangThreeLetterCode;
				dlg.UseSimplifiedChinese();

				dlg.ShowDialog();
				MessageBox.Show("Got LanguageTag='" + dlg.SelectedLanguage.LanguageTag + "'.");
			}
		}
	}
}
