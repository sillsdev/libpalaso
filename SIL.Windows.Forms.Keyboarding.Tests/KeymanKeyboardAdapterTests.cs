// ---------------------------------------------------------------------------------------------
#region // Copyright (c) 2024 SIL Global.
// <copyright from='2011' to='2024' company='SIL Global'>
//		Copyright (c) 2024, SIL Global.
//
//		Distributable under the terms of the MIT License (http://sil.mit-license.org/)
// </copyright>
#endregion
// ---------------------------------------------------------------------------------------------
using NUnit.Framework;
using SIL.Windows.Forms.Keyboarding.Windows;

namespace SIL.Windows.Forms.Keyboarding.Tests
{
	[TestFixture]
	[SetUICulture("en-US")]
	public class KeymanKeyboardAdapterTests
	{
		[Test]
		public void CanCreate()
		{
			var adapter = new KeymanKeyboardAdaptor();
			Assert.IsNotNull(adapter);
		}
	}
}
