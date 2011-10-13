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
			LoadKeyboards(this.keyboardsC);
			LoadKeyboards(this.currentKeyboard);
		}

		public void LoadKeyboards(ComboBox comboBox)
		{
			var keyboards = KeyboardController.GetAvailableKeyboards(KeyboardController.Engines.All);
			foreach (var keyboard in keyboards)
			{
				comboBox.Items.Add(keyboard.ShortName);
			}
			comboBox.SelectedIndex = 0;
		}

		private void testAreaA_Enter(object sender, EventArgs e)
		{
			if (cbOnEnter.Checked)
			{
				string wantKeyboard = (string)keyboardsA.SelectedItem;
				Console.WriteLine("Enter A: Set to {0}", wantKeyboard);
				KeyboardController.ActivateKeyboard(wantKeyboard);
			} else {
				Console.WriteLine("Enter A");
			}
		}

		private void testAreaB_Enter(object sender, EventArgs e)
		{
			if (cbOnEnter.Checked)
			{
				string wantKeyboard = (string)keyboardsB.SelectedItem;
				Console.WriteLine("Enter B: Set to {0}", wantKeyboard);
				KeyboardController.ActivateKeyboard(wantKeyboard);
			} else {
				Console.WriteLine("Enter B");
			}
		}

		private void testAreaC_Enter(object sender, EventArgs e)
		{
			if (cbOnEnter.Checked)
			{
				string wantKeyboard = (string)keyboardsC.SelectedItem;
				Console.WriteLine("Enter C: Set to {0}", wantKeyboard);
				KeyboardController.ActivateKeyboard(wantKeyboard);
			} else {
				Console.WriteLine("Enter C");
			}
		}

	}
}
