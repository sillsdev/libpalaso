using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Palaso.UI.WindowsForms.Keyboarding;
using Palaso.UI.WindowsForms.Keyboarding.Interfaces;
using Palaso.WritingSystems;

namespace TestAppKeyboard
{
	public partial class KeyboardForm : Form, IWindowsLanguageProfileSink
	{
		public KeyboardForm()
		{
			InitializeComponent();
			if (DesignMode)
				return;

			KeyboardController.Initialize();
			KeyboardController.Register(testAreaA, this);
			KeyboardController.Register(testAreaB, this);
			KeyboardController.Register(testAreaC, this);
			LoadKeyboards(this.keyboardsA);
			LoadKeyboards(this.keyboardsB);
			LoadKeyboards(this.keyboardsC);
			LoadKeyboards(this.currentKeyboard);
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


		#region IWindowsLanguageProfileSink Members

		public void OnInputLanguageChanged(IKeyboardDefinition previousKeyboard, IKeyboardDefinition newKeyboard)
		{
			Console.WriteLine("TestAppKeyboard.OnLanguageChanged: previous {0}, new {1}",
				previousKeyboard != null ? previousKeyboard.Layout : "<null>",
				newKeyboard != null ? newKeyboard.Layout : "<null>");
			lblCurrentKeyboard.Text = newKeyboard != null ? newKeyboard.Layout : "<null>";
		}

		#endregion
	}
}
