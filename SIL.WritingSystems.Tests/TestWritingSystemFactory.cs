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

		public override bool Create(string ietfLanguageTag, out WritingSystemDefinition ws)
		{
			if (_writingSystems.TryGet(ietfLanguageTag, out ws))
				return true;

			ws = ConstructDefinition(ietfLanguageTag);
			return true;
		}
	}
}
