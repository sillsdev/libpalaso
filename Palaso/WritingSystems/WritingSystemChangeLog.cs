using System;
using System.Collections.Generic;
using System.Reflection;

namespace Palaso.WritingSystems
{
	public class WritingSystemChangeLog
	{
		private List<WritingSystemChange>_changes = new List<WritingSystemChange>();
		private string _producer;
		private string _producerVersion;
		public WritingSystemChangeLog(string version)
		{
			Version = version;
			_producer = _getProducer();
			_producerVersion = _getProducerVersion();
		}

		private string _getProducer()
		{
			var assembly = Assembly.GetEntryAssembly();
			if (assembly != null)
			{
				return assembly.FullName;
			}
			return "???";
		}

		private string _getProducerVersion()
		{
			var assembly = Assembly.GetEntryAssembly();
			if (assembly != null)
			{
				var ver = assembly.GetName().Version;
				return string.Format("Version {0}.{1}.{2}", ver.Major, ver.Minor, ver.Revision);
			}
			return "???";
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

		public void Set(string from, string to)
		{
			_changes.Add(new WritingSystemChange(from, to, _producer, _producerVersion));
		}

		public void Set(string from, string to, WritingSystemChange.ChangeType type, string iso8601datetime)
		{
			_changes.Add(new WritingSystemChange(from, to, _producer, _producerVersion, type, iso8601datetime));
		}

		public void Set(string from, string to, string iso8601datetime)
		{
			_changes.Add(new WritingSystemChange(from, to, _producer, _producerVersion, iso8601datetime));
		}

		public void Set(string from, string to, WritingSystemChange.ChangeType type)
		{
			_changes.Add(new WritingSystemChange(from, to, _producer, _producerVersion, type));
		}

		public List<WritingSystemChange> Items
		{
			get {
				return _changes;
			}
		}
	}
}