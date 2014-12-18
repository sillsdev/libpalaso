using System;
using System.Windows.Forms;
using SIL.WritingSystems;
using SIL.WritingSystems.WindowsForms.Keyboarding;

namespace TestAppKeyboard
{
	public partial class KeyboardForm : Form
	{
		public KeyboardForm()
		{
			InitializeComponent();
			if (DesignMode)
				return;

			KeyboardController.Initialize();
			KeyboardController.Instance.RegisterControl(testAreaA);
			KeyboardController.Instance.RegisterControl(testAreaB);
			KeyboardController.Instance.RegisterControl(testAreaC);
			LoadKeyboards(keyboardsA);
			LoadKeyboards(keyboardsB);
			LoadKeyboards(keyboardsC);
			LoadKeyboards(currentKeyboard);
		}

		public void LoadKeyboards(ComboBox comboBox)
		{
			var keyboards = Keyboard.Controller.AllAvailableKeyboards;
			foreach (var keyboard in keyboards)
			{
				comboBox.Items.Add(keyboard);
				Console.WriteLine("added keyboard id: {0}, name: {1}", keyboard.Id, keyboard.Name);
			}
			comboBox.SelectedIndex = 0;
		}

		private void testAreaA_Enter(object sender, EventArgs e)
		{
			if (cbOnEnter.Checked)
			{
				var wantKeyboard = (KeyboardDescription)keyboardsA.SelectedItem;
				Console.WriteLine("Enter A: Set to {0}", wantKeyboard);
				Keyboard.Controller.SetKeyboard(wantKeyboard);
			} else {
				Console.WriteLine("Enter A");
			}
		}

		private void testAreaB_Enter(object sender, EventArgs e)
		{
			if (cbOnEnter.Checked)
			{
				var wantKeyboard = (KeyboardDescription)keyboardsB.SelectedItem;
				Console.WriteLine("Enter B: Set to {0}", wantKeyboard);
				Keyboard.Controller.SetKeyboard(wantKeyboard);
			} else {
				Console.WriteLine("Enter B");
			}
		}

		private void testAreaC_Enter(object sender, EventArgs e)
		{
			if (cbOnEnter.Checked)
			{
				var wantKeyboard = (KeyboardDescription)keyboardsC.SelectedItem;
				Console.WriteLine("Enter C: Set to {0}", wantKeyboard);
				Keyboard.Controller.SetKeyboard(wantKeyboard);
			} else {
				Console.WriteLine("Enter C");
			}
		}

	}
}
