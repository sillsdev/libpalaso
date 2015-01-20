using System.Windows.Forms;
using NUnit.Framework;

namespace SIL.WritingSystems.WindowsForms.Tests
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
			var dialog = new LookupIsoCodeDialog();
			Application.Run(dialog);
			MessageBox.Show("returned:" + dialog.SelectedLanguage.Code + " with desired name: " + dialog.SelectedLanguage.DesiredName);
		}

		[Test, Ignore("By hand only")]
		public void LookupISODialog_WithInitialCodeAndCustomName()
		{
			var dialog = new LookupIsoCodeDialog();
			dialog.SelectedLanguage = new LanguageInfo() { Code = "etr", DesiredName = "Etoloooo" };
			Application.Run(dialog);
			MessageBox.Show("returned:" + dialog.SelectedLanguage.Code + " with desired name: " + dialog.SelectedLanguage.DesiredName);
		}

		[Test, Ignore("By hand only")]
		public void LookupISODialog_WithInitialCodeOnly()
		{
			var dialog = new LookupIsoCodeDialog();
			dialog.SelectedLanguage = new LanguageInfo() { Code = "etr" };
			Application.Run(dialog);
			MessageBox.Show("returned:" + dialog.SelectedLanguage.Code + " with desired name: " + dialog.SelectedLanguage.DesiredName);
		}

		[Test, Ignore("By hand only")]
		public void LookupISODialog_WithInitialQAACodeAndCustomName()
		{
			var dialog = new LookupIsoCodeDialog();
			dialog.SelectedLanguage = new LanguageInfo() { Code = "qaa", DesiredName = "Vulcan" };
			Application.Run(dialog);
			MessageBox.Show("returned:" + dialog.SelectedLanguage.Code + " with desired name: " + dialog.SelectedLanguage.DesiredName);
		}

	}
}