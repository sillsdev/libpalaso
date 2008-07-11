using System;
using System.Collections.Generic;
using System.Text;

using Palaso.WritingSystems;

namespace Palaso.UI.WindowsForms.WritingSystems
{
	public class PickerPM
	{
		#region Properties
		private IWritingSystemStore _writingSystemStore;
		public IWritingSystemStore WritingSystemStore
		{
			get { return _writingSystemStore; }
			set { _writingSystemStore = value; }
		}

		public IEnumerable<WritingSystemDefinition> WritingSystemsDefinitions
		{
			get
			{
				return _writingSystemStore.WritingSystemDefinitions;
			}
		}

		#endregion

	}
}
