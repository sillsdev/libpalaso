using System.ComponentModel;
using Palaso.UiBindings;

namespace Palaso.Lift
{
	public class PictureRef: IParentable, IValueHolder<string>, IReportEmptiness
	{
		private string _fileName;
		private MultiText _caption;

		/// <summary>
		/// This "backreference" is used to notify the parent of changes.
		/// IParentable gives access to this during explicit construction.
		/// </summary>
		private PalasoDataObject _parent;

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
				PropertyChanged(this, new PropertyChangedEventArgs("picture"));
			}

			//tell our parent

			if (_parent != null)
			{
				_parent.NotifyPropertyChanged("picture");
			}
		}

		public string Value
		{
			get { return _fileName; }
			set
			{
				_fileName = value;
				NotifyPropertyChanged();
			}
		}

		public MultiText Caption
		{
			get { return _caption; }
			set { _caption = value; }
		}

		#region IReportEmptiness Members

		public bool ShouldHoldUpDeletionOfParentObject
		{
			get { return false; }
		}

		public bool ShouldCountAsFilledForPurposesOfConditionalDisplay
		{
			get { return !string.IsNullOrEmpty(_fileName); }
		}

		public bool ShouldBeRemovedFromParentDueToEmptiness
		{
			get { return string.IsNullOrEmpty(_fileName); }
		}

		public void RemoveEmptyStuff() {}

		#endregion
	}
}