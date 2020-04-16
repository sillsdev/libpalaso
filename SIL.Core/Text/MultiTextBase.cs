using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Xml.Serialization;

namespace SIL.Text
{
	public class MultiTextBase : INotifyPropertyChanged, IComparable
	{
		/// <summary>
		/// We have this pesky "backreference" solely to enable fast
		/// searching in our current version of db4o (5.5), which
		/// can find strings fast, but can't be queried for the owner
		/// quickly, if there is an intervening collection.  Since
		/// each string in WeSay is part of a collection of writing
		/// system alternatives, that means we can't quickly get
		/// an answer, for example, to the question Get all
		/// the Entries that contain a senes which matches the gloss "cat".
		///
		/// Using this field, we can do a query asking for all
		/// the LanguageForms matching "cat".
		/// This can all be done in a single, fast query.
		///  In code, we can then follow the
		/// LanguageForm._parent up to the MultiTextBase, then this _parent
		/// up to it's owner, etc., on up the hierarchy to get the Entries.
		///
		/// Subclasses should provide a property which set the proper class.
		///
		/// 23 Jan 07, note: starting to switch to using these for notifying parent of changes, too.
		/// </summary>

		/// <summary>
		/// For INotifyPropertyChanged
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged;

		private LanguageForm[] _forms;
		public MultiTextBase()
		{
			_forms = new LanguageForm[0];
		}

		public void Add(Object objectFromSerializer) {}

		public static bool IsEmpty(MultiTextBase mt)
		{
			return mt == null || mt.Empty;
		}

		public static MultiTextBase Create(Dictionary<string, string> forms)
		{
			MultiTextBase m = new MultiTextBase();
			CopyForms(forms, m);
			return m;
		}

		protected static void CopyForms(Dictionary<string, string> forms, MultiTextBase m)
		{
			if (forms != null && forms.Keys != null)
			{
				foreach (string key in forms.Keys)
				{
					LanguageForm f = m.Find(key);
					if (f != null)
					{
						f.Form = forms[key];
					}
					else
					{
						m.SetAlternative(key, forms[key]);
					}
				}
			}
		}

		public bool GetAnnotationOfAlternativeIsStarred(string id)
		{
			LanguageForm alt = Find(id);
			if (alt == null)
			{
				return false;
			}
			return alt.IsStarred;
		}

		public void SetAnnotationOfAlternativeIsStarred(string id, bool isStarred)
		{
			LanguageForm alt = Find(id);
			if (isStarred)
			{
				if (alt == null)
				{
					AddLanguageForm(new LanguageForm(id, String.Empty, this));
					alt = Find(id);
					Debug.Assert(alt != null);
				}
				alt.IsStarred = true;
			}
			else
			{
				if (alt != null)
				{
					if (alt.Form == String.Empty) //non-starred and empty? Nuke it.
					{
						RemoveLanguageForm(alt);
					}
					else
					{
						alt.IsStarred = false;
					}
				}
				else
				{
					//nothing to do.  Missing altertive == not starred.
				}
			}
			NotifyPropertyChanged(id);
		}


		[XmlArrayItem(typeof (LanguageForm), ElementName = "tobedetermined")]
		public string this[string writingSystemId]
		{
			get => GetExactAlternative(writingSystemId);
			set => SetAlternative(writingSystemId, value);
		}

		public LanguageForm Find(string writingSystemId)
		{
			foreach (LanguageForm f in Forms)
			{
				if (f.WritingSystemId.ToLowerInvariant() == writingSystemId.ToLowerInvariant())
				{
					return f;
				}
			}
			return null;
		}

		/// <summary>
		/// Throws exception if alternative does not exist.
		/// </summary>
		/// <param name="writingSystemId"></param>
		/// <returns></returns>
//        public string GetExactAlternative(string writingSystemId)
//        {
//            if (!Contains(writingSystemId))
//            {
//                throw new ArgumentOutOfRangeException("Use Contains() to first check if the MultiTextBase has a language form for this writing system.");
//            }
//
//            return GetBestAlternative(writingSystemId, false, null);
//        }
		public bool ContainsAlternative(string writingSystemId)
		{
			return (Find(writingSystemId) != null);
		}

		/// <summary>
		/// Get exact alternative or String.Empty
		/// </summary>
		/// <param name="writingSystemId"></param>
		/// <returns></returns>
		public string GetExactAlternative(string writingSystemId)
		{
			return GetAlternative(writingSystemId, false, null);
		}

		/// <summary>
		/// Gets the Spans for the exact alternative or null.
		/// </summary>
		public List<LanguageForm.FormatSpan> GetExactAlternativeSpans(string writingSystemId)
		{
			LanguageForm alt = Find(writingSystemId);
			if (null == alt)
				return null;
			else
				return alt.Spans;
		}

		/// <summary>
		/// Gives the string of the requested id if it exists, else the 'first'(?) one that does exist, else Empty String
		/// </summary>
		/// <returns></returns>
		public string GetBestAlternative(string writingSystemId)
		{
			return GetAlternative(writingSystemId, true, string.Empty);
		}

		public string GetBestAlternative(string writingSystemId, string notFirstChoiceSuffix)
		{
			return GetAlternative(writingSystemId, true, notFirstChoiceSuffix);
		}

		/// <summary>
		/// Get a string out
		/// </summary>
		/// <returns>the string of the requested id if it exists,
		/// else the 'first'(?) one that does exist + the suffix,
		/// else the given suffix </returns>
		private string GetAlternative(string writingSystemId, bool doShowSomethingElseIfMissing,
									  string notFirstChoiceSuffix)
		{
			LanguageForm alt = Find(writingSystemId);
			if (null == alt)
			{
				if (doShowSomethingElseIfMissing)
				{
					return GetFirstAlternative() + notFirstChoiceSuffix;
				}
				else
				{
					return string.Empty;
				}
			}
			string form = alt.Form;
			if (form == null || (form.Trim().Length == 0))
			{
				if (doShowSomethingElseIfMissing)
				{
					return GetFirstAlternative() + notFirstChoiceSuffix;
				}
				else
				{
					return string.Empty;
				}
			}
			else
			{
				return form;
			}
		}

		public string GetFirstAlternative()
		{
			foreach (LanguageForm form in Forms)
			{
				if (form.Form.Trim().Length > 0)
				{
					return form.Form;
				}
			}
			return string.Empty;
		}

		public string GetBestAlternativeString(IEnumerable<string> orderedListOfWritingSystemIds)
		{
			LanguageForm form = GetBestAlternative(orderedListOfWritingSystemIds);
			if (form == null)
				return string.Empty;
			return form.Form;
		}

		/// <summary>
		/// Try to get an alternative according to the ws's given(e.g. the enabled writing systems for a field)
		/// </summary>
		/// <param name="orderedListOfWritingSystemIds"></param>
		/// <returns>May return null!</returns>
		public LanguageForm GetBestAlternative(IEnumerable<string> orderedListOfWritingSystemIds)
		{
			foreach (string id in orderedListOfWritingSystemIds)
			{
				LanguageForm alt = Find(id);
				if (null != alt)
					return alt;
			}

//            //just send back an empty
//            foreach (string id in orderedListOfWritingSystemIds)
//            {
//                return new LanguageForm(id, string.Empty );
//            }
			return null;
		}

		public bool Empty => Count == 0;

		public int Count => Forms.Length;

		/// <summary>
		/// just for deserialization
		/// </summary>
		[XmlElement(typeof (LanguageForm), ElementName="form")]
		public LanguageForm[] Forms
		{
			get
			{
				if (_forms == null)
				{
					 throw new ApplicationException("The LanguageForms[] attribute of this entry was null.  This is a symptom of a mismatch between a cache and WeSay model.  Please delete the cache.");
				}
				return _forms;
			}
			set => _forms = value;
		}


		public void SetAlternative(string writingSystemId, string form)
		{
			Debug.Assert(!string.IsNullOrEmpty(writingSystemId), "The writing system id was empty.");
			Debug.Assert(writingSystemId.Trim() == writingSystemId,
						 "The writing system id had leading or trailing whitespace");

			//enhance: check to see if there has actually been a change

			LanguageForm alt = Find(writingSystemId);
			if (string.IsNullOrEmpty(form)) // we don't use space to store empty strings.
			{
				if (alt != null && !alt.IsStarred)
				{
					RemoveLanguageForm(alt);
				}
			}
			else
			{
				if (alt != null)
				{
					alt.Form = form;
				}
				else
				{
					AddLanguageForm(new LanguageForm(writingSystemId, form, this));
				}
			}

			NotifyPropertyChanged(writingSystemId);
		}

		public void RemoveLanguageForm(LanguageForm languageForm)
		{
			Debug.Assert(Forms.Length > 0);
			LanguageForm[] forms = new LanguageForm[Forms.Length - 1];
			for (int i = 0, j = 0; i < forms.Length; i++,j++)
			{
				if (Forms[j] == languageForm)
				{
					j++;
				}
				forms[i] = Forms[j];
			}
			_forms = forms;
		}

		protected void AddLanguageForm(LanguageForm languageForm)
		{
			LanguageForm[] forms = new LanguageForm[Forms.Length + 1];
			for (int i = 0; i < Forms.Length; i++)
			{
				forms[i] = Forms[i];
			}

			//actually copy the contents, as we must now be the parent
			forms[Forms.Length] = new LanguageForm(languageForm, this);
			Array.Sort(forms);
			_forms = forms;
		}

		protected void NotifyPropertyChanged(string writingSystemId)
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(writingSystemId));
			}
		}

		public IEnumerator GetEnumerator()
		{
			return Forms.GetEnumerator();
		}

		public int CompareTo(object o)
		{

			if(o == null)
			{
				return 1;
			}
			if (!(o is MultiTextBase))
			{
				throw new ArgumentException("Can not compare to objects other than MultiTextBase.");
			}
			MultiTextBase other = (MultiTextBase) o;
			int formLengthOrder = this.Forms.Length.CompareTo(other.Forms.Length);
			if(formLengthOrder != 0)
			{
				return formLengthOrder;
			}
			for(int i = 0; i < Forms.Length; i++)
			{
				int languageFormOrder = Forms[i].CompareTo(other.Forms[i]);
				if(languageFormOrder != 0)
				{
					return languageFormOrder;
				}
			}
			return 0;
		}

		public override string ToString()
		{
			return GetFirstAlternative();
		}

		public LanguageForm[] GetOrderedAndFilteredForms(IEnumerable<string> writingSystemIdsInOrder)
		{
			List<LanguageForm> forms = new List<LanguageForm>();
			foreach (string id in writingSystemIdsInOrder)
			{
				LanguageForm form = Find(id);
				if(form!=null)
				{
					forms.Add(form);
				}
			}
			return forms.ToArray();
		}


		public void MergeInWithAppend(MultiTextBase incoming, string separator)
		{
			foreach (LanguageForm form in incoming)
			{
				LanguageForm f = Find(form.WritingSystemId);
				if (f != null)
				{
					f.Form += separator + form.Form;
				}
				else
				{
					AddLanguageForm(form); //this actually copies the meat of the form
				}
			}
		}

		public void MergeIn(MultiTextBase incoming)
		{
			foreach (LanguageForm form in incoming)
			{
				LanguageForm f = Find(form.WritingSystemId);
				if (f != null)
				{
					f.Form = form.Form;
				}
				else
				{
					AddLanguageForm(form); //this actually copies the meat of the form
				}
			}
		}


		/// <summary>
		/// Will merge the two mt's if they are compatible; if they collide anywhere, leaves the original untouched
		/// and returns false
		/// </summary>
		/// <param name="incoming"></param>
		/// <returns></returns>
		public bool TryMergeIn(MultiTextBase incoming)
		{
			//abort if they collide
			if (!CanBeUnifiedWith(incoming))
				return false;

			MergeIn(incoming);
			return true;
		}

		/// <summary>
		/// False if they have different forms on any single writing system. If true, they can be safely merged.
		/// </summary>
		/// <param name="incoming"></param>
		/// <returns></returns>
		public bool CanBeUnifiedWith(MultiTextBase incoming)
		{
			foreach (var form in incoming.Forms)
			{
				if (!ContainsAlternative(form.WritingSystemId))
					continue;//no problem, we don't have one of those
				if (GetExactAlternative(form.WritingSystemId) != form.Form)
					return false;
			}
			return true;
		}

		public override bool Equals(Object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != typeof(MultiTextBase)) return false;
			return Equals((MultiTextBase)obj);
		}

		public bool Equals(MultiTextBase other)
		{
			if (other == null) return false;
			if (other.Count != Count) return false;
			if (!_forms.SequenceEqual(other.Forms)) return false;
			return true;
		}

		public override int GetHashCode()
		{
			// https://stackoverflow.com/a/263416/487503
			unchecked // Overflow is fine, just wrap
			{
				var hash = 23;
				hash *= 29 + Count.GetHashCode();
				hash *= 29 + Forms.GetHashCode();
				return hash;
			}
		}

		public bool HasFormWithSameContent(MultiTextBase other)
		{
			if (other.Count == 0 && Count == 0)
			{
				return true;
			}
			foreach (LanguageForm form in other)
			{
				if (ContainsEqualForm(form))
				{
					return true;
				}
			}
			return false;
		}

		public bool ContainsEqualForm(LanguageForm other)
		{
			foreach (LanguageForm form in Forms)
			{
				if (other.Equals(form))
				{
					return true;
				}
			}
			return false;
		}
	}
}