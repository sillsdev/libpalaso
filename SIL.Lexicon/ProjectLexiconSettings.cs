using System.ComponentModel;
using SIL.ObjectModel;

namespace SIL.Lexicon
{
	public class ProjectLexiconSettings : ObservableObject, IChangeTracking
	{
		private bool _addWritingSystemsToSldr;
		private bool _addEnableProjectSharing;

		public bool AddWritingSystemsToSldr
		{
			get { return _addWritingSystemsToSldr; }
			set { Set(() => AddWritingSystemsToSldr, ref _addWritingSystemsToSldr, value); }
		}

		public bool AddEnableProjectSharing
		{
			get { return _addEnableProjectSharing; }
			set { Set(() => AddEnableProjectSharing, ref _addEnableProjectSharing, value); }
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
