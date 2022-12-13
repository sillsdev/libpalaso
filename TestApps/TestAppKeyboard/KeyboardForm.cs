using System;
using System.Collections.Generic;
using System.Windows.Forms;
using SIL.Keyboarding;
using SIL.Windows.Forms.Keyboarding;
using SIL.Windows.Forms.Keyboarding.Windows;
using SIL.Windows.Forms.WritingSystems;

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
			var eventHandler = new TestWindowsLanguageProfileSink(this);
			KeyboardController.RegisterControl(testAreaA, eventHandler);
			KeyboardController.RegisterControl(testAreaB, eventHandler);
			KeyboardController.RegisterControl(testAreaC, eventHandler);

			LoadKeyboards(keyboardsA, 0);
			LoadKeyboards(keyboardsB, 1);
			LoadKeyboards(keyboardsC, 2);
			LoadKeyboards(currentKeyboard, 0);
		}

		private static void LoadKeyboards(ComboBox comboBox, int preferredInitialKeyboard)
		{
			IEnumerable<IKeyboardDefinition> keyboards = Keyboard.Controller.AvailableKeyboards;
			foreach (var keyboard in keyboards)
			{
				comboBox.Items.Add(new WSKeyboardControl.KeyboardDefinitionAdapter(keyboard));
				Console.WriteLine($"added keyboard id: {keyboard.Id}, name: {keyboard.Name}");
			}

			if (comboBox.Items.Count <= 0)
			{
				Console.WriteLine("WARNING: No available keyboards found!");
				return;
			}

			if (preferredInitialKeyboard < comboBox.Items.Count)
				comboBox.SelectedIndex = preferredInitialKeyboard;
			else
				comboBox.SelectedIndex = 0;
		}

		private void testAreaA_Enter(object sender, EventArgs e)
		{
			if (cbOnEnter.Checked)
			{
				var wantKeyboard = ((WSKeyboardControl.KeyboardDefinitionAdapter)keyboardsA.SelectedItem).KeyboardDefinition;
				Console.WriteLine("Enter A: Set to {0}", wantKeyboard);
				wantKeyboard.Activate();
			} else {
				Console.WriteLine("Enter A");
			}
		}

		private void testAreaB_Enter(object sender, EventArgs e)
		{
			if (cbOnEnter.Checked)
			{
				var wantKeyboard = ((WSKeyboardControl.KeyboardDefinitionAdapter) keyboardsB.SelectedItem).KeyboardDefinition;
				Console.WriteLine("Enter B: Set to {0}", wantKeyboard);
				wantKeyboard.Activate();
			} else {
				Console.WriteLine("Enter B");
			}
		}

		private void testAreaC_Enter(object sender, EventArgs e)
		{
			if (cbOnEnter.Checked)
			{
				var wantKeyboard = ((WSKeyboardControl.KeyboardDefinitionAdapter)keyboardsC.SelectedItem).KeyboardDefinition;
				Console.WriteLine("Enter C: Set to {0}", wantKeyboard);
				wantKeyboard.Activate();
			} else {
				Console.WriteLine("Enter C");
			}
		}

		private class TestWindowsLanguageProfileSink : IWindowsLanguageProfileSink
		{
			private readonly KeyboardForm _form;

			public TestWindowsLanguageProfileSink(KeyboardForm form)
			{
				_form = form;
			}

			public void OnInputLanguageChanged(IKeyboardDefinition previousKeyboard, IKeyboardDefinition newKeyboard)
			{
				Console.WriteLine("TestAppKeyboard.OnLanguageChanged: previous {0}, new {1}",
					previousKeyboard != null ? previousKeyboard.Layout : "<null>",
					newKeyboard != null ? newKeyboard.Layout : "<null>");
				_form.lblCurrentKeyboard.Text = newKeyboard != null ? newKeyboard.Layout : "<null>";
			}
		}
	}
}
