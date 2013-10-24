using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SIL.Archiving.IMDI
{
	class IMDIArchivingDlgViewModel : ArchivingDlgViewModel
	{
		private string _corpusName;
		private IMDIData _imdiData;

		public IMDIArchivingDlgViewModel(string appName, string corpusName, string title, string id) : base(appName, title, id)
		{
			_corpusName = corpusName;
		}

		protected override bool DoArchiveSpecificInitialization()
		{
			_imdiData = new IMDIData
			{
				Title = _title,
				Name = _corpusName
			};

			return true;
		}

		public override bool LaunchArchivingProgram()
		{
			throw new NotImplementedException();
		}

		public override bool CreatePackage()
		{
			throw new NotImplementedException();
		}
	}
}
