using System;
using Palaso.TestUtilities;
using Palaso.UI.WindowsForms.i8n;
using NUnit.Framework;
using System.Windows.Forms;

namespace PalasoUIWindowsForms.Tests.i8n
{
	class Environment : IDisposable
	{

		public void CreateSimpleStringCatalog()
		{
			TempFile tempPoFile = GetSimplePoFile();
			StringCatalog catalog = new StringCatalog(tempPoFile.Path, String.Empty, (float) 9);
		}

		private TempFile GetSimplePoFile()
		{
			string[] fileContent = new string[2];
			fileContent[0] = "msgid \"This is test input\"";
			fileContent[1] = "msgstr \"This is a localized output\"";

			var poFile = new TempFile(fileContent);
			return poFile;
		}

		private TempFile GetSimplePoTemplateFile()
		{
			string[] fileContent = new string[2];
			fileContent[0] = "msgid \"This is test input\"";
			fileContent[1] = "";

			var potFile = new TempFile(fileContent);
			return potFile;
		}

		public void Dispose()
		{
		}
	}

	[TestFixture]
	public class LocalizationHelperTests
	{
		private Form _mainWindow;
		private Label _label;
		private LocalizationHelper _localizationHelper;

		[Test]
		public void LocalizationHelperAttachedToLabel_LabelTextIsSet_LabelTextIsLocalized()
		{
			using (var env = new Environment())
			{
				env.CreateSimpleStringCatalog();
				CreateFormWithLabel();

				using (_localizationHelper = new LocalizationHelper())
				{
					_localizationHelper.Parent = _label;
					_localizationHelper.EndInit();

					_label.Text = "This is test input";
					Assert.AreEqual(_label.Text, "This is a localized output");
				}
			}
		}

		[Test]
		public void LocalizationHelperAttachedToMainWindow_ChildControlsTextIsSet_ChildControlsTextIsLocalized()
		{
			using (var env = new Environment())
			{
				env.CreateSimpleStringCatalog();
				CreateFormWithLabel();

				using (_localizationHelper = new LocalizationHelper())
				{
					_localizationHelper.Parent = _mainWindow;
					_localizationHelper.EndInit();

					_label.Text = "This is test input";
					Assert.AreEqual(_label.Text, "This is a localized output");
				}
			}
		}

		[Test]
		public void LocalizationHelperAttachedToLabel_LabelTextIsSetToUntranslatedString_LabelTextIsNotLocalized()
		{
			using (var env = new Environment())
			{
				env.CreateSimpleStringCatalog();
				CreateFormWithLabel();

				using (_localizationHelper = new LocalizationHelper())
				{
					_localizationHelper.Parent = _mainWindow;
					_localizationHelper.EndInit();

					_label.Text = "This string has not been localized.";
					Assert.AreEqual(_label.Text, "This string has not been localized.");
				}
			}
		}

		[Test]
		public void LocalizationHelperAttachedToLabel_LabelTextIsSetToUntranslatedStringWhichIsAlsoNotInTemplateFile_LabelTextIsAddedToTemplate()
		{
			using (var env = new Environment())
			{
				env.CreateSimpleStringCatalog();
				CreateFormWithLabel();

				using (_localizationHelper = new LocalizationHelper())
				{
					_localizationHelper.Parent = _label;
					_localizationHelper.EndInit();

					_label.Text = "This string has not been localized and is not in Template file.";
					throw new NotImplementedException();
					Assert.AreEqual(_label.Text, "This string has not been localized.");
				}
			}
		}

		private void CreateFormWithLabel()
		{
			_mainWindow = new Form();
			_label = new Label();
			_mainWindow.Controls.Add(_label);
			_mainWindow.Show();
		}
	}
}
