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
			Console.WriteLine("Setup created control");
		}

		private void _control_ReadinessChanged(object sender, EventArgs e)
		{
			Console.WriteLine("Test: check readiness");
			if (_control.SelectedLanguage != null)
			{
				Console.WriteLine("Test: control readiness has changed");
				_ready = true;
			}
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
			Console.WriteLine("AkanSearchDoesNotCrash: starting");
			_control.SearchText = "a";
			_testForm.Show();
			Console.WriteLine("AkanSearchDoesNotCrash: about to wait for control (1)");
			WaitForControl();
			Console.WriteLine("AkanSearchDoesNotCrash: finished waiting for control (1)");
			_control.SearchText = "ak";
			Console.WriteLine("AkanSearchDoesNotCrash: about to wait for control (2)");
			WaitForControl();
			Console.WriteLine("AkanSearchDoesNotCrash: finished waiting for control (2)");
			Assert.AreEqual("akq", _control.SelectedLanguage.LanguageTag);
			Assert.AreEqual("Ak", _control.DesiredLanguageName);
			_control.SearchText = "akq";
			Console.WriteLine("AkanSearchDoesNotCrash: about to wait for control (3)");
			WaitForControl();
			Console.WriteLine("AkanSearchDoesNotCrash: finished waiting for control (3)");
			Assert.AreEqual("akq", _control.SelectedLanguage.LanguageTag);
			Assert.AreEqual("Ak", _control.DesiredLanguageName);
		}
	}
}
