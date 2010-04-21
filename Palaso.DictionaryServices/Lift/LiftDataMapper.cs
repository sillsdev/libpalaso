using System.Collections.Generic;
using Palaso.DictionaryServices.Lift;
using Palaso.DictionaryServices.Model;
using Palaso.Lift;
using Palaso.Lift.Options;
using Palaso.Progress;

namespace Palaso.DictionaryServices.Lift
{
	public class LiftDataMapper : LiftDataMapper<LexEntry>
	{
		private class LiftReaderWriterProvider : ILiftReaderWriterProvider<LexEntry>
		{
			private readonly ProgressState _progressState;
			private readonly OptionsList _semanticDomainsList;
			private readonly IEnumerable<string> _idsOfSingleOptionFields;

			public LiftReaderWriterProvider(ProgressState progressState, OptionsList semanticDomainsList, IEnumerable<string> idsOfSingleOptionFields)
			{
				_progressState = progressState;
				_semanticDomainsList = semanticDomainsList;
				_idsOfSingleOptionFields = idsOfSingleOptionFields;
			}

			public ILiftWriter<LexEntry> CreateWriter(string liftFilePath)
			{
				return new LiftWriter(liftFilePath, LiftWriter.ByteOrderStyle.BOM);
			}

			public ILiftReader<LexEntry> CreateReader()
			{
				return new LiftReader(_progressState, _semanticDomainsList, _idsOfSingleOptionFields);
			}
		}

		public LiftDataMapper(string filePath, OptionsList semanticDomainsList, IEnumerable<string> idsOfSingleOptionFields, ProgressState progressState)
			: base(filePath, progressState, new LiftReaderWriterProvider(progressState, semanticDomainsList, idsOfSingleOptionFields))
		{
		}

		/// <summary>
		/// unit tests only
		/// </summary>
		/// <param name="filePath"></param>
		public LiftDataMapper(string filePath)
			: this(filePath, new OptionsList(), new string[] { }, new ProgressState())
		{
		}

	}
}