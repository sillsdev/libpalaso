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
			set => _parent = value;
		}

		#endregion

		private void NotifyPropertyChanged()
		{
			//tell any data binding
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("picture"));

			//tell our parent

			_parent?.NotifyPropertyChanged("picture");
		}

		public string Value
		{
			get => _fileName;
			set
			{
				_fileName = value;
				NotifyPropertyChanged();
			}
		}

		public MultiText Caption
		{
			get => _caption;
			set => _caption = value;
		}

		#region IReportEmptiness Members

		public bool ShouldHoldUpDeletionOfParentObject => false;

		public bool ShouldCountAsFilledForPurposesOfConditionalDisplay => !string.IsNullOrEmpty(_fileName);

		public bool ShouldBeRemovedFromParentDueToEmptiness => string.IsNullOrEmpty(_fileName);

		public void RemoveEmptyStuff() {}

		#endregion

		public IPalasoDataObjectProperty Clone()
		{
			var clone = new PictureRef();
			clone._fileName = _fileName;
			clone._caption = (MultiText) _caption?.Clone();
			return clone;
		}

		public override bool Equals(object other)
		{
			return Equals(other as PictureRef);
		}

		public bool Equals(IPalasoDataObjectProperty other)
		{
			return Equals(other as PictureRef);
		}

		public bool Equals(PictureRef other)
		{
			if (other == null) return false;
			if ((_fileName != null && !_fileName.Equals(other._fileName)) || (other._fileName != null && !other._fileName.Equals(_fileName))) return false;
			if ((_caption != null && !_caption.Equals(other._caption)) || (other._caption != null && !other._caption.Equals(_caption))) return false;
			return true;
		}

		public override int GetHashCode()
		{
			// https://stackoverflow.com/a/263416/487503
			unchecked // Overflow is fine, just wrap
			{
				var hash = 47;
				hash *= 61 + _caption?.GetHashCode() ?? 0;
				hash *= 61 + _fileName?.GetHashCode() ?? 0;
				return hash;
			}
		}
	}
}