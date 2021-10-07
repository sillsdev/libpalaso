using System.Diagnostics;
using System.Windows.Forms;

namespace SIL.Windows.Forms.TestApp
{
	public partial class ParentOfModalChild : ParentFormBase
	{
		public ParentOfModalChild()
		{
			InitializeComponent();
			// In the output window, the four Trace.WriteLine messages should appear in ascending order:
			OnModalFormShown += () => Trace.WriteLine($"1) Modal child form shown. IsShowingModalForm = {IsShowingModalForm}");
			OnModalFormClosed += () => Trace.WriteLine($"2) Modal child form closed. IsShowingModalForm = {IsShowingModalForm}");
			OnModalFormDisposed += () => Trace.WriteLine($"4) Modal child form disposed. IsShowingModalForm = {IsShowingModalForm}");
		}

		private void button1_Click(object sender, System.EventArgs e)
		{
			ShowModalChild(new ModalChildForm(), form =>
			{
				Trace.WriteLine($"3) Modal child form ({form.Name}) onClosed handler called. DialogResult = {form.DialogResult}");
				if (form.DialogResult == DialogResult.OK)
					MessageBox.Show("OK button was clicked.");
			});
		}
	}
}
