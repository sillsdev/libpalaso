using System.Collections.Generic;
using Palaso.DictionaryServices.Model;
using Palaso.Lift;
using Palaso.Lift.Options;
using Palaso.Progress;

namespace Palaso.DictionaryServices.Lift
{
	public class LiftReaderWriterProvider : ILiftReaderWriterProvider<LexEntry>
	{
		private readonly ProgressState _progressState;
		private readonly OptionsList _semanticDomainsList;
		private readonly IEnumerable<string> _idsOfSingleOptionFields;

		public LiftReaderWriterProvider(ProgressState progressState, OptionsList semanticDomainsList, IEnumerable<string> idsOfSingleOptionFields)
		{
			_progressState = progressState;
			_idsOfSingleOptionFields = idsOfSingleOptionFields;
			_semanticDomainsList = semanticDomainsList;
		}

		public ILiftWriter<LexEntry> CreateWriter(string liftFilePath)
		{
			return new LiftWriter(liftFilePath);
		}

		public ILiftReader<LexEntry> CreateReader()
		{
			return new LiftReader(_progressState, _semanticDomainsList, _idsOfSingleOptionFields);
		}
	}
}