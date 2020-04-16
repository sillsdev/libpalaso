using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using SIL.Annotations;
using SIL.UiBindings;

namespace SIL.Lift.Options
{
	/// <summary>
	/// Used to refer to this option from a field.
	/// This class just wraps the key, which is a string, with various methods to make it fit in
	/// with the system.
	/// </summary>
	public class OptionRef: Annotatable,
							IPalasoDataObjectProperty,
							IValueHolder<string>,
							IReportEmptiness,
							IReferenceContainer,
							IComparable

	{
		private string _humanReadableKey;

		/// <summary>
		/// This "backreference" is used to notify the parent of changes.
		/// IParentable gives access to this during explicit construction.
		/// </summary>
		private IReceivePropertyChangeNotifications _parent;

		public List<string> EmbeddedXmlElements = new List<string>();


		private bool _suspendNotification;

		public OptionRef(): this(string.Empty) {}

		public OptionRef(string key) //WeSay.Foundation.PalasoDataObject parent)
		{
			_humanReadableKey = key;
		}

		public bool IsEmpty => string.IsNullOrEmpty(Value);

		#region IParentable Members

		public PalasoDataObject Parent
		{
			set => _parent = value;
		}

		#endregion

		#region IValueHolder<string> Members

		/// <summary>
		/// For INotifyPropertyChanged
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged;

		public string Key
		{
			get => Value;
			set => Value = value;
		}

		public string Value
		{
			get => _humanReadableKey;
			set
			{
				if (value != null)
				{
					_humanReadableKey = value.Trim();
				}
				else
				{
					_humanReadableKey = null;
				}
				// this.Guid = value.Guid;
				NotifyPropertyChanged();
			}
		}

		// IReferenceContainer
		public string TargetId
		{
			get => _humanReadableKey;
			set
			{
				if (value == _humanReadableKey ||
					(value == null && _humanReadableKey == string.Empty))
				{
					return;
				}

				if (value == null)
				{
					_humanReadableKey = string.Empty;
				}
				else
				{
					_humanReadableKey = value;
				}
				NotifyPropertyChanged();
			}
		}

		public void SetTarget(Option o)
		{
			if (o == null)
			{
				TargetId = string.Empty;
			}
			else
			{
				TargetId = o.Key;
			}
		}

		#endregion

		private void NotifyPropertyChanged()
		{
			if (_suspendNotification)
				return;

			//tell any data binding
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("option"));

			//tell our parent
			_parent?.NotifyPropertyChanged("option");
		}

		#region IReportEmptiness Members

		public bool ShouldHoldUpDeletionOfParentObject => false;

		public bool ShouldCountAsFilledForPurposesOfConditionalDisplay => !IsEmpty;

		public bool ShouldBeRemovedFromParentDueToEmptiness => IsEmpty;

		public void RemoveEmptyStuff()
		{
			if (Value == string.Empty)
			{
				_suspendNotification = true;
				Value = null; // better for matching 'missing' for purposes of missing info task
				_suspendNotification = false;
			}
		}

		#endregion

		public int CompareTo(object obj)
		{
			if(obj == null)
			{
				return 1;
			}
			if(!(obj is OptionRef))
			{
				throw new ArgumentException($"Can not compare to anything but {nameof(OptionRef)}s.");
			}
			OptionRef other = (OptionRef) obj;
			int order = Key.CompareTo(other.Key);
			return order;
		}

		public new IPalasoDataObjectProperty Clone()
		{
			var clone = new OptionRef(Key);
			clone.EmbeddedXmlElements = new List<string>(EmbeddedXmlElements);
			clone.Annotation = Annotation?.Clone();
			return clone;
		}

		public override string ToString()
		{
			return Value;
		}

		public override bool Equals(object obj)
		{
			return Equals(obj as OptionRef);
		}

		public bool Equals(IPalasoDataObjectProperty other)
		{
			return Equals(other as OptionRef);
		}

		public bool Equals(OptionRef other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			if (Key != other.Key) return false;
			//we are doing a reference comparison here in case it's null
			if ((Annotation != null && !Annotation.Equals(other.Annotation)) || (other.Annotation != null && !other.Annotation.Equals(Annotation))) return false;
			if (!EmbeddedXmlElements.SequenceEqual(other.EmbeddedXmlElements)) return false;
			return true;
		}

		public override int GetHashCode()
		{
			// https://stackoverflow.com/a/263416/487503
			unchecked // Overflow is fine, just wrap
			{
				var hash = 3;
				hash *= 41 + Key.GetHashCode();
				hash *= 41 + Annotation.GetHashCode();
				hash *= 41 + EmbeddedXmlElements.GetHashCode();
				return hash;
			}
		}
	}
}