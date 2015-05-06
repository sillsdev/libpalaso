using System.ComponentModel;
using SIL.ObjectModel;

namespace SIL.LexiconUtils
{
	public class ProjectLexiconSettings : ObservableObject, IChangeTracking
	{
		private bool _addWritingSystemsToSldr;

		public bool AddWritingSystemsToSldr
		{
			get { return _addWritingSystemsToSldr; }
			set { Set(() => AddWritingSystemsToSldr, ref _addWritingSystemsToSldr, value); }
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
