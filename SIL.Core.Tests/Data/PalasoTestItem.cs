using System;
using System.Collections.Generic;
using System.ComponentModel;
using SIL.Data;

namespace SIL.Tests.Data
{
	public class PalasoChildTestItem
	{
		private PalasoChildTestItem _testItem;

		public PalasoChildTestItem() {}

		public PalasoChildTestItem(string s, int i, DateTime d)
		{
			StoredInt = i;
			StoredString = s;
			StoredDateTime = d;
		}

		public PalasoChildTestItem Child
		{
			get { return _testItem; }
			set { _testItem = value; }
		}

		public int Depth
		{
			get
			{
				int depth = 1;
				PalasoChildTestItem item = Child;
				while (item != null)
				{
					++depth;
					item = item.Child;
				}
				return depth;
			}
		}

		public int StoredInt { get; set; }

		public string StoredString { get; set; }

		public DateTime StoredDateTime { get; set; }
	}

	public class PalasoTestItem: INotifyPropertyChanged
	{
		private int _storedInt;
		private string _storedString;
		private DateTime _storedDateTime;
		private PalasoChildTestItem _childTestItem;
		private List<string> _storedList;

		private List<PalasoChildTestItem> _childItemList;

		public List<PalasoChildTestItem> ChildItemList
		{
			get { return _childItemList; }
			set
			{
				_childItemList = value;
				OnPropertyChanged(new PropertyChangedEventArgs("Children"));
			}
		}

		public int OnActivateDepth { get; private set; }

		public PalasoChildTestItem Child
		{
			get { return _childTestItem; }
			set
			{
				_childTestItem = value;
				OnPropertyChanged(new PropertyChangedEventArgs("Child"));
			}
		}


		public int Depth
		{
			get
			{
				int depth = 1;
				PalasoChildTestItem item = Child;
				while (item != null)
				{
					++depth;
					item = item.Child;
				}
				return depth;
			}
		}

		public PalasoTestItem()
		{
			_storedDateTime = PreciseDateTime.UtcNow;
			_childTestItem = new PalasoChildTestItem();
		}

		public PalasoTestItem(string s, int i, DateTime d)
		{
			_storedString = s;
			_storedInt = i;
			_storedDateTime = d;
		}

		public override string ToString()
		{
			return StoredInt + ". " + StoredString + " " + StoredDateTime;
		}

		public override bool Equals(object obj)
		{
			if (obj == null)
			{
				return false;
			}
			var item = obj as PalasoTestItem;
			if (item == null)
			{
				return false;
			}

			return Equals(item);
		}

		public bool Equals(PalasoTestItem item)
		{
			if (item == null)
			{
				return false;
			}

			return (_storedInt == item._storedInt) && (_storedString == item._storedString) &&
				   (_storedDateTime == item._storedDateTime);
		}

		public int StoredInt
		{
			get { return _storedInt; }
			set
			{
				if (_storedInt != value)
				{
					_storedInt = value;
					OnPropertyChanged(new PropertyChangedEventArgs("StoredInt"));
				}
			}
		}

		public string StoredString
		{
			get { return _storedString; }
			set
			{
				if (_storedString != value)
				{
					_storedString = value;
					OnPropertyChanged(new PropertyChangedEventArgs("StoredString"));
				}
			}
		}

		public DateTime StoredDateTime
		{
			get { return _storedDateTime; }
			set
			{
				if (_storedDateTime != value)
				{
					_storedDateTime = value;
					OnPropertyChanged(new PropertyChangedEventArgs("StoredDateTime"));
				}
			}
		}

		public List<string> StoredList
		{
			get { return _storedList; }
			set
			{
				_storedList = value;
				OnPropertyChanged(new PropertyChangedEventArgs("StoredList"));
			}
		}

		#region INotifyPropertyChanged Members

		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, e);
			}
		}

		#endregion
	}
}