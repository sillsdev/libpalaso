// ---------------------------------------------------------------------------------------------
#region // Copyright (c) 2013, SIL International. All Rights Reserved.
// <copyright from='2013' to='2013' company='SIL International'>
//		Copyright (c) 2013, SIL International. All Rights Reserved.
//
//		Distributable under the terms of either the Common Public License or the
//		GNU Lesser General Public License, as specified in the LICENSING.txt file.
// </copyright>
#endregion
// ---------------------------------------------------------------------------------------------
using System;
using System.Collections.ObjectModel;
using Palaso.WritingSystems;

namespace Palaso.UI.WindowsForms.Keyboarding.Types
{
	/// ----------------------------------------------------------------------------------------
	/// <summary>
	/// A collection of keyboard descriptions
	/// </summary>
	/// ----------------------------------------------------------------------------------------
	public class KeyboardCollection: KeyedCollection<string, IKeyboardDefinition>
	{
		#region Overrides of KeyedCollection<string,IKeyboardDefinition>
		/// <summary>
		///Returns the key from the specified <paramref name="item"/>.
		/// </summary>
		protected override string GetKeyForItem(IKeyboardDefinition item)
		{
			return item.Id;
		}
		#endregion

		public bool Contains(string layout, string locale)
		{
			return Contains(DefaultKeyboardDefinition.GetId(locale, layout));
		}

		public IKeyboardDefinition this[string layout, string locale]
		{
			get { return this[DefaultKeyboardDefinition.GetId(locale, layout)]; }
		}
	}
}
