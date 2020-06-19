
namespace SIL.Keyboarding
{
	/// <summary>
	/// This class acts as a central point for writing-system related keyboarding information.
	/// Currently its only function is to provide a home for the active IKeyboardController.
	/// </summary>
	public static class Keyboard
	{
		private static IKeyboardController _controller;

		/// <summary>
		/// The active instance of keyboard controller which is used by various writing system methods.
		/// This may be set to a stub for testing, or by clients to a useful controller such as
		/// SIL.Windows.Forms.Keyboarding.KeyboardController. If not otherwise set, it returns a
		/// DefaultKeyboardController.
		/// </summary>
		public static IKeyboardController Controller
		{
			get => _controller ?? (_controller = new DefaultKeyboardController());
			set => _controller = value;
		}
	}
}
