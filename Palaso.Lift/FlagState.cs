using System.ComponentModel;
using Palaso.Lift.UiBindings;

namespace Palaso.Lift
{
	/// <summary>
	/// Holds a boolean value for, for example, a checkbox
	/// </summary>
	public class FlagState: IParentable, IValueHolder<bool>, IReportEmptiness
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
			set { _parent = value; }
		}

		#endregion

		private void NotifyPropertyChanged()
		{
			//tell any data binding
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs("checkBox")); //todo
			}

			//tell our parent

			if (_parent != null)
			{
				_parent.NotifyPropertyChanged("checkBox");
			}
		}

		public bool Value
		{
			get { return _isChecked; }
			set
			{
				_isChecked = value;
				// this.Guid = value.Guid;
				NotifyPropertyChanged();
			}
		}

		#region IReportEmptiness Members

		public bool ShouldHoldUpDeletionOfParentObject
		{
			get { return false; }
		}

		public bool ShouldCountAsFilledForPurposesOfConditionalDisplay
		{
			get { return _isChecked; } //review: is that right? or always true?
		}

		public bool ShouldBeRemovedFromParentDueToEmptiness
		{
			get { return !_isChecked; }
		}

		public void RemoveEmptyStuff() {}

		#endregion
	}
}