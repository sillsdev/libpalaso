using SIL.Keyboarding;

namespace SIL.Windows.Forms.Keyboarding
{
	/// <summary>
	/// Events for switching keyboards
	///
	/// This interface can be implemented by an application that wants to be notified when the
	/// user switches the input method, either by shortcut key or by using the language bar.
	///
	/// An application should use this interface instead of relying on the
	/// WM_INPUTLANGCHANGE messages or Form.InputLanguageChanged events. These messages and events
	/// don't work reliably when the user switches between two text services (e.g. Chinese and
	/// Keyman), and they won't reliably tell which input method is the newly activated one (see
	/// also Michael S. Kaplan's blog post http://www.siao2.com/2006/05/16/598980.aspx).
	///
	/// The method in this interface gets called when the TSF Text Services sends a notification
	/// that the keyboard or text service got changed. The Windows keyboard adaptor falls back
	/// to using Windows messages in case the text services are not available.
	/// </summary>
	public interface IWindowsLanguageProfileSink
	{
		/// <summary>
		/// Called after the language profile has changed.
		/// </summary>
		/// <param name="previousKeyboard">The previous input method</param>
		/// <param name="newKeyboard">The new input method</param>
		void OnInputLanguageChanged(IKeyboardDefinition previousKeyboard, IKeyboardDefinition newKeyboard);
	}
}
