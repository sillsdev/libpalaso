using System;
using System.Windows.Forms;
using NUnit.Framework;

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
			Assert.AreEqual("Ak", _control.SelectedLanguage.DesiredName);
			_control.SearchText = "akq";
			WaitForControl();
			Assert.AreEqual("akq", _control.SelectedLanguage.LanguageTag);
			Assert.AreEqual("Ak", _control.SelectedLanguage.DesiredName);
		}
	}
}
