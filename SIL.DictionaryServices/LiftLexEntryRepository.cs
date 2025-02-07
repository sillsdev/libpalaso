using System;
using System.Collections.Generic;
using System.Diagnostics;
using SIL.Code;
using SIL.Data;
using SIL.DictionaryServices.Lift;
using SIL.DictionaryServices.Model;
using SIL.Lift;
using SIL.Lift.Options;
using SIL.Progress;
using SIL.Text;
using SIL.UiBindings;
using SIL.WritingSystems;

namespace SIL.DictionaryServices
{
	public class LiftLexEntryRepository : IDataMapper<LexEntry>, ICountGiver
	{
		public class EntryEventArgs : EventArgs
		{
			public readonly string label;

			public EntryEventArgs(LexEntry entry)
			{
				label = entry.LexicalForm.GetFirstAlternative();
			}
			public EntryEventArgs(RepositoryId repositoryId)
			{
				label = "?";//enhance: how do we get a decent label?
			}
		}
		public event EventHandler<EntryEventArgs> AfterEntryModified;
		public event EventHandler<EntryEventArgs> AfterEntryDeleted;
		//public event EventHandler<EntryEventArgs> AfterEntryAdded;  I (JH) don't know how to tell the difference between new and modified


		readonly ResultSetCacheManager<LexEntry> _caches = new ResultSetCacheManager<LexEntry>();

		//hack to prevent sending nested Save calls, which was causing a bug when
		//the exporter caused an item to get a new id, which led eventually to the list thinking it was modified, etc...
		private bool _currentlySaving;

		private readonly IDataMapper<LexEntry> _decoratedDataMapper;

#if DEBUG
		private readonly StackTrace _constructionStackTrace;
#endif

		// review: this constructor is only used for tests, and causes grief with
		// the dispose pattern.  Remove and refactor tests to use the other constructor
		// in a using style. cp
		public LiftLexEntryRepository(string path)
		{
			_disposed = true;
#if DEBUG
			_constructionStackTrace = new StackTrace();
#endif
			_decoratedDataMapper = new LiftDataMapper(path, null, new string[] {}, new ProgressState());
			_disposed = false;
		}

		// review: may want to change LiftDataMapper to IDataMapper<LexEntry> but I (cp) am leaving
		// this for the moment as would also need to change the container builder.Register in WeSayWordsProject
		public LiftLexEntryRepository(LiftDataMapper decoratedDataMapper)
		{
			Guard.AgainstNull(decoratedDataMapper, "decoratedDataMapper");
#if DEBUG
			_constructionStackTrace = new StackTrace();
#endif
			_decoratedDataMapper = decoratedDataMapper;
			_disposed = false;
		}


		public DateTime LastModified
		{
			get { return _decoratedDataMapper.LastModified; }
		}

		public LexEntry CreateItem()
		{
			LexEntry item = _decoratedDataMapper.CreateItem();
			_caches.AddItemToCaches(item);
			return item;
		}

		public RepositoryId[] GetAllItems()
		{
			return _decoratedDataMapper.GetAllItems();
		}

		public int CountAllItems()
		{
			return _decoratedDataMapper.CountAllItems();
		}

		public RepositoryId GetId(LexEntry item)
		{
			return _decoratedDataMapper.GetId(item);
		}

		public LexEntry GetItem(RepositoryId id)
		{
			LexEntry item = _decoratedDataMapper.GetItem(id);
			return item;
		}

		public void SaveItems(IEnumerable<LexEntry> items)
		{
			if (items == null)
			{
				throw new ArgumentNullException("items");
			}
			var dirtyItems = new List<LexEntry>();
			foreach (LexEntry item in items)
			{
				if (item.IsDirty)
				{
					dirtyItems.Add(item);
					_caches.UpdateItemInCaches(item);
				}
			}
			_decoratedDataMapper.SaveItems(dirtyItems);
			foreach (LexEntry item in dirtyItems)
			{
				item.Clean();
			}
		}

		public ResultSet<LexEntry> GetItemsMatching(IQuery<LexEntry> query)
		{
			return _decoratedDataMapper.GetItemsMatching(query);
		}

		public void SaveItem(LexEntry item)
		{
			if (_currentlySaving) //sometimes the process of saving leads modification which leads to a new save
			{
				return;
			}
			_currentlySaving = true;
			try
			{

				if (item == null)
				{
					throw new ArgumentNullException("item");
				}
				if (item.IsDirty)
				{
					_decoratedDataMapper.SaveItem(item);
					_caches.UpdateItemInCaches(item);
					item.Clean();

					//review: I (JH) don't know how to tell the difference between new and modified
					if (AfterEntryModified != null)
					{
						AfterEntryModified(this, new EntryEventArgs(item));
					}
				}
			}
			finally
			{
				_currentlySaving = false;
			}
		}

		public bool CanQuery
		{
			get { return _decoratedDataMapper.CanQuery; }
		}

		public bool CanPersist
		{
			get { return _decoratedDataMapper.CanPersist; }
		}

		public void DeleteItem(LexEntry item)
		{
			if (item == null)
				throw new ArgumentNullException("item");

			var args = new EntryEventArgs(item);

			_caches.DeleteItemFromCaches(item);
			_decoratedDataMapper.DeleteItem(item);

			if(AfterEntryDeleted !=null)
			{
				AfterEntryDeleted(this, args);
			}
		}

		public void DeleteItem(RepositoryId repositoryId)
		{
			var args = new EntryEventArgs(repositoryId);

			_caches.DeleteItemFromCaches(repositoryId);
			_decoratedDataMapper.DeleteItem(repositoryId);

			if(AfterEntryDeleted !=null)
			{
				AfterEntryDeleted(this, args);
			}
		}

		public void DeleteAllItems()
		{
			_decoratedDataMapper.DeleteAllItems();
			_caches.DeleteAllItemsFromCaches();
		}

		public void NotifyThatLexEntryHasBeenUpdated(LexEntry updatedLexEntry)
		{
			if(updatedLexEntry == null)
			{
				throw new ArgumentNullException("updatedLexEntry");
			}
			//This call checks that the Entry is in the repository
			GetId(updatedLexEntry);
			_caches.UpdateItemInCaches(updatedLexEntry);
		}

		public int GetHomographNumber(LexEntry entry, WritingSystemDefinition headwordWritingSystem)
		{
			if (entry == null)
			{
				throw new ArgumentNullException("entry");
			}
			if (headwordWritingSystem == null)
			{
				throw new ArgumentNullException("headwordWritingSystem");
			}
			ResultSet<LexEntry> resultSet = GetAllEntriesSortedByHeadword(headwordWritingSystem);
			RecordToken<LexEntry> first = resultSet.FindFirst(entry);
			if (first == null)
			{
				throw new ArgumentOutOfRangeException("entry", entry, "Entry not in repository");
			}
			if ((bool) first["HasHomograph"])
			{
				return (int) first["HomographNumber"];
			}
			return 0;
		}

		/// <summary>
		/// Gets a ResultSet containing all entries sorted by citation if one exists and otherwise
		/// by lexical form.
		/// Use "Form" to access the headword in a RecordToken.
		/// </summary>
		/// <param name="writingSystemDefinition"></param>
		/// <returns></returns>
		public ResultSet<LexEntry> GetAllEntriesSortedByHeadword(WritingSystemDefinition writingSystemDefinition)
		{
			if (writingSystemDefinition == null)
			{
				throw new ArgumentNullException("writingSystemDefinition");
			}

			string cacheName = String.Format("sortedByHeadWord_{0}", writingSystemDefinition.LanguageTag);
			if (_caches[cacheName] == null)
			{
				var headWordQuery = new DelegateQuery<LexEntry>(
					delegate(LexEntry entryToQuery)
						{
							IDictionary<string, object> tokenFieldsAndValues = new Dictionary<string, object>();
							string headWord = entryToQuery.VirtualHeadWord[writingSystemDefinition.LanguageTag];
							if (String.IsNullOrEmpty(headWord))
							{
								headWord = null;
							}
							tokenFieldsAndValues.Add("Form",headWord);
							return new[] { tokenFieldsAndValues };
						});

				ResultSet<LexEntry> itemsMatching = _decoratedDataMapper.GetItemsMatching(headWordQuery);
				var sortOrder = new SortDefinition[4];
				sortOrder[0] = new SortDefinition("Form", writingSystemDefinition.DefaultCollation.Collator);
				sortOrder[1] = new SortDefinition("OrderForRoundTripping", Comparer<int>.Default);
				sortOrder[2] = new SortDefinition("OrderInFile", Comparer<int>.Default);
				sortOrder[3] = new SortDefinition("CreationTime", Comparer<DateTime>.Default);

				_caches.Add(cacheName, new ResultSetCache<LexEntry>(this, sortOrder, itemsMatching, headWordQuery));
				// _caches.Add(headWordQuery, /* itemsMatching */ results); // review cp Refactor caches to this signature.
			}
			ResultSet<LexEntry> resultsFromCache = _caches[cacheName].GetResultSet();

			string previousHeadWord = null;
			int homographNumber = 1;
			RecordToken<LexEntry> previousToken = null;
			foreach (RecordToken<LexEntry> token in resultsFromCache)
			{
				// A null Form indicates there is no HeadWord in this writing system.
				// However, we need to ensure that we return all entries, so the AtLeastOne in the query
				// above ensures that we keep it in the result set with a null Form and null WritingSystemId.
				var currentHeadWord = (string) token["Form"];
				if (string.IsNullOrEmpty(currentHeadWord))
				{
					token["HasHomograph"] = false;
					token["HomographNumber"] = 0;
					continue;
				}
				if (currentHeadWord == previousHeadWord)
				{
					homographNumber++;
				}
				else
				{
					previousHeadWord = currentHeadWord;
					homographNumber = 1;
				}
				// only used to get our sort correct --This comment seems nonsensical --TA 2008-08-14!!!
				token["HomographNumber"] = homographNumber;
				switch (homographNumber)
				{
					case 1:
						token["HasHomograph"] = false;
						break;
					case 2:
						Debug.Assert(previousToken != null);
						previousToken["HasHomograph"] = true;
						token["HasHomograph"] = true;
						break;
					default:
						token["HasHomograph"] = true;
						break;
				}
				previousToken = token;
			}

			return resultsFromCache;
		}

		/// <summary>
		/// Gets a ResultSet containing all entries sorted by lexical form for a given writing system.
		/// If a lexical form for a given writing system does not exist we substitute one from another writing system.
		/// Use "Form" to access the lexical form in a RecordToken.
		/// </summary>
		/// <param name="writingSystemDefinition"></param>
		/// <returns></returns>
		public ResultSet<LexEntry> GetAllEntriesSortedByLexicalFormOrAlternative(WritingSystemDefinition writingSystemDefinition)
		{
			if (writingSystemDefinition == null)
			{
				throw new ArgumentNullException("writingSystemDefinition");
			}
			string cacheName = String.Format("sortedByLexicalFormOrAlternative_{0}", writingSystemDefinition.LanguageTag);
			if (_caches[cacheName] == null)
			{
				var lexicalFormWithAlternativeQuery = new DelegateQuery<LexEntry>(
					delegate(LexEntry entryToQuery)
						{
							IDictionary<string, object> tokenFieldsAndValues = new Dictionary<string, object>();
							string lexicalForm = entryToQuery.LexicalForm[writingSystemDefinition.LanguageTag];
							string writingSystemOfForm = writingSystemDefinition.LanguageTag;
							if (lexicalForm == "")
							{
								lexicalForm = entryToQuery.LexicalForm.GetBestAlternative(writingSystemDefinition.LanguageTag);
								foreach (LanguageForm form in entryToQuery.LexicalForm.Forms)
								{
									if(form.Form == lexicalForm)
									{
										writingSystemOfForm = form.WritingSystemId;
									}
								}
								if (lexicalForm == "")
								{
									lexicalForm = null;
								}
							}
							tokenFieldsAndValues.Add("Form", lexicalForm);
							tokenFieldsAndValues.Add("WritingSystem", writingSystemOfForm);
							return new[] { tokenFieldsAndValues };
						});
				ResultSet<LexEntry> itemsMatching = _decoratedDataMapper.GetItemsMatching(lexicalFormWithAlternativeQuery);

				var sortOrder = new SortDefinition[1];
				sortOrder[0] = new SortDefinition("Form", writingSystemDefinition.DefaultCollation.Collator);

				_caches.Add(cacheName, new ResultSetCache<LexEntry>(this, sortOrder, itemsMatching, lexicalFormWithAlternativeQuery));
			}
			ResultSet<LexEntry> resultsFromCache = _caches[cacheName].GetResultSet();

			return resultsFromCache;
		}

		/// <summary>
		/// Gets a ResultSet containing all entries sorted by lexical form for a given writing system.
		/// Use "Form" to access the lexical form in a RecordToken.
		/// </summary>
		/// <param name="writingSystemDefinition"></param>
		/// <returns></returns>
		private ResultSet<LexEntry> GetAllEntriesSortedByLexicalForm(WritingSystemDefinition writingSystemDefinition)
		{
			if (writingSystemDefinition == null)
			{
				throw new ArgumentNullException("writingSystemDefinition");
			}
			string cacheName = String.Format("sortedByLexicalForm_{0}", writingSystemDefinition.LanguageTag);
			if (_caches[cacheName] == null)
			{
				var lexicalFormQuery = new DelegateQuery<LexEntry>(
					delegate(LexEntry entryToQuery)
						{
							var tokenFieldsAndValues = new Dictionary<string, object>();
							string headWord = entryToQuery.LexicalForm[writingSystemDefinition.LanguageTag];
							if (String.IsNullOrEmpty(headWord)){
								headWord = null;
							}
							tokenFieldsAndValues.Add("Form", headWord);
							return new[] { tokenFieldsAndValues };
						});
				ResultSet<LexEntry> itemsMatching = _decoratedDataMapper.GetItemsMatching(lexicalFormQuery);

				var sortOrder = new SortDefinition[1];
				sortOrder[0] = new SortDefinition("Form", writingSystemDefinition.DefaultCollation.Collator);

				_caches.Add(cacheName, new ResultSetCache<LexEntry>(this, sortOrder, itemsMatching, lexicalFormQuery));
			}
			ResultSet<LexEntry> resultsFromCache = _caches[cacheName].GetResultSet();

			return resultsFromCache;
		}

		private ResultSet<LexEntry> GetAllEntriesSortedByGuid()
		{
			string cacheName = String.Format("sortedByGuid");
			if (_caches[cacheName] == null)
			{
				var guidQuery = new DelegateQuery<LexEntry>(
					delegate(LexEntry entryToQuery)
						{
							IDictionary<string, object> tokenFieldsAndValues = new Dictionary<string, object>();
							tokenFieldsAndValues.Add("Guid", entryToQuery.Guid);
							return new[] { tokenFieldsAndValues };
						});
				ResultSet<LexEntry> itemsMatching = _decoratedDataMapper.GetItemsMatching(guidQuery);

				var sortOrder = new SortDefinition[1];
				sortOrder[0] = new SortDefinition("Guid", Comparer<Guid>.Default);

				_caches.Add(cacheName, new ResultSetCache<LexEntry>(this, sortOrder, itemsMatching, guidQuery));
			}
			ResultSet<LexEntry> resultsFromCache = _caches[cacheName].GetResultSet();

			return resultsFromCache;
		}

		private ResultSet<LexEntry> GetAllEntriesSortedById()
		{
			string cacheName = String.Format("sortedById");
			if (_caches[cacheName] == null)
			{
				var IdQuery = new DelegateQuery<LexEntry>(
					delegate(LexEntry entryToQuery)
						{
							IDictionary<string, object> tokenFieldsAndValues = new Dictionary<string, object>();
							tokenFieldsAndValues.Add("Id", entryToQuery.Id);
							return new[] { tokenFieldsAndValues };
						});
				ResultSet<LexEntry> itemsMatching = _decoratedDataMapper.GetItemsMatching(IdQuery);

				var sortOrder = new SortDefinition[1];
				sortOrder[0] = new SortDefinition("Id", Comparer<string>.Default);

				_caches.Add(cacheName, new ResultSetCache<LexEntry>(this, sortOrder, itemsMatching, IdQuery));
			}
			ResultSet<LexEntry> resultsFromCache = _caches[cacheName].GetResultSet();

			return resultsFromCache;
		}

		/// <summary>
		/// Gets a ResultSet containing all entries sorted by definition and gloss. It will return both the definition
		/// and the gloss if both exist and are different.
		/// Use "Form" to access the Definition/Gloss in RecordToken.
		/// </summary>
		/// <param name="writingSystemDefinition"></param>
		/// <returns>Definition and gloss in "Form" field of RecordToken</returns>
		public ResultSet<LexEntry> GetAllEntriesSortedByDefinitionOrGloss(WritingSystemDefinition writingSystemDefinition)
		{
			if (writingSystemDefinition == null)
			{
				throw new ArgumentNullException("writingSystemDefinition");
			}

			string cacheName = String.Format("SortByDefinition_{0}", writingSystemDefinition.LanguageTag);
			if (_caches[cacheName] == null)
			{
				var definitionQuery = new DelegateQuery<LexEntry>(
					delegate(LexEntry entryToQuery)
						{
							var fieldsAndValuesForRecordTokens = new List<IDictionary<string, object>>();

							int senseNumber = 0;
							foreach (LexSense sense in entryToQuery.Senses)
							{
								var rawDefinition = sense.Definition[writingSystemDefinition.LanguageTag];
								var definitions = GetTrimmedElementsSeparatedBySemiColon(rawDefinition);

								var rawGloss = sense.Gloss[writingSystemDefinition.LanguageTag];
								var glosses = GetTrimmedElementsSeparatedBySemiColon(rawGloss);

								var definitionAndGlosses = MergeListsWhileExcludingDoublesAndEmptyStrings(definitions, glosses);


								if(definitionAndGlosses.Count == 0)
								{
									IDictionary<string, object> tokenFieldsAndValues = new Dictionary<string, object>();
									tokenFieldsAndValues.Add("Form", null);
									tokenFieldsAndValues.Add("Sense", senseNumber);
									fieldsAndValuesForRecordTokens.Add(tokenFieldsAndValues);
								}
								else
								{
									foreach (string definition in definitionAndGlosses)
									{
										IDictionary<string, object> tokenFieldsAndValues = new Dictionary<string, object>();
										tokenFieldsAndValues.Add("Form", definition);
										tokenFieldsAndValues.Add("Sense", senseNumber);
										fieldsAndValuesForRecordTokens.Add(tokenFieldsAndValues);
									}
								}

								senseNumber++;
							}
							return fieldsAndValuesForRecordTokens;
						});
				ResultSet<LexEntry> itemsMatching = _decoratedDataMapper.GetItemsMatching(definitionQuery);

				var sortOrder = new SortDefinition[2];
				sortOrder[0] = new SortDefinition("Form", writingSystemDefinition.DefaultCollation.Collator);
				sortOrder[1] = new SortDefinition("Sense", Comparer<int>.Default);
				_caches.Add(cacheName, new ResultSetCache<LexEntry>(this, sortOrder, itemsMatching, definitionQuery));
			}
			return _caches[cacheName].GetResultSet();
		}

		private static List<string> MergeListsWhileExcludingDoublesAndEmptyStrings(IEnumerable<string> list1, IEnumerable<string> list2)
		{
			var mergedList = new List<string>();
			foreach (string definitionElement in list1)
			{
				if((!mergedList.Contains(definitionElement)) && (definitionElement != ""))
				{
					mergedList.Add(definitionElement);
				}
			}
			foreach (string glossElement in list2)
			{
				if (!mergedList.Contains(glossElement) && (glossElement != ""))
				{
					mergedList.Add(glossElement);
				}
			}
			return mergedList;
		}

		private static List<string> GetTrimmedElementsSeparatedBySemiColon(string text)
		{
			var textElements = new List<string>();
			foreach (string textElement in text.Split(new[] { ';' }))
			{
				string textElementTrimmed = textElement.Trim();
				textElements.Add(textElementTrimmed);
			}
			return textElements;
		}

		/// <summary>
		/// Gets a ResultSet containing entries that contain a semantic domain assigned to them
		/// sorted by semantic domain.
		/// Use "SemanticDomain" to access the semantic domain in a RecordToken.
		/// </summary>
		/// <param name="fieldName"></param>
		/// <returns></returns>
		public ResultSet<LexEntry> GetEntriesWithSemanticDomainSortedBySemanticDomain(
			string fieldName)
		{
			if (fieldName == null)
			{
				throw new ArgumentNullException("fieldName");
			}

			string cachename = String.Format("Semanticdomains_{0}", fieldName);

			if (_caches[cachename] == null)
			{
				var semanticDomainsQuery = new DelegateQuery<LexEntry>(
					delegate(LexEntry entry)
						{
							var fieldsAndValuesForRecordTokens = new List<IDictionary<string, object>>();
							foreach (LexSense sense in entry.Senses)
							{
								foreach (KeyValuePair<string, IPalasoDataObjectProperty> pair in sense.Properties)
								{
									if (pair.Key == fieldName)
									{
										var semanticDomains = (OptionRefCollection) pair.Value;
										foreach (string semanticDomain in semanticDomains.Keys)
										{
											IDictionary<string, object> tokenFieldsAndValues = new Dictionary<string, object>();
											string domain = semanticDomain;
											if (String.IsNullOrEmpty(semanticDomain))
											{
												domain = null;
											}
											if (CheckIfTokenHasAlreadyBeenReturnedForThisSemanticDomain(fieldsAndValuesForRecordTokens, domain))
											{
												continue; //This is to avoid duplicates
											}
											tokenFieldsAndValues.Add("SemanticDomain", domain);
											fieldsAndValuesForRecordTokens.Add(tokenFieldsAndValues);
										}
									}
								}
							}
							return fieldsAndValuesForRecordTokens;
						}
					);
				ResultSet<LexEntry> itemsMatchingQuery = GetItemsMatching(semanticDomainsQuery);
				var sortDefinition = new SortDefinition[2];
				sortDefinition[0] = new SortDefinition("SemanticDomain", StringComparer.InvariantCulture);
				sortDefinition[1] = new SortDefinition("Sense", Comparer<int>.Default);
				var cache =
					new ResultSetCache<LexEntry>(this, sortDefinition, itemsMatchingQuery, semanticDomainsQuery);
				_caches.Add(cachename, cache);
			}
			return _caches[cachename].GetResultSet();
		}

		private static bool CheckIfTokenHasAlreadyBeenReturnedForThisSemanticDomain(IEnumerable<IDictionary<string, object>> fieldsAndValuesForRecordTokens, string domain)
		{
			foreach (var tokenInfo in fieldsAndValuesForRecordTokens)
			{
				if((string)tokenInfo["SemanticDomain"] == domain)
				{
					return true;
				}
			}
			return false;
		}


		private ResultSet<LexEntry> GetAllEntriesWithGlossesSortedByLexicalForm(WritingSystemDefinition lexicalUnitWritingSystemDefinition)
		{
			if (lexicalUnitWritingSystemDefinition == null)
			{
				throw new ArgumentNullException("lexicalUnitWritingSystemDefinition");
			}
			string cachename = String.Format("GlossesSortedByLexicalForm_{0}", lexicalUnitWritingSystemDefinition);
			if (_caches[cachename] == null)
			{
				var MatchingGlossQuery = new DelegateQuery<LexEntry>(
					delegate(LexEntry entry)
						{
							var fieldsAndValuesForRecordTokens = new List<IDictionary<string, object>>();
							int senseNumber = 0;
							foreach (LexSense sense in entry.Senses)
							{
								foreach (LanguageForm form in sense.Gloss.Forms)
								{
									IDictionary<string, object> tokenFieldsAndValues = new Dictionary<string, object>();
									string lexicalForm = entry.LexicalForm[lexicalUnitWritingSystemDefinition.LanguageTag];
									if (String.IsNullOrEmpty(lexicalForm))
									{
										lexicalForm = null;
									}
									tokenFieldsAndValues.Add("Form", lexicalForm);

									string gloss = form.Form;
									if (String.IsNullOrEmpty(gloss))
									{
										gloss = null;
									}
									tokenFieldsAndValues.Add("Gloss", gloss);

									string glossWritingSystem = form.WritingSystemId;
									if (String.IsNullOrEmpty(glossWritingSystem))
									{
										glossWritingSystem = null;
									}
									tokenFieldsAndValues.Add("GlossWritingSystem", glossWritingSystem);
									tokenFieldsAndValues.Add("SenseNumber", senseNumber);
									fieldsAndValuesForRecordTokens.Add(tokenFieldsAndValues);
								}
								senseNumber++;
							}
							return fieldsAndValuesForRecordTokens;
						}
					);
				ResultSet<LexEntry> itemsMatchingQuery = GetItemsMatching(MatchingGlossQuery);
				var sortDefinition = new SortDefinition[4];
				sortDefinition[0] = new SortDefinition("Form", lexicalUnitWritingSystemDefinition.DefaultCollation.Collator);
				sortDefinition[1] = new SortDefinition("Gloss", StringComparer.InvariantCulture);
				sortDefinition[2] = new SortDefinition("GlossWritingSystem", StringComparer.InvariantCulture);
				sortDefinition[3] = new SortDefinition("SenseNumber", Comparer<int>.Default);
				var cache = new ResultSetCache<LexEntry>(this, sortDefinition, itemsMatchingQuery, MatchingGlossQuery);
				_caches.Add(cachename, cache);
			}
			return _caches[cachename].GetResultSet();
		}

		/// <summary>
		/// Gets a ResultSet containing entries whose gloss match glossForm sorted by the lexical form
		/// in the given writing system.
		/// Use "Form" to access the lexical form and "Gloss/Form" to access the Gloss in a RecordToken.
		/// </summary>
		/// <param name="glossForm"></param>
		/// <param name="lexicalUnitWritingSystemDefinition"></param>
		/// <returns></returns>
		public ResultSet<LexEntry> GetEntriesWithMatchingGlossSortedByLexicalForm(
			LanguageForm glossForm, WritingSystemDefinition lexicalUnitWritingSystemDefinition)
		{

			if (null==glossForm || string.IsNullOrEmpty(glossForm.Form))
			{
				throw new ArgumentNullException("glossForm");
			}
			if (lexicalUnitWritingSystemDefinition == null)
			{
				throw new ArgumentNullException("lexicalUnitWritingSystemDefinition");
			}
			var allGlossesResultSet = GetAllEntriesWithGlossesSortedByLexicalForm(lexicalUnitWritingSystemDefinition);
			var filteredResultSet = new List<RecordToken<LexEntry>>();
			foreach (RecordToken<LexEntry> recordToken in allGlossesResultSet)
			{
				if (((string) recordToken["Gloss"] == glossForm.Form)
					&& ((string) recordToken["GlossWritingSystem"] == glossForm.WritingSystemId))
				{
					filteredResultSet.Add(recordToken);
				}
			}

			return new ResultSet<LexEntry>(this, filteredResultSet);
		}

		/// <summary>
		/// Gets the LexEntry whose Id matches id.
		/// </summary>
		/// <returns></returns>
		public LexEntry GetLexEntryWithMatchingId(string id)
		{
			if (id == null)
			{
				throw new ArgumentNullException("id");
			}
			if (id == string.Empty)
			{
				throw new ArgumentOutOfRangeException("id", "The Id should not be empty.");
			}
			ResultSet<LexEntry> allGlossesResultSet = GetAllEntriesSortedById();
			var filteredResultSet = new List<RecordToken<LexEntry>>();
			foreach (RecordToken<LexEntry> recordToken in allGlossesResultSet)
			{
				if (((string)recordToken["Id"] == id))
				{
					filteredResultSet.Add(recordToken);
				}
			}
			if (filteredResultSet.Count > 1)
			{
				throw new ApplicationException("More than one entry exists with the guid " + id);
			}
			if (filteredResultSet.Count == 0)
			{
				return null;
			}
			return filteredResultSet[0].RealObject;
		}

		/// <summary>
		/// Gets the LexEntry whose Guid matches guid.
		/// </summary>
		/// <returns></returns>
		public LexEntry GetLexEntryWithMatchingGuid(Guid guid)
		{
			if(guid == Guid.Empty)
			{
				throw new ArgumentOutOfRangeException("guid", "Guids should not be empty!");
			}
			ResultSet<LexEntry> allGlossesResultSet = GetAllEntriesSortedByGuid();
			var filteredResultSet = new List<RecordToken<LexEntry>>();
			foreach (RecordToken<LexEntry> recordToken in allGlossesResultSet)
			{
				if (((Guid) recordToken["Guid"] == guid))
				{
					filteredResultSet.Add(recordToken);
				}
			}
			if(filteredResultSet.Count > 1)
			{
				throw new ApplicationException("More than one entry exists with the guid " + guid);
			}
			if(filteredResultSet.Count == 0)
			{
				return null;
			}
			return filteredResultSet[0].RealObject;
		}

		/// <summary>
		/// Gets a ResultSet containing entries whose lexical form is similar to lexicalForm
		/// sorted by the lexical form in the given writing system.
		/// Use "Form" to access the lexical form in a RecordToken.
		/// </summary>
		/// <returns></returns>
		public ResultSet<LexEntry> GetEntriesWithSimilarLexicalForm(string lexicalForm,
																	WritingSystemDefinition writingSystemDefinition,
																	ApproximateMatcherOptions
																		matcherOptions)
		{
			if (lexicalForm == null)
			{
				throw new ArgumentNullException("lexicalForm");
			}
			if (writingSystemDefinition == null)
			{
				throw new ArgumentNullException("writingSystemDefinition");
			}
			return new ResultSet<LexEntry>(this,
										   ApproximateMatcher.FindClosestForms
											   <RecordToken<LexEntry>>(
											   GetAllEntriesSortedByLexicalForm(writingSystemDefinition),
											   GetFormForMatchingStrategy,
											   lexicalForm,
											   matcherOptions));
		}

		private static string GetFormForMatchingStrategy(object item)
		{
			return (string) ((RecordToken<LexEntry>) item)["Form"];
		}

		/// <summary>
		/// Gets a ResultSet containing entries whose lexical form match lexicalForm
		/// Use "Form" to access the lexical form in a RecordToken.
		/// </summary>
		/// <param name="lexicalForm"></param>
		/// <param name="writingSystemDefinition"></param>
		/// <returns></returns>
		public ResultSet<LexEntry> GetEntriesWithMatchingLexicalForm(string lexicalForm,
																	 WritingSystemDefinition writingSystemDefinition)
		{
			if (lexicalForm == null)
			{
				throw new ArgumentNullException("lexicalForm");
			}
			if (writingSystemDefinition == null)
			{
				throw new ArgumentNullException("writingSystemDefinition");
			}
			ResultSet<LexEntry> allGlossesResultSet = GetAllEntriesSortedByLexicalForm(writingSystemDefinition);
			var filteredResultSet = new List<RecordToken<LexEntry>>();
			foreach (RecordToken<LexEntry> recordToken in allGlossesResultSet)
			{
				if (((string)recordToken["Form"] == lexicalForm))
				{
					filteredResultSet.Add(recordToken);
				}
			}

			return new ResultSet<LexEntry>(this, filteredResultSet);
		}

//
//        private string MakeSafeForFileName(string fileName)
//        {
//            foreach (char invalChar in Path.GetInvalidFileNameChars())
//            {
//                fileName = fileName.Replace(invalChar.ToString(), "");
//            }
//            return fileName;
//        }

		#region IDisposable Members

#if DEBUG
		~LiftLexEntryRepository()
		{
			if (!_disposed)
			{
				throw new ApplicationException(
					"Disposed not explicitly called on LexEntryRepository." + "\n" + _constructionStackTrace
					);
			}
		}
#endif

		private bool _disposed = true;

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!_disposed)
			{
				if (disposing)
				{
					// dispose-only, i.e. non-finalizable logic
					_decoratedDataMapper.Dispose();
				}

				// shared (dispose and finalizable) cleanup logic
				_disposed = true;
			}
		}

		protected void VerifyNotDisposed()
		{
			if (_disposed)
			{
				throw new ObjectDisposedException("LexEntryRepository");
			}
		}

		#endregion

		public int Count
		{
			get { return CountAllItems(); }
		}
	}
}