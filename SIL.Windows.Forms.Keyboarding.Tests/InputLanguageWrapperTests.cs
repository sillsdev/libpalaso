// ---------------------------------------------------------------------------------------------
#region // Copyright (c) 2025 SIL Global
// <copyright from='2013' to='2024' company='SIL Global'>
//		Copyright (c) 2025 SIL Global
//
//		Distributable under the terms of the MIT License (http://sil.mit-license.org/)
// </copyright>
#endregion
//
// This class originated in FieldWorks (under the GNU Lesser General Public License), but we
// decided to move the class it tests into SIL.Windows.Forms.Keyboarding to make it more readily
// available to other projects.
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
