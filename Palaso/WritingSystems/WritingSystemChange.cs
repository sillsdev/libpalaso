using Palaso.Extensions;

namespace Palaso.WritingSystems
{
	public class WritingSystemChange
	{
		public enum ChangeType
		{
			Change,
			Add,
			Delete
		}

		public WritingSystemChange(string from, string to, string producer, string producerVersion) :
			this(from, to, producer, producerVersion, ChangeType.Change) {}

		public WritingSystemChange(string from, string to, string producer, string producerVersion, string iso8601Datetime) :
			this(from, to, producer, producerVersion, ChangeType.Change, iso8601Datetime) { }

		public WritingSystemChange(string from, string to, string producer, string producerVersion, ChangeType type) :
			this(from, to, producer, producerVersion, type, System.DateTime.UtcNow.ToISO8601DateAndUTCTimeString()) { }


		public WritingSystemChange(string from, string to, string producer, string producerVersion, ChangeType type, string iso8601Datetime)
		{
			Type = type;
			From = from;
			To = to;
			Producer = producer;
			ProducerVersion = producerVersion;
			DateTime = iso8601Datetime;
		}

		public string From { get; set; }
		public string To { get; set; }
		public string Producer { get; set; }
		public string ProducerVersion { get; set; }
		public string DateTime { get; set; }
		public ChangeType Type { get; set; }
	}
}