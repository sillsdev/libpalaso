// Copyright (c) 2013-2015 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

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
