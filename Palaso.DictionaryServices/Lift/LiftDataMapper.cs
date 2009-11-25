using Palaso.DictionaryServices.Model;
using Palaso.Lift;
using Palaso.Lift.Options;
using Palaso.Progress;

namespace Palaso.DictionaryServices.Lift
{
	public class LiftDataMapper : LiftDataMapper<LexEntry>
	{
		public LiftDataMapper(string filePath, OptionsList semanticDomainsList, ProgressState progressState, ILiftReaderWriterProvider<LexEntry> readerWriter)
			: base(filePath, progressState, readerWriter)
		{
		}

	}
}