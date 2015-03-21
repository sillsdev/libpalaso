using System;
using System.Threading;
using System.Windows.Forms;
using NUnit.Framework;
using Palaso.UI.WindowsForms.WritingSystems;

namespace PalasoUIWindowsForms.Tests.WritingSystems
{
	[TestFixture]
	public class LookupIsoControlTests
	{
		private LookupISOControl _control;
		private bool _ready;
		private Form _testForm;

		[SetUp]
		public void Setup()
		{
			_ready = false;
			_control = new LookupISOControl();
			_control.ReadinessChanged += _control_ReadinessChanged;
			_testForm = new Form();
			_testForm.Controls.Add(_control);
		}

		private void _control_ReadinessChanged(object sender, EventArgs e)
		{
			if (_control.LanguageInfo != null)
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
		public void LookupIsoControl_AkanSearchDoesNotCrash()
		{
			_control.ISOCode = "a";
			_testForm.Show();
			WaitForControl();
			_control.ISOCode = "ak";
			WaitForControl();
			Assert.AreEqual("akq", _control.ISOCode);
			Assert.AreEqual("Ak", _control.DesiredLanguageName);
			_control.ISOCode = "akq";
			WaitForControl();
			Assert.AreEqual("akq", _control.ISOCode);
			Assert.AreEqual("Ak", _control.DesiredLanguageName);
		}
	}
}
