using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace Palaso.UI.WindowsForms.WritingSystems.WSIdentifiers
{
	public partial class VoiceIdentifierView : UserControl
	{
		public VoiceIdentifierView()
		{
			InitializeComponent();
		}
		public string ChoiceName
		{
			get { return "Voice"; }
		}
	}
}
