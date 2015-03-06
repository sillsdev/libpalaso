using SIL.ObjectModel;

namespace SIL.WritingSystems.Tests
{
	public class TestWritingSystemFactory : IWritingSystemFactory<WritingSystemDefinition>
	{
		private readonly KeyedList<string, WritingSystemDefinition> _writingSystems; 

		public TestWritingSystemFactory()
		{
			_writingSystems = new KeyedList<string, WritingSystemDefinition>(ws => ws.IetfLanguageTag);
		}

		public IKeyedCollection<string, WritingSystemDefinition> WritingSystems
		{
			get { return _writingSystems; }
		}

		public WritingSystemDefinition Create()
		{
			return new WritingSystemDefinition();
		}

		public WritingSystemDefinition Create(string ietfLanguageTag)
		{
			WritingSystemDefinition ws;
			if (_writingSystems.TryGet(ietfLanguageTag, out ws))
				return ws;

			return new WritingSystemDefinition(ietfLanguageTag);
		}

		public WritingSystemDefinition Create(WritingSystemDefinition ws)
		{
			return new WritingSystemDefinition(ws);
		}
	}
}
