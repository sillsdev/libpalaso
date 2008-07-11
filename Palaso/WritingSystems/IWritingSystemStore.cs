using System;
using System.Collections.Generic;
using System.Text;

namespace Palaso.WritingSystems
{
	public interface IWritingSystemStore
	{
		void Set(WritingSystemDefinition ws);
		WritingSystemDefinition Get(string identifier);
		bool Exists(string identifier);
		int Count { get; }

		WritingSystemDefinition AddNew();
		void Remove(string identifier);
		IEnumerable<WritingSystemDefinition> WritingSystemDefinitions { get; }
		WritingSystemDefinition MakeDuplicate(WritingSystemDefinition definition);

		void LastChecked(string identifier, DateTime dateModified);

		void Save();

		// TODO: Maybe this should be IEnumerable<string> .... which returns the identifiers.
		IEnumerable<WritingSystemDefinition> WritingSystemsNewerIn(IEnumerable<WritingSystemDefinition> rhs);
	}
}
