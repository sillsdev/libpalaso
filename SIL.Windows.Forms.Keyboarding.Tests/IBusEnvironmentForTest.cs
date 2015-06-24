#if __MonoCS__
#if WANT_PORT
namespace SIL.Windows.Forms.Keyboarding.Tests
{
	public class IBusEnvironmentForTest : IDisposable
	{
		private Form _window;
		private bool wasibusrunning;

		public const string OtherKeyboard = "m17n:am:sera";
		public const string DefaultKeyboard = "m17n:en:ispell";

		public IBusEnvironmentForTest(bool withWindow, bool withIBus)
		{
			ErrorReport.IsOkToInteractWithUser = false;
			wasibusrunning = IBusAdaptor.EngineAvailable;
			if (withIBus)
			{
				if (!IBusAdaptor.StartIBus())
					throw new ApplicationException("Could not start IBus");
			}
			else if (wasibusrunning) {
				if (!IBusAdaptor.ExitIBus())
					throw new ApplicationException("Could not stop IBus");
			}
			IBusAdaptor.CloseConnection();
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
			IBusAdaptor.CloseConnection();
			if (wasibusrunning)
			{
				IBusAdaptor.StartIBus();
			}
			else {
				IBusAdaptor.ExitIBus();
			}

		}
	}
}
#endif
#endif
