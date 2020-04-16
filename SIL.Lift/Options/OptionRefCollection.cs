using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace SIL.Lift.Options
{
	/// <summary>
	/// Used to refer to this option from a field
	/// </summary>
	public class OptionRefCollection : IPalasoDataObjectProperty,
									  INotifyPropertyChanged,
									  IReportEmptiness,
									  IComparable
	{
		// private readonly List<string> _keys;
		private readonly BindingList<OptionRef> _members;

		/// <summary>
		/// This "backreference" is used to notify the parent of changes.
		/// IParentable gives access to this during explicit construction.
		/// </summary>
		private IReceivePropertyChangeNotifications _parent;

		public OptionRefCollection(): this(null) {}

		public OptionRefCollection(IReceivePropertyChangeNotifications parent)
		{
			_parent = parent;
			//_keys = new List<string>();
			_members = new BindingList<OptionRef>();
			_members.ListChanged += _members_ListChanged;
		}

		private void _members_ListChanged(object sender, ListChangedEventArgs e)
		{
			NotifyPropertyChanged();
		}

		public bool IsEmpty => _members.Count == 0;

		#region ICollection<string> Members

		//        void ICollection<string>.Add(string key)
		//        {
		//            if (Keys.Contains(key))
		//            {
		//                throw new ArgumentOutOfRangeException("key", key,
		//                        "OptionRefCollection already contains that key");
		//            }
		//
		//            Add(key);
		//        }

		private OptionRef FindByKey(string key)
		{
			foreach (OptionRef _member in _members)
			{
				if (_member.Key == key)
				{
					return _member;
				}
			}
			return null;
		}

		/// <summary>
		/// Removes a key from the OptionRefCollection
		/// </summary>
		/// <param name="key">The OptionRef key to be removed</param>
		/// <returns>true when removed, false when doesn't already exists in collection</returns>
		public bool Remove(string key)
		{
			OptionRef or = FindByKey(key);
			if (or != null)
			{
				_members.Remove(or);
				NotifyPropertyChanged();
				return true;
			}
			return false;
		}

		public bool Contains(string key)
		{
			foreach (OptionRef _member in _members)
			{
				if (_member.Key == key)
				{
					return true;
				}
			}
			return false;
			//return Keys.Contains(key);
		}

		public int Count => _members.Count;

		public void Clear()
		{
			_members.Clear();
			NotifyPropertyChanged();
		}

		//        public void CopyTo(string[] array, int arrayIndex)
		//        {
		//            Keys.CopyTo(array, arrayIndex);
		//        }
		//
		//        public bool IsReadOnly
		//        {
		//            get { return false; }
		//        }
		//
		//        public IEnumerator<string> GetEnumerator()
		//        {
		//            return Keys.GetEnumerator();
		//        }
		//
		//        IEnumerator IEnumerable.GetEnumerator()
		//        {
		//            return Keys.GetEnumerator();
		//        }

		#endregion

		#region INotifyPropertyChanged Members

		/// <summary>
		/// For INotifyPropertyChanged
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged;

		#endregion

		#region IParentable Members

		public PalasoDataObject Parent
		{
			set => _parent = value;
		}

		#endregion

		protected void NotifyPropertyChanged()
		{
			//tell any data binding
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("option"));

			//tell our parent
			_parent?.NotifyPropertyChanged("option");
		}

		/// <summary>
		/// Adds a key to the OptionRefCollection
		/// </summary>
		/// <param name="key">The OptionRef key to be added</param>
		/// <returns>true when added, false when already exists in collection</returns>
		public bool Add(string key)
		{
			if (Contains(key))
			{
				return false;
			}

			_members.Add(new OptionRef(key));
			NotifyPropertyChanged();
			return true;
		}

		/// <summary>
		/// Adds a set of keys to the OptionRefCollection
		/// </summary>
		/// <param name="keys">A set of keys to be added</param>
		public void AddRange(IEnumerable<string> keys)
		{
			bool changed = false;
			foreach (string key in keys)
			{
				if (Contains(key))
				{
					continue;
				}

				Add(key);
				changed = true;
			}

			if (changed)
			{
				NotifyPropertyChanged();
			}
		}

		#region IReportEmptiness Members

		public bool ShouldHoldUpDeletionOfParentObject =>
			//this one is a conundrum.  Semantic domain gathering involves making senses
			//and adding to their semantic domain collection, without (necessarily) adding
			//a definition.  We don't want this info lost just because some eager-beaver decides
			//to clean up.
			// OTOH, we would like to have this *not* prevent deletion, if it looks like
			//the user is trying to delete the sense.
			//It will take more code to have both of these desiderata at the same time. For
			//now, we'll choose the first one, in interest of not loosing data.  It will just
			//be impossible to delete such a sense until we have SD editing.
			ShouldBeRemovedFromParentDueToEmptiness;

		public bool ShouldCountAsFilledForPurposesOfConditionalDisplay => !(IsEmpty);

		public bool ShouldBeRemovedFromParentDueToEmptiness
		{
			get
			{
				foreach (string s in Keys)
				{
					if (!string.IsNullOrEmpty(s))
					{
						return false; // one non-empty is enough to keep us around
					}
				}
				return true;
			}
		}

		public IEnumerable<string> Keys
		{
			get
			{
				foreach (OptionRef _member in _members)
				{
					yield return _member.Key;
				}
			}
		}

		public IBindingList Members => _members;

		public string KeyAtIndex(int index)
		{
			if (index < 0)
			{
				throw new ArgumentException("index");
			}
			if (index >= _members.Count)
			{
				throw new ArgumentException("index");
			}
			return _members[index].Key;
		}

		//        public IEnumerable<OptionRef> AsEnumeratorOfOptionRefs
		//        {
		//            get
		//            {
		//                foreach (string key in _keys)
		//                {
		//                    OptionRef or = new OptionRef();
		//                    or.Value = key;
		//                   yield return or;
		//                }
		//            }
		//        }

		//        public IBindingList GetConnectedBindingListOfOptionRefs()
		//        {
		//            foreach (string key in _keys)
		//                {
		//                    OptionRef or = new OptionRef();
		//                    or.Key = key;
		//                    or.Parent = (PalasoDataObject) _parent ;
		//
		//                    _optionRefProxyList.Add(or);
		//                }
		//
		//        }

		public void RemoveEmptyStuff()
		{
			List<OptionRef> condemened = new List<OptionRef>();
			foreach (OptionRef _member in _members)
			{
				if (_member.IsEmpty)
				{
					condemened.Add(_member);
				}
			}
			foreach (OptionRef or in condemened)
			{
				Members.Remove(or);
			}
		}

		#endregion

		public int CompareTo(object obj)
		{
			if (obj == null)
			{
				return 1;
			}
			if (!(obj is OptionRefCollection))
			{
				throw new ArgumentException($"Can not compare to anything but {nameof(OptionRefCollection)}s.");
			}
			OptionRefCollection other = (OptionRefCollection) obj;
			int order = _members.Count.CompareTo(other.Members.Count);
			if(order != 0)
			{
				return order;
			}
			for (int i = 0; i < _members.Count; i++)
			{
				order = _members[i].CompareTo(other.Members[i]);
				if(order != 0)
				{
					return order;
				}
			}
			return 0;
		}

		public override string ToString()
		{
			var builder = new StringBuilder();
			foreach (var optionRef in _members)
			{
				builder.Append(optionRef.ToString()+", ");
			}
			return builder.ToString().TrimEnd(new char[] {',', ' '});
		}

		public bool MergeByKey(OptionRefCollection incoming)
		{
			var combined = new List<OptionRef>(_members);
			foreach (OptionRef optionRef in incoming.Members)
			{
				var match = this.FindByKey(optionRef.Key);
				if(match == null)
				{
					combined.Add(optionRef);
				}
				else
				{   //now, if we don't have embedded xml and they do, take on theirs
					if (match.EmbeddedXmlElements == null || match.EmbeddedXmlElements.Count == 0)
						match.EmbeddedXmlElements = optionRef.EmbeddedXmlElements;
					else if(optionRef.EmbeddedXmlElements.Count>0)
					{
						return false; //we don't know how to combine these
					}
				}
			}
			_members.Clear();
			foreach (var optionRef in combined)
			{
				_members.Add(optionRef);
			}
			return true;
		}

		public IPalasoDataObjectProperty Clone()
		{
			var clone = new OptionRefCollection();
			foreach (var memberToClone in _members)
			{
				clone._members.Add((OptionRef) memberToClone.Clone());
			}
			return clone;
		}

		public override bool Equals(object obj)
		{
			return Equals(obj as OptionRefCollection);
		}

		public bool Equals(IPalasoDataObjectProperty other)
		{
			return Equals(other as OptionRefCollection);
		}

		public bool Equals(OptionRefCollection other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			if (!_members.SequenceEqual(other._members)) return false;
			return true;
		}

		public override int GetHashCode()
		{
			// https://stackoverflow.com/a/263416/487503
			unchecked // Overflow is fine, just wrap
			{
				var hash = 23;
				hash *= 29 + _members.GetHashCode();
				return hash;
			}
		}
	}
}