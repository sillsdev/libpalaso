using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using NUnit.Framework;
using SIL.Windows.Forms.HotSpot;

namespace SIL.Windows.Forms.Tests.Hotspot
{
	[TestFixture]
	public class HotSpotProviderTests
	{
		private HotSpotProvider _hotSpotProvider;

		[SetUp]
		public void Setup()
		{
			_hotSpotProvider = new HotSpotProvider();
		}

		[TearDown]
		public void Teardown()
		{
			_hotSpotProvider.Dispose();
		}

		[Test]
		public void Construct()
		{
			using (HotSpotProvider hotSpotProvider = new HotSpotProvider())
			{
				Assert.IsNotNull(hotSpotProvider);
			}
		}

		[Test]
		public void CanExtend_TextBox_True()
		{
			using (TextBox textBox = new TextBox())
			{
				Assert.IsTrue(((IExtenderProvider) _hotSpotProvider).CanExtend(textBox));
			}
		}

		[Test]
		public void CanExtend_RichTextBox_True()
		{
			using (RichTextBox textBox = new RichTextBox())
			{
				Assert.IsTrue(((IExtenderProvider) _hotSpotProvider).CanExtend(textBox));
			}
		}

		[Test]
		public void CanExtend_ComboBox_False()
		{
			using (ComboBox comboBox = new ComboBox())
			{
				Assert.IsFalse(((IExtenderProvider) _hotSpotProvider).CanExtend(comboBox));
			}
		}

		[Test]
		public void CanExtend_Null_False()
		{
			Assert.IsFalse(((IExtenderProvider) _hotSpotProvider).CanExtend(null));
		}

		[Test]
		public void CanExtend_CalledAfterDisposed_Throws()
		{
			HotSpotProvider hotSpotProvider = new HotSpotProvider();
			hotSpotProvider.Dispose();
			using (TextBox textBox = new TextBox())
			{
				Assert.Throws<ObjectDisposedException>(() =>
 hotSpotProvider.CanExtend(textBox));
			}
		}

		[Test]
		public void GetEnableHotSpots_Null_Throws()
		{
			Assert.Throws<ArgumentNullException>(() =>
_hotSpotProvider.GetEnableHotSpots(null));
		}

		[Test]
		public void GetEnableHotSpots_ComboBox_Throws()
		{
			using (ComboBox comboBox = new ComboBox())
			{
				Assert.Throws<ArgumentException>(() =>
_hotSpotProvider.GetEnableHotSpots(comboBox));
			}
		}

		[Test]
		public void GetEnableHotSpots_NeverSet_False()
		{
			using (TextBox textBox = new TextBox())
			{
				Assert.IsFalse(_hotSpotProvider.GetEnableHotSpots(textBox));
			}
		}

		[Test]
		public void GetEnableHotSpots_SetTrue_True()
		{
			using (TextBox textBox = new TextBox())
			{
				_hotSpotProvider.SetEnableHotSpots(textBox, true);
				Assert.IsTrue(_hotSpotProvider.GetEnableHotSpots(textBox));
			}
		}

		[Test]
		public void GetEnableHotSpots_CalledAfterDisposed_Throws()
		{
			HotSpotProvider hotSpotProvider = new HotSpotProvider();
			hotSpotProvider.Dispose();
			using (TextBox textBox = new TextBox())
			{
				Assert.Throws<ObjectDisposedException>(() =>
hotSpotProvider.GetEnableHotSpots(textBox));
			}
		}

		[Test]
		public void GetEnableHotSpots_SetFalse_False()
		{
			using (TextBox textBox = new TextBox())
			{
				_hotSpotProvider.SetEnableHotSpots(textBox, false);
				Assert.IsFalse(_hotSpotProvider.GetEnableHotSpots(textBox));
			}
		}

		[Test]
		public void SetEnableHotSpots_NullControl_EmptyString()
		{
			Assert.Throws<ArgumentNullException>(() =>
_hotSpotProvider.SetEnableHotSpots(null, true));
		}

		[Test]
		public void SetEnableHotSpots_ComboBox_Throws()
		{
			using (ComboBox comboBox = new ComboBox())
			{
				Assert.Throws<ArgumentException>(() =>
_hotSpotProvider.SetEnableHotSpots(comboBox, true));
			}
		}

		[Test]
		public void SetEnableHotSpots_SetTwice_GetSecond()
		{
			using (TextBox textBox = new TextBox())
			{
				_hotSpotProvider.SetEnableHotSpots(textBox, true);
				_hotSpotProvider.SetEnableHotSpots(textBox, false);
				Assert.IsFalse(_hotSpotProvider.GetEnableHotSpots(textBox));
			}
		}

		[Test]
		public void SetEnableHotSpots_CalledAfterDisposed_Throws()
		{
			HotSpotProvider hotSpotProvider = new HotSpotProvider();
			hotSpotProvider.Dispose();
			using (TextBox textBox = new TextBox())
			{
				Assert.Throws<ObjectDisposedException>(() =>
hotSpotProvider.SetEnableHotSpots(textBox, false));
			}
		}

		[Test]
		public void Dispose_HasDisposedEventHandler_CallsHandler()
		{
			HotSpotProvider hotSpotProvider = new HotSpotProvider();
			bool disposedCalled = false;
			hotSpotProvider.Disposed += delegate { disposedCalled = true; };
			Assert.IsFalse(disposedCalled);
			hotSpotProvider.Dispose();
			Assert.IsTrue(disposedCalled);
		}

		[Test]
		public void Dispose_CalledTwice_CallsHandlerOnce()
		{
			HotSpotProvider hotSpotProvider = new HotSpotProvider();
			int disposedCalledCount = 0;
			hotSpotProvider.Disposed += delegate { ++disposedCalledCount; };
			Assert.AreEqual(0, disposedCalledCount);
			hotSpotProvider.Dispose();
			Assert.AreEqual(1, disposedCalledCount);
			hotSpotProvider.Dispose();
			Assert.AreEqual(1, disposedCalledCount);
		}

		[Test]
		public void GetSite_CalledAfterDisposed_Throws()
		{
			HotSpotProvider hotSpotProvider = new HotSpotProvider();
			hotSpotProvider.Dispose();
			ISite x;
			Assert.Throws<ObjectDisposedException>(() =>
				 x = hotSpotProvider.Site);
		}

		[Test]
		public void SetSite_CalledAfterDisposed_Throws()
		{
			HotSpotProvider hotSpotProvider = new HotSpotProvider();
			hotSpotProvider.Dispose();
			Assert.Throws<ObjectDisposedException>(() =>
			hotSpotProvider.Site = null);
		}

		[Test]
		public void Refresh_HotSpotsAreNotEnabledOnControl_DoNothing()
		{
			using (TextBox textBox = new TextBox())
			{
				bool retrieveHotSpotsCalled = false;
				_hotSpotProvider.RetrieveHotSpots += delegate { retrieveHotSpotsCalled = true; };
				Assert.IsFalse(retrieveHotSpotsCalled);
				_hotSpotProvider.Refresh(textBox);
				Assert.IsFalse(retrieveHotSpotsCalled);
			}
		}

		[Test]
		public void Refresh_HotSpotsAreEnabled_TriggersRetrieveHotSpotEvent()
		{
			using (TextBox textBox = new TextBox())
			{
				_hotSpotProvider.SetEnableHotSpots(textBox, true);
				bool retrieveHotSpotsCalled = false;
				_hotSpotProvider.RetrieveHotSpots += delegate { retrieveHotSpotsCalled = true; };
				// reset since installing the handler causes it to fire.
				retrieveHotSpotsCalled = false;
				_hotSpotProvider.Refresh(textBox);
				Assert.IsTrue(retrieveHotSpotsCalled);
			}
		}

		[Test]
		public void RetrieveHotSpots_AddingAHandler_CausesHotSpotsToBeRetrieved()
		{
			bool handlerCalled = false;

			using (TextBox textBox = new TextBox())
			{
				_hotSpotProvider.SetEnableHotSpots(textBox, true);
				_hotSpotProvider.RetrieveHotSpots += delegate { handlerCalled = true; };
			}

			Assert.IsTrue(handlerCalled);
		}

		[Test]
		public void RefreshAll_TriggersRetrieveHotSpotEventForAllEnabled()
		{
			TextBox textBox1 = new TextBox();
			TextBox textBox2 = new TextBox();
			TextBox textBox3 = new TextBox();
			TextBox textBox4 = new TextBox();

			_hotSpotProvider.SetEnableHotSpots(textBox1, true);
			_hotSpotProvider.SetEnableHotSpots(textBox2, false);
			_hotSpotProvider.SetEnableHotSpots(textBox3, false);
			_hotSpotProvider.SetEnableHotSpots(textBox4, true);

			bool retrieveHotSpots1Called = false;
			bool retrieveHotSpots2Called = false;
			bool retrieveHotSpots3Called = false;
			bool retrieveHotSpots4Called = false;

			_hotSpotProvider.RetrieveHotSpots +=
				delegate(object sender, RetrieveHotSpotsEventArgs e)
					{
						if (e.Control == textBox1)
						{
							retrieveHotSpots1Called = true;
							return;
						}
						if (e.Control == textBox2)
						{
							retrieveHotSpots2Called = true;
							return;
						}
						if (e.Control == textBox3)
						{
							retrieveHotSpots3Called = true;
							return;
						}
						if (e.Control == textBox4)
						{
							retrieveHotSpots4Called = true;
							return;
						}
						throw new InvalidOperationException();
					};

			// reset since installing the handler causes it to fire.
			retrieveHotSpots1Called = false;
			retrieveHotSpots2Called = false;
			retrieveHotSpots3Called = false;
			retrieveHotSpots4Called = false;

			// force hotSpotProvider to retrieve HotSpots
			_hotSpotProvider.RefreshAll();

			Assert.IsTrue(retrieveHotSpots1Called);
			Assert.IsFalse(retrieveHotSpots2Called);
			Assert.IsFalse(retrieveHotSpots3Called);
			Assert.IsTrue(retrieveHotSpots4Called);

			textBox1.Dispose();
			textBox2.Dispose();
			textBox3.Dispose();
			textBox4.Dispose();
		}

	[Test]
	[NUnit.Framework.Category("KnownMonoIssue")] // review: WS-????
	public void RetrieveHotSpots_GiveSomeHotspots_HotSpotsVisible()
	{
			using (TextBox textBox = new TextBox())
			{
				textBox.Text = "Now is the time for all good men to come to the aid...";
				_hotSpotProvider.SetEnableHotSpots(textBox, true);
				_hotSpotProvider.RetrieveHotSpots +=
					delegate(object sender, RetrieveHotSpotsEventArgs e)
						{
							e.AddHotSpot(new HotSpot.HotSpot(e.Control, 7, 3));
							e.AddHotSpot(new HotSpot.HotSpot(e.Control, 16, 3));
						};

				Point position = textBox.GetPositionFromCharIndex(8);
				List<HotSpot.HotSpot> hotSpots =
					new List<HotSpot.HotSpot>(
						_hotSpotProvider.GetHotSpotsFromPosition(textBox, position));
				Assert.AreEqual(1, hotSpots.Count);
				Assert.AreEqual(7, hotSpots[0].Offset);
				Assert.AreEqual("the", hotSpots[0].Text);

				position = textBox.GetPositionFromCharIndex(16);
				hotSpots =
					new List<HotSpot.HotSpot>(
						_hotSpotProvider.GetHotSpotsFromPosition(textBox, position));
				Assert.AreEqual(1, hotSpots.Count);
				Assert.AreEqual(16, hotSpots[0].Offset);
				Assert.AreEqual("for", hotSpots[0].Text);
			}
		}

		[Test]
		public void RetrieveHotSpots_NoHotspotsReturned_NoHotSpotsVisible()
		{
			using (TextBox textBox = new TextBox())
			{
				textBox.Text = "Now is the time.";
				_hotSpotProvider.SetEnableHotSpots(textBox, true);
				_hotSpotProvider.RetrieveHotSpots += delegate
														 {
															 // give back no hot spots
														 };

				//if we scan the entire text for hot spots we shouldn't find any
				for (int i = 0;i != textBox.Text.Length;++i)
				{
					Point position = textBox.GetPositionFromCharIndex(i);
					List<HotSpot.HotSpot> hotSpots =
						new List<HotSpot.HotSpot>(
							_hotSpotProvider.GetHotSpotsFromPosition(textBox, position));
					Assert.AreEqual(0, hotSpots.Count);
				}
			}
		}
		[Test]
		public void RetrieveHotSpots_CalledWhenTheTextChanges()
		{
			using (TextBox textBox = new TextBox())
			{
				bool hotSpotsWereRetrieved;
				textBox.Text = "Now is the time.";
				_hotSpotProvider.SetEnableHotSpots(textBox, true);
				_hotSpotProvider.RetrieveHotSpots += delegate
														 {
															 hotSpotsWereRetrieved = true;
														 };
				hotSpotsWereRetrieved = false;
				textBox.Text = "For all Good Men...";
				Assert.IsTrue(hotSpotsWereRetrieved);
			}
		}
	}
}
