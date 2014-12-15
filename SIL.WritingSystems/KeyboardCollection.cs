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
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace SIL.WritingSystems
{
	/// ----------------------------------------------------------------------------------------
	/// <summary>
	/// A collection of keyboard descriptions
	/// </summary>
	/// ----------------------------------------------------------------------------------------
	public class KeyboardCollection: KeyedCollection<string, IKeyboardDefinition>, INotifyCollectionChanged
	{
		public event NotifyCollectionChangedEventHandler CollectionChanged;

		#region Overrides of KeyedCollection<string,IKeyboardDefinition>
		/// <summary>
		///Returns the key from the specified <paramref name="item"/>.
		/// </summary>
		protected override string GetKeyForItem(IKeyboardDefinition item)
		{
			return item.Id;
		}

		protected override void ClearItems()
		{
			base.ClearItems();
			OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
		}

		protected override void InsertItem(int index, IKeyboardDefinition item)
		{
			if (item == null || Contains(item.Id))
				return;

			base.InsertItem(index, item);
			OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));
		}

		protected override void RemoveItem(int index)
		{
			IKeyboardDefinition oldItem = this[index];
			base.RemoveItem(index);
			OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, oldItem, index));
		}

		protected override void SetItem(int index, IKeyboardDefinition item)
		{
			IKeyboardDefinition oldItem = this[index];
			base.SetItem(index, item);
			OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, item, oldItem, index));
		}

		#endregion

		protected void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
		{
			if (CollectionChanged != null)
				CollectionChanged(this, e);
		}

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
