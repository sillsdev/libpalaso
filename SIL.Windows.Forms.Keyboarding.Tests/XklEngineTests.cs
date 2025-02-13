// ---------------------------------------------------------------------------------------------
#region // Copyright (c) 2025 SIL Global
// <copyright from='2013' to='2025' company='SIL Global'>
//		Copyright (c) 2025 SIL Global
//
//		Distributable under the terms of the MIT License (http://sil.mit-license.org/)
// </copyright>
#endregion
//
// This class originated in FieldWorks (under the GNU Lesser General Public License), but we
// decided to make it available in SIL.Scripture to make it more readily available to other
// projects.
//
// Original author: MarkS 2013-01-04 XklEngineTests.cs
// ---------------------------------------------------------------------------------------------
using System;
using NUnit.Framework;
using X11.XKlavier;

namespace SIL.Windows.Forms.Keyboarding.Tests
{
	[TestFixture]
	[Platform(Include="Linux", Reason="Linux specific tests")]
	[SetUICulture("en-US")]
	public class XklEngineTests
	{
		/// <summary>
		/// Can be created and closed. Doesn't crash.
		/// </summary>
		[Test]
		public void Basic()
		{
			var engine = new XklEngine();
			engine.Close();
		}

		/// <summary/>
		[Test]
		public void UseAfterClose_NotCrash()
		{
			var engine = new XklEngine();
			engine.Close();
			engine.SetGroup(0);
		}

		/// <summary/>
		[Test]
		public void MultipleEngines_ClosedInReverseOrder_NotCrash()
		{
			var engine1 = new XklEngine();
			var engine2 = new XklEngine();
			engine2.Close();
			engine1.Close();
		}

		/// <summary/>
		[Test]
		public void MultipleEngines_ClosedInOpenOrder_NotCrash()
		{
			var engine1 = new XklEngine();
			var engine2 = new XklEngine();
			engine1.Close();
			engine2.Close();
		}

		/// <summary/>
		[Test]
		public void GetDisplayConnection()
		{
			var displayConnection = X11.X11Helper.GetDisplayConnection();
			Assert.That(displayConnection, Is.Not.EqualTo(IntPtr.Zero), "Expected display connection");
		}
	}
}
