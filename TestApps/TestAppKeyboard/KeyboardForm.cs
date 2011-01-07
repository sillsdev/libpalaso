using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Palaso.UI.WindowsForms.Keyboarding;

namespace TestAppKeyboard
{
	public partial class KeyboardForm : Form
	{
		public KeyboardForm()
		{
			InitializeComponent();
			if (DesignMode)
				return;

			LoadKeyboards(this.keyboardsA);
			LoadKeyboards(this.keyboardsB);
		}

		public void LoadKeyboards(ComboBox comboBox)
		{
			var keyboards = KeyboardController.GetAvailableKeyboards(KeyboardController.Engines.All);
			foreach (var keyboard in keyboards)
			{
				comboBox.Items.Add(keyboard.Name);
			}
			comboBox.SelectedIndex = 0;
		}

		private void testAreaA_Enter(object sender, EventArgs e)
		{
			KeyboardController.ActivateKeyboard((string)keyboardsA.SelectedItem);
		}

		private void testAreaB_Enter(object sender, EventArgs e)
		{
			KeyboardController.ActivateKeyboard((string)keyboardsB.SelectedItem);
		}
	}
}
