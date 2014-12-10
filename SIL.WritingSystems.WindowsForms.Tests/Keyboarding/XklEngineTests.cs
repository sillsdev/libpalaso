// Copyright (c) 2013, SIL International. All Rights Reserved.
//
// Distributable under the terms of either the Common Public License or the
// GNU Lesser General Public License, as specified in the LICENSING.txt file.
//
// Original author: MarkS 2013-01-04 XklEngineTests.cs

namespace SIL.WritingSystems.WindowsForms.Tests.Keyboarding
{
#if __MonoCS__
using System;
using NUnit.Framework;
using Palaso.UI.WindowsForms.Keyboarding.Linux;
using X11.XKlavier;

namespace PalasoUIWindowsForms.Tests.Keyboarding
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
#endif
}