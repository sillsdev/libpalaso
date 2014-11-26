using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Text;

using NUnit.Framework;

using Palaso.WritingSystems;
using Palaso.UI.WindowsForms.WritingSystems;

namespace PalasoUIWindowsForms.Tests.WritingSystems
{
	[TestFixture]
	public class LookupIsoCodeModelTests
	{
		private LookupIsoCodeModel _model;

		[SetUp]
		public void Setup()
		{
			_model = new LookupIsoCodeModel();
		}

		[Test, Ignore("By hand only")]
		public void LookupISODialog()
		{
			var dialog = new LookupISOCodeDialog();
			Application.Run(dialog);
			MessageBox.Show("returned:" + dialog.SelectedLanguage.Code + " with desired name: " + dialog.SelectedLanguage.DesiredName);
		}

		[Test, Ignore("By hand only")]
		public void LookupISODialog_WithInitialCodeAndCustomName()
		{
			var dialog = new LookupISOCodeDialog();
			dialog.SelectedLanguage = new LanguageInfo() { Code = "etr", DesiredName = "Etoloooo" };
			Application.Run(dialog);
			MessageBox.Show("returned:" + dialog.SelectedLanguage.Code + " with desired name: " + dialog.SelectedLanguage.DesiredName);
		}

		[Test, Ignore("By hand only")]
		public void LookupISODialog_WithInitialCodeOnly()
		{
			var dialog = new LookupISOCodeDialog();
			dialog.SelectedLanguage = new LanguageInfo() { Code = "etr" };
			Application.Run(dialog);
			MessageBox.Show("returned:" + dialog.SelectedLanguage.Code + " with desired name: " + dialog.SelectedLanguage.DesiredName);
		}

		[Test, Ignore("By hand only")]
		public void LookupISODialog_WithInitialQAACodeAndCustomName()
		{
			var dialog = new LookupISOCodeDialog();
			dialog.SelectedLanguage = new LanguageInfo() { Code = "qaa", DesiredName = "Vulcan" };
			Application.Run(dialog);
			MessageBox.Show("returned:" + dialog.SelectedLanguage.Code + " with desired name: " + dialog.SelectedLanguage.DesiredName);
		}

	}
}