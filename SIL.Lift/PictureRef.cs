using System.ComponentModel;
using SIL.UiBindings;

namespace SIL.Lift
{
	public class PictureRef: IPalasoDataObjectProperty, IValueHolder<string>, IReportEmptiness
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

		public IPalasoDataObjectProperty Clone()
		{
			var clone = new PictureRef();
			clone._fileName = _fileName;
			clone._caption = _caption == null ? null:(MultiText) _caption.Clone();
			return clone;
		}

		public override bool Equals(object other)
		{
			return Equals((PictureRef) other);
		}

		public bool Equals(IPalasoDataObjectProperty other)
		{
			return Equals((PictureRef)other);
		}

		public bool Equals(PictureRef other)
		{
			if (other == null) return false;
			if ((_fileName != null && !_fileName.Equals(other._fileName)) || (other._fileName != null && !other._fileName.Equals(_fileName))) return false;
			if ((_caption != null && !_caption.Equals(other._caption)) || (other._caption != null && !other._caption.Equals(_caption))) return false;
			return true;
		}
	}
}