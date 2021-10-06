using System.Windows.Forms;

namespace SIL.Windows.Forms.TestApp
{
	public partial class ParentOfModalChild : ParentFormBase
	{
		public ParentOfModalChild()
		{
			InitializeComponent();
		}

		private void button1_Click(object sender, System.EventArgs e)
		{
			ShowModalChild(new ModalChildForm(), form =>
			{
				if (form.DialogResult == DialogResult.OK)
					MessageBox.Show("OK button was clicked.");
			});
		}
	}
}
