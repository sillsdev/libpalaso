using System.Windows.Forms;
using NUnit.Framework;
using SIL.WritingSystems;

namespace SIL.Windows.Forms.WritingSystems.Tests
{
	[TestFixture]
	public class LookupLanguageModelTests
	{
		private LookupLanguageModel _model;

		[SetUp]
		public void Setup()
		{
			_model = new LookupLanguageModel();
		}

		[Test, Ignore("By hand only")]
		public void LookupISODialog()
		{
			var dialog = new LookupLanguageDialog();
			Application.Run(dialog);
			MessageBox.Show("returned:" + dialog.SelectedLanguage.LanguageTag + " with desired name: " + dialog.SelectedLanguage.DesiredName);
		}

		[Test, Ignore("By hand only")]
		public void LookupISODialog_WithInitialCodeAndCustomName()
		{
			var dialog = new LookupLanguageDialog();
			dialog.SelectedLanguage = new LanguageInfo() { LanguageTag = "etr", DesiredName = "Etoloooo" };
			Application.Run(dialog);
			MessageBox.Show("returned:" + dialog.SelectedLanguage.LanguageTag + " with desired name: " + dialog.SelectedLanguage.DesiredName);
		}

		[Test, Ignore("By hand only")]
		public void LookupISODialog_WithInitialCodeOnly()
		{
			var dialog = new LookupLanguageDialog();
			dialog.SelectedLanguage = new LanguageInfo() { LanguageTag = "etr" };
			Application.Run(dialog);
			MessageBox.Show("returned:" + dialog.SelectedLanguage.LanguageTag + " with desired name: " + dialog.SelectedLanguage.DesiredName);
		}

		[Test, Ignore("By hand only")]
		public void LookupISODialog_WithInitialQAACodeAndCustomName()
		{
			var dialog = new LookupLanguageDialog();
			dialog.SelectedLanguage = new LanguageInfo() { LanguageTag = "qaa", DesiredName = "Vulcan" };
			Application.Run(dialog);
			MessageBox.Show("returned:" + dialog.SelectedLanguage.LanguageTag + " with desired name: " + dialog.SelectedLanguage.DesiredName);
		}

	}
}