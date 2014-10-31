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
	/// One gotcha here:
	/// There are times when this dialog doesn't display the message correctly but instead shows a white rectangle.
	/// To prevent this, you can call form.Update() after form.Show()
	/// 
	/// e.g.
	///		using (var dlg = new SimpleMessageDialog("my message", "my title"))
	///		{
	///			dlg.Show();
	///			dlg.Update();
	///			System.Threading.Thread.Sleep(5000);
	///		}
	/// 
	/// An example of when this might be useful is to let the user know something is happening in the background,
	/// but that something will time out.
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
	}
}