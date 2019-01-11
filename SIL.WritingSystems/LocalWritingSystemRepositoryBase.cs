using System;
using System.Collections.Generic;
using System.Linq;

namespace SIL.WritingSystems
{
	public abstract class LocalWritingSystemRepositoryBase<T> : WritingSystemRepositoryBase<T>, ILocalWritingSystemRepository<T> where T : WritingSystemDefinition
	{
		private readonly Dictionary<string, DateTime> _writingSystemsToIgnore;
		private readonly IWritingSystemRepository<T> _globalRepository;

		protected LocalWritingSystemRepositoryBase(IWritingSystemRepository<T> globalRepository)
		{
			_globalRepository = globalRepository;
			_writingSystemsToIgnore = new Dictionary<string, DateTime>(StringComparer.OrdinalIgnoreCase);
		}

		protected IDictionary<string, DateTime> WritingSystemsToIgnore
		{
			get { return _writingSystemsToIgnore; }
		}

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

			if (_globalRepository != null)
			{
				T globalWs;
				if (_globalRepository.TryGet(ws.LanguageTag, out globalWs))
				{
					if (ws.DateModified > globalWs.DateModified)
					{
						T newWs = WritingSystemFactory.Create(ws, cloneId: true);
						try
						{
							_globalRepository.Replace(ws.LanguageTag, newWs);
						}
						catch (Exception)
						{
							// Live with it if we can't update the global store. In a CS world we might
							// well not have permission.
						}
					}
				}
				else
				{
					_globalRepository.Set(WritingSystemFactory.Create(ws, cloneId: true));
				}
			}
		}

		private IEnumerable<T> WritingSystemsNewerInGlobalRepository()
		{
			foreach (T ws in _globalRepository.AllWritingSystems)
			{
				if (WritingSystems.ContainsKey(ws.LanguageTag))
				{
					DateTime lastDateModified;
					if ((!_writingSystemsToIgnore.TryGetValue(ws.LanguageTag, out lastDateModified) || ws.DateModified > lastDateModified)
						&& (ws.DateModified > WritingSystems[ws.LanguageTag].DateModified))
					{
						yield return WritingSystemFactory.Create(ws);
					}
				}
			}
		}

		public virtual IEnumerable<T> CheckForNewerGlobalWritingSystems()
		{
			if (_globalRepository != null)
			{
				var results = new List<T>();
				foreach (T wsDef in WritingSystemsNewerInGlobalRepository())
				{
					LastChecked(wsDef.LanguageTag, wsDef.DateModified);
					results.Add(wsDef); // REVIEW Hasso 2013.12: add only if not equal?
				}
				return results;
			}
			return Enumerable.Empty<T>();
		}

		public IWritingSystemRepository<T> GlobalWritingSystemRepository
		{
			get { return _globalRepository; }
		}

		public override void Save()
		{
			if (_globalRepository != null)
				_globalRepository.Save();
		}

		IEnumerable<WritingSystemDefinition> ILocalWritingSystemRepository.CheckForNewerGlobalWritingSystems()
		{
			return CheckForNewerGlobalWritingSystems();
		}

		IWritingSystemRepository ILocalWritingSystemRepository.GlobalWritingSystemRepository
		{
			get { return GlobalWritingSystemRepository; }
		}
	}
}
