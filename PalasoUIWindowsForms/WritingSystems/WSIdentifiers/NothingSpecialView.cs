using System;
using System.Windows.Forms;

namespace Palaso.UI.WindowsForms.WritingSystems.WSIdentifiers
{
	public partial class NothingSpecialView : UserControl
	{
		public NothingSpecialView()
		{
			InitializeComponent();
		}

		public string ChoiceName
		{
			get { return "None"; }
		}
	}
}
