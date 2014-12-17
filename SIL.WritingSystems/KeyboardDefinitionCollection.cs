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
namespace SIL.WritingSystems
{
	/// ----------------------------------------------------------------------------------------
	/// <summary>
	/// A collection of keyboard descriptions
	/// </summary>
	/// ----------------------------------------------------------------------------------------
	public class KeyboardDefinitionCollection : ObservableKeyedCollection<string, IKeyboardDefinition>
	{
		/// <summary>
		///Returns the key from the specified <paramref name="item"/>.
		/// </summary>
		protected override string GetKeyForItem(IKeyboardDefinition item)
		{
			return item.Id;
		}

		protected override void InsertItem(int index, IKeyboardDefinition item)
		{
			if (Contains(item.Id))
			{
				IKeyboardDefinition oldItem = this[item.Id];
				int oldIndex = IndexOf(oldItem);
				if (oldIndex == index - 1 && oldItem == item)
					return;
				RemoveAt(oldIndex);
				if (index > oldIndex)
					index--;
			}
			base.InsertItem(index, item);
		}

		public bool TryGetKeyboardDefinition(string id, out IKeyboardDefinition kd)
		{
			if (Contains(id))
			{
				kd = this[id];
				return true;
			}

			kd = null;
			return false;
		}
	}
}
