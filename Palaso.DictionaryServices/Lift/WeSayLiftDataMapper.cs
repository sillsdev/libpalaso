using Palaso.DictionaryServices.Model;
using Palaso.Lift;
using Palaso.Lift.Options;
using Palaso.Progress;

namespace Palaso.DictionaryServices.Lift
{
	public class WeSayLiftDataMapper : LiftDataMapper<LexEntry>
	{
		public WeSayLiftDataMapper(string filePath, OptionsList semanticDomainsList, ProgressState progressState, ILiftReaderWriterProvider<LexEntry> readerWriter)
			: base(filePath, progressState, readerWriter)
		{
		}

	}
}