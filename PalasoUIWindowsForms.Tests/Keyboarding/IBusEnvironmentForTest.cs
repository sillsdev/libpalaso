using System;
using System.Windows.Forms;
using Palaso.Reporting;

namespace PalasoUIWindowsForms.Tests.Keyboarding
{
	public class IBusEnvironmentForTest : IDisposable
	{
		private Form _window;

		public const string OtherKeyboard = "m17n:am:sera";
		public const string DefaultKeyboard = "m17n:en:ispell";

		public IBusEnvironmentForTest(bool withWindow)
		{
			ErrorReport.IsOkToInteractWithUser = false;
			ShowOncePerSessionBasedOnExactMessagePolicy.Reset();
			if (withWindow)
			{
				CreateWindow();
			}
		}

		private void CreateWindow()
		{
			_window = new Form();
			var box = new TextBox();
			box.Dock = DockStyle.Fill;
			_window.Controls.Add(box);

			_window.Show();
			box.Select();
			Application.DoEvents();
		}

		public void Dispose()
		{
			ShowOncePerSessionBasedOnExactMessagePolicy.Reset();
			if (_window != null)
			{
				_window.Close();
				_window.Dispose();
				_window = null;
			}
		}
	}
}