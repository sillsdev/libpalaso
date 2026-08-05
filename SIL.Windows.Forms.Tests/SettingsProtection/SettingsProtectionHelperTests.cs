using System.ComponentModel;
using System.Reflection;
using System.Windows.Forms;
using NUnit.Framework;
using SIL.Windows.Forms.SettingProtection;

namespace SIL.Windows.Forms.Tests.SettingsProtection
{
	[TestFixture]
	[Apartment(System.Threading.ApartmentState.STA)]
	public class SettingsProtectionHelperTests
	{
		private SettingsProtectionHelper _helper;
		private bool _savedNormallyHidden;

		[SetUp]
		public void SetUp()
		{
			_savedNormallyHidden = SettingsProtectionSingleton.Settings.NormallyHidden;
			_helper = new SettingsProtectionHelper(null);
		}

		[TearDown]
		public void TearDown()
		{
			SettingsProtectionSingleton.Settings.NormallyHidden = _savedNormallyHidden;
			_helper.Dispose();
		}

		private static void CallUpdateDisplay(SettingsProtectionHelper helper)
		{
			var method = typeof(SettingsProtectionHelper).GetMethod("UpdateDisplay",
				BindingFlags.NonPublic | BindingFlags.Instance);
			method.Invoke(helper, null);
		}

		[Test]
		public void UpdateDisplay_NormalProtectedControl_IsVisibleWhenNotNormallyHidden()
		{
			SettingsProtectionSingleton.Settings.NormallyHidden = false;
			using (var control = new Button { Visible = false })
			{
				_helper.SetSettingsProtection(control, true);

				CallUpdateDisplay(_helper);

				Assert.That(control.Visible, Is.True);
			}
		}

		[Test]
		public void UpdateDisplay_NormalProtectedControl_IsHiddenWhenNormallyHidden()
		{
			SettingsProtectionSingleton.Settings.NormallyHidden = true;
			using (var control = new Button { Visible = true })
			{
				_helper.SetSettingsProtection(control, true);

				CallUpdateDisplay(_helper);

				Assert.That(control.Visible, Is.False);
			}
		}

		[Test]
		public void UpdateDisplay_AlwaysHiddenControl_RemainsHiddenWhenNotNormallyHidden()
		{
			// Even when NormallyHidden=false (i.e., normal controls are visible),
			// an always-hidden control must stay hidden.
			SettingsProtectionSingleton.Settings.NormallyHidden = false;
			using (var control = new Button { Visible = true })
			{
				_helper.SetSettingsProtection(control, true, keepHidden: true);

				CallUpdateDisplay(_helper);

				Assert.That(control.Visible, Is.False);
			}
		}

		[Test]
		public void UpdateDisplay_AlwaysHiddenControl_RemainsHiddenWhenNormallyHidden()
		{
			SettingsProtectionSingleton.Settings.NormallyHidden = true;
			using (var control = new Button { Visible = true })
			{
				_helper.SetSettingsProtection(control, true, keepHidden: true);

				CallUpdateDisplay(_helper);

				Assert.That(control.Visible, Is.False);
			}
		}

		[Test]
		public void SetSettingsProtection_AlwaysHiddenControl_IsHiddenWithoutWaitingForTimer()
		{
			SettingsProtectionSingleton.Settings.NormallyHidden = false;
			using (var control = new Button { Visible = true })
			{
				_helper.SetSettingsProtection(control, true, keepHidden: true);

				// Deliberately no UpdateDisplay call: an always-hidden control must not stay
				// visible until the timer next fires.
				Assert.That(control.Visible, Is.False);
			}
		}

		[Test]
		public void SetSettingsProtection_SwitchFromAlwaysHiddenToNormal_ControlBecomesNormallyManaged()
		{
			SettingsProtectionSingleton.Settings.NormallyHidden = false;
			using (var control = new Button { Visible = true })
			{
				_helper.SetSettingsProtection(control, true, keepHidden: true);
				CallUpdateDisplay(_helper);
				Assert.That(control.Visible, Is.False, "Precondition: always-hidden should be hidden");

				// Re-register without keepHidden — should now follow the normal rule
				_helper.SetSettingsProtection(control, true, keepHidden: false);
				CallUpdateDisplay(_helper);

				Assert.That(control.Visible, Is.True);
			}
		}

		[Test]
		public void SetSettingsProtection_SwitchFromAlwaysHiddenToNormal_IsShownWithoutWaitingForTimer()
		{
			SettingsProtectionSingleton.Settings.NormallyHidden = false;
			using (var control = new Button { Visible = true })
			{
				_helper.SetSettingsProtection(control, true, keepHidden: true);
				Assert.That(control.Visible, Is.False, "Precondition: always-hidden should be hidden");

				_helper.SetSettingsProtection(control, true, keepHidden: false);

				// Deliberately no UpdateDisplay call: registering hid the control, so the normal
				// rule must be applied without waiting for the timer.
				Assert.That(control.Visible, Is.True);
			}
		}

		[Test]
		public void SetSettingsProtection_SwitchFromAlwaysHiddenToNormal_StaysHiddenWhenNormallyHidden()
		{
			SettingsProtectionSingleton.Settings.NormallyHidden = true;
			using (var control = new Button { Visible = true })
			{
				_helper.SetSettingsProtection(control, true, keepHidden: true);

				_helper.SetSettingsProtection(control, true, keepHidden: false);

				Assert.That(control.Visible, Is.False);
			}
		}

		[Test]
		public void SetSettingsProtection_UnprotectAlwaysHiddenControl_ControlBecomesVisible()
		{
			SettingsProtectionSingleton.Settings.NormallyHidden = false;
			using (var control = new Button { Visible = true })
			{
				_helper.SetSettingsProtection(control, true, keepHidden: true);
				CallUpdateDisplay(_helper);
				Assert.That(control.Visible, Is.False, "Precondition: always-hidden should be hidden");

				_helper.SetSettingsProtection(control, false);

				Assert.That(control.Visible, Is.True);
			}
		}

		[Test]
		public void GetSettingsProtection_AlwaysHiddenControl_ReturnsTrue()
		{
			using (var control = new Button())
			{
				_helper.SetSettingsProtection(control, true, keepHidden: true);

				Assert.That(_helper.GetSettingsProtection(control), Is.True);
			}
		}

		[Test]
		public void Dispose_WhileStillSitedInContainer_DoesNotThrow()
		{
			// Disposing removes the component from its container, which reads Site, and the Site
			// override throws once disposed; so _isDisposed must not be set until that is done.
			using (var container = new Container())
			{
				var helper = new SettingsProtectionHelper(container);

				Assert.That(() => helper.Dispose(), Throws.Nothing);
			}
		}
	}
}
