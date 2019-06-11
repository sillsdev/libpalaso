using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SIL.WritingSystems
{
	/// <summary>
	/// This class is used by the LdmlInFolderWritingSystemRepository to log any changes that an application makes to the repository.
	/// The idea is that consumers can query this class to learn of any changes made to contained writing systems by another program
	/// and update its other files accordingly.
	/// </summary>
	public class WritingSystemChangeLog : IAuditTrail
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
			//this is the case if there is a cycle id => id2 => id
			var changedId = GetChangeFor(id);
			if (changedId == id)
			{
				return false;
			}
			if (changedId == null)
			{
				if (_logEvents.Count(c => (c.Type == "Change" || c.Type == "Merge") && ((WritingSystemLogChangeEvent)c).From.Equals(id)) > 1)
				{
					return true;
				}
				return false;
			}
			return true;
		}

		public string GetChangeFor(string id)
		{
			if (_logEvents.Count(c => (c.Type == "Change" || c.Type == "Merge") && ((WritingSystemLogChangeEvent)c).From.Equals(id)) != 1)
			{
				return null;
			}
			string result = id;
			foreach (WritingSystemLogChangeEvent change in _logEvents.Where(c => c.Type == "Change" || c.Type == "Merge"))
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
			if (from != to)
			{
				WriteToLog(new WritingSystemLogChangeEvent(from, to));
			}
		}

		public void LogAdd(string added)
		{
			WriteToLog(new WritingSystemLogAddEvent(added));
		}

		public void LogConflate(string from, string to)
		{
			WriteToLog(new WritingSystemLogConflateEvent(from, to));
		}

		public void LogDelete(string deleted)
		{
			WriteToLog(new WritingSystemLogDeleteEvent(deleted));
		}

		private void WriteToLog(WritingSystemLogEvent logEvent)
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

	public interface IAuditTrail
	{
		void LogChange(string from, string to);
		void LogAdd(string id);
		void LogDelete(string id);
		void LogConflate(string oldId, string newId);
		string GetChangeFor(string id);
		bool HasChangeFor(string id);
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