namespace Palaso.Data
{
	public class RecordTokenToStringAdapter<T> where T:class, new()
	{
		private RecordToken<T> _recordToken = null;
		private string _fieldToShow = null;



		public RecordTokenToStringAdapter(string fieldToShow, RecordToken<T> recordToken)
		{
			_recordToken = recordToken;
			_fieldToShow = fieldToShow;
		}

		public RecordToken<T> AdaptedRecordToken
		{
			get { return _recordToken; }
		}

		public override string ToString()
		{
			return (string) AdaptedRecordToken[_fieldToShow];
		}
	}
}
