using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Linq;

namespace Palaso.WritingSystems
{
	public class WritingSystemChangeLog
	{
		private List<WritingSystemLogEvent> _logEvents;
		private IWritingSystemChangeLogDataMapper _dataMapper;

		public WritingSystemChangeLog(IWritingSystemChangeLogDataMapper dataMapper)
		{
			_logEvents = new List<WritingSystemLogEvent>();
			_dataMapper = dataMapper ?? new NullDataMapper();

			_dataMapper.Read(this);
			Version = "1";
		}

		public WritingSystemChangeLog() : this(null)
		{
		}

		public string Version { get; internal set; }

		public bool HasChangeFor(string id)
		{
			if (GetChangeFor(id) == id)
			{
				return false;
			}
			return true;
		}

		public string GetChangeFor(string id)
		{
			string result = id;
			foreach (WritingSystemLogChangeEvent change in _logEvents.Where(c => c.Type == "Change"))
			{
				if (result == change.From)
				{
					result = change.To;
				}
			}
			return result;
		}

		public void LogChange(string from, string to)
		{
			_WriteToLog(new WritingSystemLogChangeEvent(from, to));
		}

		public void LogAdd(string added)
		{
			_WriteToLog(new WritingSystemLogAddEvent(added));
		}

		public void LogDelete(string deleted)
		{
			_WriteToLog(new WritingSystemLogDeleteEvent(deleted));
		}

		private void _WriteToLog(WritingSystemLogEvent logEvent)
		{
			//_logEvents.Clear();
			_logEvents.Add(logEvent);
			if (File.Exists(_dataMapper.FilePath))
			{
				_dataMapper.AppendEvent(logEvent);
			}
			else
			{
				_dataMapper.Write(this);
			}

			//_dataMapper.Read(this);
		}

		// only intended for use by the datamapper
		internal List<WritingSystemLogEvent> Events
		{
			get {
				return _logEvents;
			}
		}

		// only intended for use by the datamapper
		internal void AddEvent(WritingSystemLogEvent logEvent)
		{
			_logEvents.Add(logEvent);
		}
	}

	public class NullDataMapper : IWritingSystemChangeLogDataMapper
	{
		public void Read(WritingSystemChangeLog log) {}
		public void Write(WritingSystemChangeLog log) {}
		public void AppendEvent(WritingSystemLogEvent logEvent) {}
		public string FilePath { get; set; }
	}

	public interface IWritingSystemChangeLogDataMapper
	{
		void Read(WritingSystemChangeLog log);
		void Write(WritingSystemChangeLog log);
		void AppendEvent(WritingSystemLogEvent logEvent);
		string FilePath { get; set; }
	}
}