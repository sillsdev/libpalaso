// Copyright (c) 2009-2024 SIL Global
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System.ComponentModel;

namespace SIL.Lift
{
	/// <summary>
	/// This class enables creating the necessary event subscriptions. It was added
	/// before we were forced to add "parent" fields to everything.  I could probably
	/// be removed now, since that field could be used by children to cause the wiring,
	/// but we are hoping that the parent field might go away with future version of db4o.
	/// </summary>
	public class ListEventHelper
	{
		private readonly string           _listName;
		private readonly PalasoDataObject _listOwner;

		public ListEventHelper(PalasoDataObject listOwner, IBindingList list, string listName)
		{
			_listOwner = listOwner;
			_listName = listName;
			list.ListChanged += OnListChanged;
			foreach (INotifyPropertyChanged x in list)
			{
				_listOwner.WireUpChild(x);
			}
		}

		private void OnListChanged(object sender, ListChangedEventArgs e)
		{
			if (e.ListChangedType == ListChangedType.ItemAdded)
			{
				IBindingList list = (IBindingList) sender;
				INotifyPropertyChanged newGuy = (INotifyPropertyChanged) list[e.NewIndex];
				_listOwner.WireUpChild(newGuy);
				if (newGuy is PalasoDataObject dataObject)
					dataObject.Parent = _listOwner;
			}
			_listOwner.NotifyPropertyChanged(_listName);
		}
	}
}