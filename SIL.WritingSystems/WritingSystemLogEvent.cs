using System;
using System.Reflection;
using SIL.Reflection;

namespace SIL.WritingSystems
{
	public class WritingSystemLogEvent
	{
		public WritingSystemLogEvent(string type) :
			this(type, DateTime.UtcNow) { }

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
				return assembly.GetName().Name;
			}
			return "???";
		}

		private string _getProducerVersion()
		{
			return ReflectionHelper.LongVersionNumberString;
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

	public class WritingSystemLogConflateEvent: WritingSystemLogChangeEvent
	{
		public WritingSystemLogConflateEvent(string from, string to) : base(from, to)
		{
			Type = "Merge";
		}
	}

	public class WritingSystemLogDeleteEvent : WritingSystemLogEvent
	{
		public string ID { get; set; }

		public WritingSystemLogDeleteEvent(string id)
			: base("Delete")
		{
			ID = id;
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
		public string ID { get; set; }

		public WritingSystemLogAddEvent(string id)
			: base("Add")
		{
			ID = id;
		}


	}
}