using System;
using System.Collections.Generic;
using System.Linq;

namespace SIL.WritingSystems
{
	public abstract class LocalWritingSystemRepositoryBase<T> : WritingSystemRepositoryBase<T>, ILocalWritingSystemRepository<T> where T : WritingSystemDefinition
	{
		private readonly Dictionary<string, DateTime> _writingSystemsToIgnore;
		public IWritingSystemRepository<T> GlobalWritingSystemRepository { get; }

		protected LocalWritingSystemRepositoryBase(IWritingSystemRepository<T> globalRepository)
		{
			GlobalWritingSystemRepository = globalRepository;
			_writingSystemsToIgnore = new Dictionary<string, DateTime>(StringComparer.OrdinalIgnoreCase);
		}

		protected IDictionary<string, DateTime> WritingSystemsToIgnore => _writingSystemsToIgnore;

		protected override void RemoveDefinition(T ws)
		{
			base.RemoveDefinition(ws);
			if (_writingSystemsToIgnore.ContainsKey(ws.Id))
				_writingSystemsToIgnore.Remove(ws.Id);
			if (_writingSystemsToIgnore.ContainsKey(ws.LanguageTag))
				_writingSystemsToIgnore.Remove(ws.LanguageTag);
		}

		protected virtual void LastChecked(string id, DateTime dateModified)
		{
			_writingSystemsToIgnore[id] = dateModified;
		}

		protected virtual void OnChangeNotifySharedStore(T ws)
		{
			DateTime lastDateModified;
			if (_writingSystemsToIgnore.TryGetValue(ws.LanguageTag, out lastDateModified) && ws.DateModified > lastDateModified)
				_writingSystemsToIgnore.Remove(ws.LanguageTag);

			if (GlobalWritingSystemRepository != null)
			{
				T globalWs;
				if (GlobalWritingSystemRepository.TryGet(ws.Id, out globalWs))
				{
					if (ws.DateModified > globalWs.DateModified)
					{
						T newWs = WritingSystemFactory.Create(ws, cloneId: true);
						try
						{
							GlobalWritingSystemRepository.Replace(ws.Id, newWs);
						}
						catch (Exception)
						{
							// Live with it if we can't update the global store. In a CS world we might
							// well not have permission.
						}
					}
				}
				else if(GlobalWritingSystemRepository.CanSet(ws)) // Don't try to set it in the global store if it claims we can't.
				{
					GlobalWritingSystemRepository.Set(WritingSystemFactory.Create(ws, cloneId: true));
				}
			}
		}

		private IEnumerable<T> WritingSystemsNewerInGlobalRepository()
		{
			foreach (T ws in GlobalWritingSystemRepository.AllWritingSystems)
			{
				if (WritingSystems.ContainsKey(ws.Id))
				{
					if ((!_writingSystemsToIgnore.TryGetValue(ws.Id, out var lastDateModified) || ws.DateModified > lastDateModified)
						&& (ws.DateModified > WritingSystems[ws.Id].DateModified))
					{
						yield return WritingSystemFactory.Create(ws);
					}
				}
			}
		}

		public virtual IEnumerable<T> CheckForNewerGlobalWritingSystems(IEnumerable<string> languageTags = null)
		{
			if (GlobalWritingSystemRepository != null)
			{
				var writingSystemsNewerInGlobalStore = languageTags != null
					? WritingSystemsNewerInGlobalRepository().Where(ws => languageTags.Contains(ws.LanguageTag))
					: WritingSystemsNewerInGlobalRepository();
				var results = new List<T>();
				foreach (T wsDef in writingSystemsNewerInGlobalStore)
				{
					LastChecked(wsDef.Id ?? wsDef.LanguageTag, wsDef.DateModified);
					results.Add(wsDef); // REVIEW Hasso 2013.12: add only if not equal?
				}

				return results;
			}
			return Enumerable.Empty<T>();
		}

		public override void Save()
		{
			GlobalWritingSystemRepository?.Save();
		}

		IEnumerable<WritingSystemDefinition> ILocalWritingSystemRepository.CheckForNewerGlobalWritingSystems(IEnumerable<string> languageTags)
		{
			return CheckForNewerGlobalWritingSystems(languageTags);
		}

		IWritingSystemRepository ILocalWritingSystemRepository.GlobalWritingSystemRepository => GlobalWritingSystemRepository;
	}
}
