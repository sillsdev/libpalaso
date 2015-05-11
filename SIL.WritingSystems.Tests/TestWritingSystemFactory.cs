using SIL.ObjectModel;

namespace SIL.WritingSystems.Tests
{
	public class TestWritingSystemFactory : WritingSystemFactory
	{
		private readonly KeyedList<string, WritingSystemDefinition> _writingSystems; 

		public TestWritingSystemFactory()
		{
			_writingSystems = new KeyedList<string, WritingSystemDefinition>(ws => ws.LanguageTag);
		}

		public IKeyedCollection<string, WritingSystemDefinition> WritingSystems
		{
			get { return _writingSystems; }
		}

		public override WritingSystemDefinition Create(string ietfLanguageTag)
		{
			WritingSystemDefinition ws;
			if (_writingSystems.TryGet(ietfLanguageTag, out ws))
				return ws;

			return ConstructDefinition(ietfLanguageTag);
		}
	}
}
