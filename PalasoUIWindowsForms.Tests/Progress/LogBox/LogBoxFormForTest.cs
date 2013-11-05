using System;
using System.Windows.Forms;
using ProgressLogBox = Palaso.UI.WindowsForms.Progress.LogBox;

namespace Palaso.Tests.Progress.LogBoxTests
{
	public class LogBoxFormForTest : IDisposable
	{
		private Form _window;
		private ProgressLogBox _box;

		public LogBoxFormForTest ()
		{
			CreateWindow();
		}

		public ProgressLogBox progress
		{
			get
			{
				return _box;
			}
		}

		private void CreateWindow()
		{
			_window = new Form();
			_box = new ProgressLogBox();
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
