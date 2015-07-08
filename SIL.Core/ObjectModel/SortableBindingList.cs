using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using SIL.Extensions;

namespace SIL.ObjectModel
{
	// Taken and modified from http://betimvwframework.codeplex.com/
	// Apache License 2.0: http://betimvwframework.codeplex.com/license

	/// <summary>
	/// Sortable version of BindingList. (Can be used to allow sorting in DataGridViews.)
	/// </summary>
	public class SortableBindingList<T> : BindingList<T>
	{
		private readonly Dictionary<Type, PropertyComparer<T>> _comparers;
		private bool _isSorted;
		private ListSortDirection _listSortDirection;
		private PropertyDescriptor _propertyDescriptor;

		public SortableBindingList()
		{
			_comparers = new Dictionary<Type, PropertyComparer<T>>();
		}

		public SortableBindingList(IList<T> list)
			: base(list)
		{
			_comparers = new Dictionary<Type, PropertyComparer<T>>();
		}

		protected override bool SupportsSortingCore
		{
			get { return true; }
		}

		protected override bool IsSortedCore
		{
			get { return _isSorted; }
		}

		protected override PropertyDescriptor SortPropertyCore
		{
			get { return _propertyDescriptor; }
		}

		protected override ListSortDirection SortDirectionCore
		{
			get { return _listSortDirection; }
		}

		protected override bool SupportsSearchingCore
		{
			get { return true; }
		}

		protected override void ApplySortCore(PropertyDescriptor property, ListSortDirection direction)
		{
			Type propertyType = property.PropertyType;
			PropertyComparer<T> comparer;
			if (!_comparers.TryGetValue(propertyType, out comparer))
			{
				comparer = new PropertyComparer<T>(property, direction);
				_comparers.Add(propertyType, comparer);
			}

			comparer.SetPropertyAndDirection(property, direction);
			Items.Sort(comparer);

			_propertyDescriptor = property;
			_listSortDirection = direction;
			_isSorted = true;

			OnListChanged(new ListChangedEventArgs(ListChangedType.Reset, -1));
		}

		protected override void RemoveSortCore()
		{
			_isSorted = false;
			_propertyDescriptor = base.SortPropertyCore;
			_listSortDirection = base.SortDirectionCore;

			OnListChanged(new ListChangedEventArgs(ListChangedType.Reset, -1));
		}

		protected override int FindCore(PropertyDescriptor property, object key)
		{
			int count = Count;
			for (int i = 0; i < count; ++i)
			{
				T element = this[i];
				if (property.GetValue(element).Equals(key))
				{
					return i;
				}
			}

			return -1;
		}
	}

	public class PropertyComparer<T> : IComparer<T>
	{
		private readonly IComparer _comparer;
		private PropertyDescriptor _propertyDescriptor;
		private int _reverse;

		public PropertyComparer(PropertyDescriptor property, ListSortDirection direction)
		{
			_propertyDescriptor = property;
			Type comparerForPropertyType = typeof(Comparer<>).MakeGenericType(property.PropertyType);
			_comparer = (IComparer)comparerForPropertyType.InvokeMember("Default", BindingFlags.Static | BindingFlags.GetProperty | BindingFlags.Public, null, null, null);
			SetListSortDirection(direction);
		}

		#region IComparer<T> Members
		public int Compare(T x, T y)
		{
			return _reverse * _comparer.Compare(_propertyDescriptor.GetValue(x), _propertyDescriptor.GetValue(y));
		}
		#endregion

		private void SetPropertyDescriptor(PropertyDescriptor descriptor)
		{
			_propertyDescriptor = descriptor;
		}

		private void SetListSortDirection(ListSortDirection direction)
		{
			_reverse = direction == ListSortDirection.Ascending ? 1 : -1;
		}

		public void SetPropertyAndDirection(PropertyDescriptor descriptor, ListSortDirection direction)
		{
			SetPropertyDescriptor(descriptor);
			SetListSortDirection(direction);
		}
	}
}
