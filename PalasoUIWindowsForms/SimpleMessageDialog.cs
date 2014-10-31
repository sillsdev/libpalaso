using System.Windows.Forms;

namespace Palaso.UI.WindowsForms
{
	/// <summary>
	/// A dialog with a caption and a message (and no buttons)
	/// 
	/// Normally, if a dialog doesn't have a caption, the caption bar is hidden;
	/// however, we always want a caption bar even if we don't have a caption.  Use " " to get this result.
	/// If the caller truly wants no caption bar, call with "".
	/// 
	/// An example of when this might be useful is an information message that doesn't need acknowledgement...
	/// Normal usage is that the caller of Show() will eventually call Hide() or just Dispose(), without waiting for the user to do anything.
	/// </summary>
	public partial class SimpleMessageDialog : Form
	{
		public SimpleMessageDialog(string msg, string caption = " ")
		{
			if (caption == null)
				caption = " ";
			InitializeComponent();
			Text = caption;
			_dialogMessage.Text = msg;
		}

		// Without this, for an unknown reason, a white rectangle is displayed in place of the message
		private void SimpleMessageDialog_Activated(object sender, System.EventArgs e)
		{
			Update();
		}
	}
}