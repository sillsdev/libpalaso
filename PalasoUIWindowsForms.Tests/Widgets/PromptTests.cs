using System;
using System.ComponentModel;
using System.Windows.Forms;
using NUnit.Framework;
using Palaso.UI.WindowsForms.Widgets;

namespace PalasoUIWindowsForms.Tests.Widgets
{
	[TestFixture]
	public class PromptTests
	{
		private Prompt _prompt;

		[SetUp]
		public void Setup()
		{
			_prompt = new Prompt();
		}

		[TearDown]
		public void Teardown()
		{
			_prompt.Dispose();
		}

		[Test]
		public void Construct()
		{
			using (Prompt prompt = new Prompt())
			{
				Assert.IsNotNull(prompt);
			}
		}

		[Test]
		public void CanExtend_TextBox_True()
		{
			using (TextBox textBox = new TextBox())
			{
				Assert.IsTrue(((IExtenderProvider) _prompt).CanExtend(textBox));
			}
		}

		[Test]
		public void CanExtend_RichTextBox_True()
		{
			using (RichTextBox textBox = new RichTextBox())
			{
				Assert.IsTrue(((IExtenderProvider) _prompt).CanExtend(textBox));
			}
		}

		[Test]
		public void CanExtend_ComboBox_False()
		{
			using (ComboBox comboBox = new ComboBox())
			{
				Assert.IsFalse(((IExtenderProvider) _prompt).CanExtend(comboBox));
			}
		}

		[Test]
		public void CanExtend_Null_False()
		{
			Assert.IsFalse(((IExtenderProvider) _prompt).CanExtend(null));
		}

		[Test]
		[ExpectedException(typeof (ObjectDisposedException))]
		public void CanExtend_CalledAfterDisposed_Throws()
		{
			Prompt prompt = new Prompt();
			prompt.Dispose();
			using (TextBox textBox = new TextBox())
			{
				prompt.CanExtend(textBox);
			}
		}

		[Test]
		[ExpectedException(typeof (ArgumentNullException))]
		public void GetPrompt_Null_Throws()
		{
			_prompt.GetPrompt(null);
		}

		[Test]
		[ExpectedException(typeof (ArgumentException))]
		public void GetPrompt_ComboBox_Throws()
		{
			using (ComboBox comboBox = new ComboBox())
			{
				_prompt.GetPrompt(comboBox);
			}
		}

		[Test]
		public void GetPrompt_NeverSet_EmptyString()
		{
			using (TextBox textBox = new TextBox())
			{
				Assert.AreEqual(string.Empty, _prompt.GetPrompt(textBox));
			}
		}

		[Test]
		public void GetPrompt_Set_StoredValue()
		{
			using (TextBox textBox = new TextBox())
			{
				string value = "A value";
				_prompt.SetPrompt(textBox, value);
				Assert.AreEqual(value, _prompt.GetPrompt(textBox));
			}
		}

		[Test]
		[ExpectedException(typeof (ObjectDisposedException))]
		public void GetPrompt_CalledAfterDisposed_Throws()
		{
			Prompt prompt = new Prompt();
			prompt.Dispose();
			using (TextBox textBox = new TextBox())
			{
				prompt.GetPrompt(textBox);
			}
		}

		[Test]
		public void GetPrompt_SetNullString_GetEmptyString()
		{
			using (TextBox textBox = new TextBox())
			{
				_prompt.SetPrompt(textBox, null);
				Assert.AreEqual(string.Empty, _prompt.GetPrompt(textBox));
			}
		}

		[Test]
		[ExpectedException(typeof (ArgumentNullException))]
		public void SetPrompt_NullControl_EmptyString()
		{
			_prompt.SetPrompt(null, "value");
		}

		[Test]
		[ExpectedException(typeof (ArgumentException))]
		public void SetPrompt_ComboBox_Throws()
		{
			using (ComboBox comboBox = new ComboBox())
			{
				_prompt.SetPrompt(comboBox, "value");
			}
		}

		[Test]
		public void SetPrompt_SetTwice_GetSecond()
		{
			string firstValue = "first";
			string secondValue = "second";

			using (TextBox textBox = new TextBox())
			{
				_prompt.SetPrompt(textBox, firstValue);
				_prompt.SetPrompt(textBox, secondValue);
				Assert.AreEqual(secondValue, _prompt.GetPrompt(textBox));
			}
		}

		[Test]
		[ExpectedException(typeof (ObjectDisposedException))]
		public void SetPrompt_CalledAfterDisposed_Throws()
		{
			Prompt prompt = new Prompt();
			prompt.Dispose();
			using (TextBox textBox = new TextBox())
			{
				prompt.SetPrompt(textBox, "prompt");
			}
		}

		[Test]
		public void Dispose_HasDisposedEventHandler_CallsHandler()
		{
			Prompt prompt = new Prompt();
			bool disposedCalled = false;
			prompt.Disposed += delegate { disposedCalled = true; };
			Assert.IsFalse(disposedCalled);
			prompt.Dispose();
			Assert.IsTrue(disposedCalled);
		}

		[Test]
		public void Dispose_CalledTwice_CallsHandlerOnce()
		{
			Prompt prompt = new Prompt();
			int disposedCalledCount = 0;
			prompt.Disposed += delegate { ++disposedCalledCount; };
			Assert.AreEqual(0, disposedCalledCount);
			prompt.Dispose();
			Assert.AreEqual(1, disposedCalledCount);
			prompt.Dispose();
			Assert.AreEqual(1, disposedCalledCount);
		}

		[Test]
		[ExpectedException(typeof (ObjectDisposedException))]
		public void GetIsPromptVisible_CalledAfterDisposed_Throws()
		{
			Prompt prompt = new Prompt();
			prompt.Dispose();
			using (TextBox textBox = new TextBox())
			{
				prompt.GetIsPromptVisible(textBox);
			}
		}

		[Test]
		[ExpectedException(typeof (ObjectDisposedException))]
		public void GetSite_CalledAfterDisposed_Throws()
		{
			Prompt prompt = new Prompt();
			prompt.Dispose();
			ISite site = prompt.Site;
		}

		[Test]
		[ExpectedException(typeof (ObjectDisposedException))]
		public void SetSite_CalledAfterDisposed_Throws()
		{
			Prompt prompt = new Prompt();
			prompt.Dispose();
			prompt.Site = null;
		}
	}
}