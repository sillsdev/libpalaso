using System.ComponentModel;
using SIL.ObjectModel;

namespace SIL.LexiconUtils
{
	public class LexiconProjectSettings : ObservableObject, IChangeTracking
	{
		private bool _allowAddWritingSystemsToSldr;

		public bool AllowAddWritingSystemsToSldr
		{
			get { return _allowAddWritingSystemsToSldr; }
			set { Set(() => AllowAddWritingSystemsToSldr, ref _allowAddWritingSystemsToSldr, value); }
		}

		protected override void OnPropertyChanged(PropertyChangedEventArgs e)
		{
			base.OnPropertyChanged(e);
			IsChanged = true;
		}

		public void AcceptChanges()
		{
			IsChanged = false;
		}

		public bool IsChanged { get; private set; }
	}
}
