using System;
using System.Windows.Forms;

namespace SIL.Windows.Forms.Tests.Progress.LogBox
{
	public class LogBoxFormForTest : IDisposable
	{
		private Form _window;
		private Windows.Forms.Progress.LogBox _box;

		public LogBoxFormForTest ()
		{
			CreateWindow();
		}

		public Windows.Forms.Progress.LogBox progress
		{
			get
			{
				return _box;
			}
		}

		private void CreateWindow()
		{
			_window = new Form();
			_box = new Windows.Forms.Progress.LogBox();
			_box.Dock = DockStyle.Fill;
			_window.Controls.Add(_box);

			_window.Show();
			_box.Select();
			Application.DoEvents();
		}

		public void Dispose()
		{
			//ShowOncePerSessionBasedOnExactMessagePolicy.Reset();
			if (_window != null)
			{
				_window.Close();
				_window.Dispose();
				_window = null;
			}
		}
	}
}
