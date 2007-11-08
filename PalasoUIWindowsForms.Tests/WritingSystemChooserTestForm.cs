using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace WritingSystemSetup.Tests
{
	public partial class WritingSystemChooserTestForm : Form
	{
		public WritingSystemChooserTestForm()
		{
			InitializeComponent();
			this.pickerUsingListView1.IdentifierOfSelectedWritingSystem = "en";
			this.pickerUsingComboBox1.IdentifierOfSelectedWritingSystem = "en";
		}

		private void WritingSystemChooserTestForm_Load(object sender, EventArgs e)
		{

		}

		private void pickerUsingListView1_DoubleClicked(object sender, EventArgs e)
		{
			this.Close();
		}
	}
}