// ---------------------------------------------------------------------------------------------
#region // Copyright 2024 SIL GlobalAll Rights Reserved.
// <copyright from='2013' to='2024' company='SIL Global'>
//		Copyright (c) 2013, SIL International. All Rights Reserved.
//
//		Distributable under the terms of either the Common Public License or the
//		GNU Lesser General Public License, as specified in the LICENSING.txt file.
// </copyright>
#endregion
// ---------------------------------------------------------------------------------------------
using System.Windows.Forms;
using NUnit.Framework;
using SIL.Keyboarding;

namespace SIL.Windows.Forms.Keyboarding.Tests
{
	[TestFixture]
	public class InputLanguageWrapperTests
	{
		[Test]
		public void CanCastToIInputLanguage()
		{
			Assert.That(InputLanguage.CurrentInputLanguage.Interface(), Is.InstanceOf<IInputLanguage>());
		}

		[Test]
		public void WrapperAndOriginalAreEqual()
		{
			var wrapper = new InputLanguageWrapper(InputLanguage.CurrentInputLanguage);
			Assert.That(wrapper.Equals(InputLanguage.CurrentInputLanguage));
		}

		[Test]
		public void ImplicitCastToInputLanguageWrapper()
		{
			InputLanguageWrapper wrapper = InputLanguage.CurrentInputLanguage;
			Assert.That(wrapper, Is.Not.Null);
		}
	}
}
