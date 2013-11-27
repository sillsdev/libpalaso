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
			var s = (string) AdaptedRecordToken[_fieldToShow];
			if(s==null)
				return string.Empty; //WinForms things that expect a string can give pretty useless errors when given null (e.g. list view)
			return s;
		}
	}
}
