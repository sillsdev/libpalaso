using System.ComponentModel;
using SIL.UiBindings;

namespace SIL.Lift
{
	/// <summary>
	/// Holds a boolean value for, for example, a checkbox
	/// </summary>
	public class FlagState: IPalasoDataObjectProperty, IValueHolder<bool>, IReportEmptiness
	{
		/// <summary>
		/// This "backreference" is used to notify the parent of changes.
		/// IParentable gives access to this during explicit construction.
		/// </summary>
		private PalasoDataObject _parent;

		private bool _isChecked;

		/// <summary>
		/// For INotifyPropertyChanged
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged;

		#region IParentable Members

		public PalasoDataObject Parent
		{
			set => _parent = value;
		}

		#endregion

		private void NotifyPropertyChanged()
		{
			//tell any data binding
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("checkBox")); //todo

			//tell our parent

			_parent?.NotifyPropertyChanged("checkBox");
		}

		public bool Value
		{
			get => _isChecked;
			set
			{
				_isChecked = value;
				// this.Guid = value.Guid;
				NotifyPropertyChanged();
			}
		}

		#region IReportEmptiness Members

		public bool ShouldHoldUpDeletionOfParentObject => false;

		public bool ShouldCountAsFilledForPurposesOfConditionalDisplay => _isChecked; //review: is that right? or always true?

		public bool ShouldBeRemovedFromParentDueToEmptiness => !_isChecked;

		public void RemoveEmptyStuff() {}

		#endregion

		public virtual IPalasoDataObjectProperty Clone()
		{
			var clone = new FlagState { _isChecked = _isChecked };
			return clone;
		}

		public override bool Equals(object other)
		{
			return Equals(other as FlagState);
		}

		public bool Equals(IPalasoDataObjectProperty other)
		{
			return Equals(other as FlagState);
		}

		public bool Equals(FlagState other)
		{
			return other != null && _isChecked.Equals(other._isChecked);
		}

		public override int GetHashCode()
		{
			// https://stackoverflow.com/a/263416/487503
			unchecked // Overflow is fine, just wrap
			{
				var hash = 71;
				hash *= 41 + _isChecked.GetHashCode();
				return hash;
			}
		}
	}
}