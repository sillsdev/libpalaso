using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using SIL.Lift.Options;
using SIL.ObjectModel;
using SIL.Reporting;
using SIL.Text;

namespace SIL.Lift
{
	public interface IPalasoDataObjectProperty : ICloneable<IPalasoDataObjectProperty>, IEquatable<IPalasoDataObjectProperty>
	{
		PalasoDataObject Parent { set; }
	}

	public interface IReceivePropertyChangeNotifications
	{
		void NotifyPropertyChanged(string property);
	}

	public interface IReferenceContainer
	{
		string TargetId { get; set; }
		string Key { get; set; }
	}

	public abstract class PalasoDataObject: INotifyPropertyChanged,
											IReceivePropertyChangeNotifications
	{
		[NonSerialized]
		private ArrayList _listEventHelpers;

		/// <summary>
		/// see comment on _parent field of MultiText for an explanation of this field
		/// </summary>
		private PalasoDataObject _parent;

		private List<KeyValuePair<string, IPalasoDataObjectProperty>> _properties;

		protected PalasoDataObject(PalasoDataObject parent)
		{
			_properties = new List<KeyValuePair<string, IPalasoDataObjectProperty>>();
			_parent = parent;
		}

		public IEnumerable<string> PropertiesInUse
		{
			get { return Properties.Select(prop => prop.Key); }
		}

		public abstract bool IsEmpty { get; }

		/// <summary>
		/// see comment on _parent field of MultiText for an explanation of this field
		/// </summary>
		public PalasoDataObject Parent
		{
			get { return _parent; }
			set
			{
				Debug.Assert(value != null);
				_parent = value;
			}
		}

		public List<KeyValuePair<string, IPalasoDataObjectProperty>> Properties
		{
			get
			{
				if (_properties == null)
				{
					_properties = new List<KeyValuePair<string, IPalasoDataObjectProperty>>();
					NotifyPropertyChanged("properties dictionary");
				}

				return _properties;
			}
		}

		public bool HasProperties
		{
			get
			{
				foreach (KeyValuePair<string, IPalasoDataObjectProperty> pair in _properties)
				{
					if (!IsPropertyEmpty(pair.Value))
					{
						return true;
					}
				}
				return false;
			}
		}

		public bool HasPropertiesForPurposesOfDeletion
		{
			get
			{ return _properties.Any(pair => !IsPropertyEmptyForPurposesOfDeletion(pair.Value)); }
		}

		#region INotifyPropertyChanged Members

		/// <summary>
		/// For INotifyPropertyChanged
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged = delegate { };

		#endregion

		public event EventHandler EmptyObjectsRemoved = delegate { };

		/// <summary>
		/// Do the non-db40-specific parts of becoming activated
		/// </summary>
		public void FinishActivation()
		{
			EmptyObjectsRemoved = delegate { };
			WireUpEvents();
		}

		protected void WireUpList(IBindingList list, string listName)
		{
			_listEventHelpers.Add(new ListEventHelper(this, list, listName));
		}

		protected virtual void WireUpEvents()
		{
			_listEventHelpers = new ArrayList();
			PropertyChanged += OnPropertyChanged;
		}

		private void OnEmptyObjectsRemoved(object sender, EventArgs e)
		{
			// perculate up
			EmptyObjectsRemoved(sender, e);
		}

		protected void OnEmptyObjectsRemoved()
		{
			EmptyObjectsRemoved(this, new EventArgs());
		}

		private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			SomethingWasModified(e.PropertyName);
		}

		public void WireUpChild(INotifyPropertyChanged child)
		{
			child.PropertyChanged -= OnChildObjectPropertyChanged;
			//prevent the bug where we were acquiring these with each GetProperty<> call
			child.PropertyChanged += OnChildObjectPropertyChanged;
			if (child is PalasoDataObject)
			{
				((PalasoDataObject) child).EmptyObjectsRemoved += OnEmptyObjectsRemoved;
			}
		}

		/// <summary>
		/// called by the binding list when senses are added, removed, reordered, etc.
		/// Also called when the user types in fields, etc.
		/// </summary>
		/// <remarks>The only side effect of this should be to update the dateModified fields</remarks>
		public virtual void SomethingWasModified(string propertyModified)
		{
			//NO: can't do this until really putting the record to bed;
			//only the display code knows when to do that.      RemoveEmptyProperties();
		}

		public virtual void CleanUpAfterEditting()
		{
			RemoveEmptyProperties();
		}

		public virtual void CleanUpEmptyObjects() {}

		/// <summary>
		/// BE CAREFUL about when this is called. Empty properties *should exist*
		/// as long as the record is being editted
		/// </summary>
		public void RemoveEmptyProperties()
		{
			// remove any custom fields that are empty
			int originalCount = Properties.Count;

			for (int i = originalCount - 1;i >= 0;i--) // NB: counting backwards
			{
				//trying to reproduce ws-564
				Debug.Assert(Properties.Count > i, "Likely hit the ws-564 bug.");

				if (Properties.Count <= i)
				{
					ErrorReport.ReportNonFatalMessageWithStackTrace(
						"The number of properties was orginally {0}, is now {1}, but the index is {2}. PLEASE help us reproduce this bug.",
						originalCount,
						Properties.Count,
						i);
				}

				object property = Properties[i].Value;
				if (property is IReportEmptiness)
				{
					((IReportEmptiness) property).RemoveEmptyStuff();
				}

				if (IsPropertyEmpty(property))
				{
					Logger.WriteMinorEvent("Removing {0} due to emptiness.", property.ToString());
					Properties.RemoveAt(i);
					// don't: this just makes for false modified events: NotifyPropertyChanged(property.ToString());
				}
			}
		}

		private static bool IsPropertyEmpty(object property)
		{
			if (property is MultiText)
			{
				return MultiTextBase.IsEmpty((MultiText) property);
			}
			else if (property is OptionRef)
			{
				return ((OptionRef) property).IsEmpty;
			}
			else if (property is OptionRefCollection)
			{
				return ((OptionRefCollection) property).IsEmpty;
			}
			else if (property is IReportEmptiness)
			{
				return ((IReportEmptiness) property).ShouldBeRemovedFromParentDueToEmptiness;
			}
			//            Debug.Fail("Unknown property type");
			return false; //don't throw it away if you don't know what it is
		}

		private static bool IsPropertyEmptyForPurposesOfDeletion(object property)
		{
			if (property is MultiText)
			{
				return IsPropertyEmpty(property);
			}
			else if (property is OptionRef)
			{
				return true;
			}
			else if (property is OptionRefCollection)
			{
				return ((OptionRefCollection) property).ShouldHoldUpDeletionOfParentObject;
			}
			else if (property is IReportEmptiness)
			{
				return IsPropertyEmpty(property);
			}
			return false; //don't throw it away if you don't know what it is
		}

		public virtual void NotifyPropertyChanged(string propertyName)
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		protected virtual void OnChildObjectPropertyChanged(object sender,
															PropertyChangedEventArgs e)
		{
			NotifyPropertyChanged(e.PropertyName);
		}

		public TContents GetOrCreateProperty<TContents>(string fieldName)
			where TContents : class, IPalasoDataObjectProperty, new()
		{
			TContents value = GetProperty<TContents>(fieldName);
			if (value != null)
			{
				return value;
			}

			TContents newGuy = new TContents();
			//Properties.Add(fieldName, newGuy);
			Properties.Add(new KeyValuePair<string, IPalasoDataObjectProperty>(fieldName, newGuy));
			newGuy.Parent = this;

			//temp hack until mt's use parents for notification
			if (newGuy is MultiText)
			{
				WireUpChild((INotifyPropertyChanged) newGuy);
			}

			return newGuy;
		}

		protected void AddProperty(string fieldName, IPalasoDataObjectProperty field)
		{
			Properties.Add(new KeyValuePair<string, IPalasoDataObjectProperty>(fieldName, field));
			field.Parent = this;

			//temp hack until mt's use parents for notification
			if (field is MultiText)
			{
				WireUpChild((INotifyPropertyChanged)field);
			}
		}

		/// <summary>
		/// Will return null if not found
		/// </summary>
		/// <typeparam name="TContents"></typeparam>
		/// <returns>null if not found</returns>
		public TContents GetProperty<TContents>(string fieldName) where TContents : class
			//, IParentable
		{
			KeyValuePair<string, IPalasoDataObjectProperty> found = Properties.Find(p => p.Key == fieldName);
			if (found.Key == fieldName)
			{
				Debug.Assert(found.Value is  TContents, "Currently we assume that there is only a single type of object for a given name.");

				//temp hack until mt's use parents for notification);on
				if (found.Value is MultiText)
				{
					WireUpChild((INotifyPropertyChanged) found.Value);
				}
				return found.Value as TContents;
			}
			return null;
		}


		/// <summary>
		/// Merge in a property from some other object, e.g., when merging senses
		/// </summary>
		public void MergeProperty(KeyValuePair<string, IPalasoDataObjectProperty> incoming)
		{
			KeyValuePair<string, IPalasoDataObjectProperty> existing = Properties.Find(
				p => p.Key == incoming.Key
			);

			if (existing.Value is OptionRefCollection)
			{
				if (existing.Key == incoming.Key)
				{
					var optionRefCollection = existing.Value as OptionRefCollection;
					var incomingRefCollection = incoming.Value as OptionRefCollection;
					optionRefCollection.MergeByKey(incomingRefCollection);
				} else
				{
					Properties.Add(new KeyValuePair<string, IPalasoDataObjectProperty>(incoming.Key, incoming.Value));
				}
			}
			else
			{
				if (existing.Key == incoming.Key)
				{
					Properties.Remove(existing);
				}

				Properties.Add(new KeyValuePair<string, IPalasoDataObjectProperty>(incoming.Key, incoming.Value));
			}

			incoming.Value.Parent = this;

			//temp hack until mt's use parents for notification
			if (incoming.Value is MultiText)
			{
				WireUpChild((INotifyPropertyChanged)incoming.Value);
			}
		}


		public bool GetHasFlag(string propertyName)
		{
			FlagState flag = GetProperty<FlagState>(propertyName);
			if (flag == null)
			{
				return false;
			}
			return flag.Value;
		}

		/// <summary>
		///
		/// </summary>
		///<remarks>Seting a flag is represented by creating a property and giving it a "set"
		/// value, though that is not really meaningful (there are no other possible values).</remarks>
		/// <param name="propertyName"></param>
		public void SetFlag(string propertyName)
		{
			FlagState f = GetOrCreateProperty<FlagState>(propertyName);
			f.Value = true;
			//            KeyValuePair<FlagState, object> found = Properties.Find(delegate(KeyValuePair<FlagState, object> p) { return p.Key == propertyName; });
			//            if (found.Key == propertyName)
			//            {
			//                _properties.Remove(found);
			//            }
			//
			//            Properties.Add(new KeyValuePair<string, object>(propertyName, "set"));
		}

		/// <summary>
		///
		/// </summary>
		/// <remarks>Clearing a flag is represented by just removing the property, if it exists</remarks>
		/// <param name="propertyName"></param>
		public void ClearFlag(string propertyName)
		{
			KeyValuePair<string, IPalasoDataObjectProperty> found = Properties.Find(p => p.Key == propertyName);
			if (found.Key == propertyName)
			{
				_properties.Remove(found);
			}
		}

		#region Nested type: WellKnownProperties

		public class WellKnownProperties
		{
			public static string Note = "note";

			public static bool Contains(string fieldName)
			{
				List<string> list = new List<string>(new string[] {Note});
				return list.Contains(fieldName);
			}
		} ;

		#endregion

		public static string GetEmbeddedXmlNameForProperty(string name)
		{
			return name + "-xml";
		}

		public override bool Equals(Object obj)
		{
			return obj is PalasoDataObject palasoDataObject && Equals(palasoDataObject);
		}

		public bool Equals(PalasoDataObject other)
		{
			if (ReferenceEquals(null, other))
				return false;
			return ReferenceEquals(this, other) || _properties.SequenceEqual(other._properties);
		}
	}

	public interface IReportEmptiness
	{
		bool ShouldHoldUpDeletionOfParentObject { get; }
		bool ShouldCountAsFilledForPurposesOfConditionalDisplay { get; }

		bool ShouldBeRemovedFromParentDueToEmptiness { get; }

		void RemoveEmptyStuff();
	}

	/// <summary>
	/// This class enables creating the necessary event subscriptions. It was added
	/// before we were forced to add "parent" fields to everything.  I could probably
	/// be removed now, since that field could be used by children to cause the wiring,
	/// but we are hoping that the parent field might go away with future version of db4o.
	/// </summary>
	public class ListEventHelper
	{
		private readonly string _listName;
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
				if (newGuy is PalasoDataObject)
				{
					((PalasoDataObject) newGuy).Parent = _listOwner;
				}
			}
			_listOwner.NotifyPropertyChanged(_listName);
		}
	}

	public class EmbeddedXmlCollection: IPalasoDataObjectProperty
	{
		private List<string> _values;
		private PalasoDataObject _parent;

		public EmbeddedXmlCollection()
		{
			_values = new List<string>();
		}

		public PalasoDataObject Parent
		{
			set { _parent = value; }
		}

		public List<string> Values
		{
			get { return _values; }
			set { _values = value; }
		}

		public IPalasoDataObjectProperty Clone()
		{
			var clone = new EmbeddedXmlCollection();
			clone._values.AddRange(_values);
			return clone;
		}

		public override bool Equals(object other)
		{
			return Equals((EmbeddedXmlCollection)other);
		}

		public bool Equals(IPalasoDataObjectProperty other)
		{
			return Equals((EmbeddedXmlCollection) other);
		}

		public bool Equals(EmbeddedXmlCollection other)
		{
			if (other == null) return false;
			if (!_values.SequenceEqual(other._values)) return false; //order is relevant
			return true;
		}

		public override string ToString()
		{
			var builder = new StringBuilder();
			foreach (var part in Values)
			{
				builder.Append(part.ToString() + " ");
			}
			return builder.ToString().Trim();
		}
	}
}