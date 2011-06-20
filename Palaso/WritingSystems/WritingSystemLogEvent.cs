using System;
using System.Reflection;
using Palaso.Extensions;

namespace Palaso.WritingSystems
{
	public class WritingSystemLogEvent
	{
		public WritingSystemLogEvent(string type) :
			this(type, System.DateTime.UtcNow) { }

		public WritingSystemLogEvent(string type, DateTime iso8601Datetime)
		{
			Type = type;
			Producer = _getProducer();
			ProducerVersion = _getProducerVersion();
			DateTime = iso8601Datetime;
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

		public string Producer { get; internal set; }
		public string ProducerVersion { get; internal set; }
		public DateTime DateTime { get; internal set; }
		public string Type { get; protected set; }
	}

	public class WritingSystemLogChangeEvent : WritingSystemLogEvent
	{
		public string To { get; set; }
		public string From { get; set; }

		public WritingSystemLogChangeEvent(string from, string to) : base("Change")
		{
			From = from;
			To = to;
		}

		/*
		public WritingSystemLogChangeEvent(string from, string to, string producer, string producerVersion, DateTime dateTime) : this(from, to)
		{
			Producer = producer;
			ProducerVersion = producerVersion;
			DateTime = dateTime;
		}*/
	}

	public class WritingSystemLogDeleteEvent : WritingSystemLogEvent
	{
		public string Id { get; set; }

		public WritingSystemLogDeleteEvent(string id)
			: base("Delete")
		{
			Id = id;
		}

		/*
		public WritingSystemLogDeleteEvent(string id, string producer, string producerVersion, DateTime dateTime) : this(id)
		{
			Producer = producer;
			ProducerVersion = producerVersion;
			DateTime = dateTime;
		}*/
	}

	public class WritingSystemLogAddEvent : WritingSystemLogEvent
	{
		public string Id { get; set; }

		public WritingSystemLogAddEvent(string id)
			: base("Add")
		{
			Id = id;
		}


	}
}