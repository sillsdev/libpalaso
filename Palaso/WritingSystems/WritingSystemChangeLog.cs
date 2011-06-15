using System.Collections.Generic;

namespace Palaso.WritingSystems
{
	public class WritingSystemChangeLog
	{
		private List<WritingSystemChange>_changes = new List<WritingSystemChange>();

		public WritingSystemChangeLog(string version)
		{
			Version = version;
		}

		public WritingSystemChangeLog() : this("1.0") {}

		public string Version { get; set; }

		public bool HasChangeFor(string id)
		{
			if (GetChangeFor(id) == null)
			{
				return false;
			}
			return true;
		}

		public string GetChangeFor(string id)
		{
			string result = id;
			foreach (var change in _changes)
			{
				if (result == change.From)
				{
					result = change.To;
				}
			}
			if (result == id)
			{
				return null;
			}
			return result;
		}

		public void Set(string from, string to, string producer, string producerVersion)
		{
			_changes.Add(new WritingSystemChange(from, to, producer, producerVersion));
		}

		public void Set(string from, string to, string producer, string producerVersion, string iso8601datetime)
		{
			_changes.Add(new WritingSystemChange(from, to, producer, producerVersion, iso8601datetime));
		}

		public List<WritingSystemChange> Items
		{
			get {
				return _changes;
			}
		}
	}
}