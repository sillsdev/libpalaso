using System.Collections.Generic;
using Palaso.Lift;
using Palaso.Lift.Options;
using Palaso.Progress;

namespace WeSay.LexicalModel
{
	public class WeSayLiftReaderWriterProvider : ILiftReaderWriterProvider<LexEntry>
	{
		private readonly ProgressState _progressState;
		private readonly OptionsList _semanticDomainsList;
		private readonly IEnumerable<string> _idsOfSingleOptionFields;

		public WeSayLiftReaderWriterProvider(ProgressState progressState, OptionsList semanticDomainsList, IEnumerable<string> idsOfSingleOptionFields)
		{
			_progressState = progressState;
			this._idsOfSingleOptionFields = idsOfSingleOptionFields;
			_semanticDomainsList = semanticDomainsList;
		}

		public ILiftWriter<LexEntry> CreateWriter(string liftFilePath)
		{
			return new WeSayLiftWriter(liftFilePath);
		}

		public ILiftReader<LexEntry> CreateReader()
		{
			return new WeSayLiftReader(_progressState, _semanticDomainsList, _idsOfSingleOptionFields);
		}
	}
}