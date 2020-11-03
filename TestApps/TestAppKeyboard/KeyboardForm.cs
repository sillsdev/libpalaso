using System;
using System.Windows.Forms;
using SIL.Keyboarding;
using SIL.Windows.Forms.Keyboarding;
using SIL.Windows.Forms.Keyboarding.Windows;
using SIL.Windows.Forms.WritingSystems;

// ReSharper disable LocalizableElement
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

			Console.WriteLine();
			Console.WriteLine("Adding keyboards for Test Area A");
			LoadKeyboards(keyboardsA);
			Console.WriteLine();
			Console.WriteLine("Adding keyboards for Test Area B");
			LoadKeyboards(keyboardsB);
			Console.WriteLine();
			Console.WriteLine("Adding keyboards for Test Area C");
			LoadKeyboards(keyboardsC);
			Console.WriteLine();
			Console.WriteLine("Adding keyboards for Current Keyboard dropdown");
			LoadKeyboards(currentKeyboard);
		}

		public void LoadKeyboards(ComboBox comboBox)
		{
			var keyboards = Keyboard.Controller.AvailableKeyboards;
			foreach (var keyboard in keyboards)
			{
				comboBox.Items.Add(new WSKeyboardControl.KeyboardDefinitionAdapter(keyboard));
				Console.WriteLine($"added keyboard id: {keyboard.Id}, name: {keyboard.Name}");
			}
			comboBox.SelectedIndex = 0;
		}

		private void testAreaA_Enter(object sender, EventArgs e)
		{
			if (cbOnEnter.Checked)
			{
				var wantKeyboard = ((WSKeyboardControl.KeyboardDefinitionAdapter)keyboardsA.SelectedItem).KeyboardDefinition;
				Console.WriteLine($"Enter A: Set to {wantKeyboard}");
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
				Console.WriteLine($"Enter B: Set to {wantKeyboard}");
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
				Console.WriteLine($"Enter C: Set to {wantKeyboard}");
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
