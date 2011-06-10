using Palaso.Extensions;

namespace Palaso.WritingSystems
{
	public class WritingSystemChange
	{
		public WritingSystemChange(string from, string to, string producer, string producerVersion, string iso8601Datetime)
		{
			From = from;
			To = to;
			Producer = producer;
			ProducerVersion = producerVersion;
			DateTime = iso8601Datetime;
		}

		public WritingSystemChange(string from, string to, string producer, string producerVersion) :
			this(from, to, producer, producerVersion, System.DateTime.UtcNow.ToISO8601DateAndUTCTimeString()) {}

		public string From { get; set; }
		public string To { get; set; }
		public string Producer { get; set; }
		public string ProducerVersion { get; set; }
		public string DateTime { get; set; }
	}
}