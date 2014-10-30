using System.Windows.Forms;

namespace Palaso.UI.WindowsForms
{
	public partial class SimpleMessageDialog : Form
	{
		public SimpleMessageDialog(string msg, string title = " ")
		{
			InitializeComponent();
			Text = title;
			label1.Text = msg;
		}
	}
}