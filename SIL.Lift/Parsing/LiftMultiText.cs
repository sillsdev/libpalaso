using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace SIL.Lift.Parsing
{
	/// <summary>
	/// This class represents the formatting information for a span of text.
	/// </summary>
	public class LiftSpan
	{
		int _index;			// index of first character covered by the span in the string
		int _length;		// length of span in the string
		string _lang;		// lang attribute value for the span, if any
		string _class;		// class attribute value for the span, if any
		string _linkurl;	// href attribute value for the span, if any
		readonly List<LiftSpan> _spans = new List<LiftSpan>();

		/// <summary>
		/// Constructor.
		/// </summary>
		public LiftSpan(int index, int length, string lang, string className, string href)
		{
			_index = index;
			_length = length;
			_lang = lang;
			_class = className;
			_linkurl = href;
		}

		/// <summary>This must be overridden for tests to pass.</summary>
		public override bool Equals(object obj)
		{
			LiftSpan that = obj as LiftSpan;
			if (that == null)
				return false;
			if (_index != that._index)
				return false;
			if (_length != that._length)
				return false;
			if (_lang != that._lang)
				return false;
			if (_class != that._class)
				return false;
			if (_linkurl != that._linkurl)
				return false;
			return true;
		}

		/// <summary>Keep ReSharper from complaining about Equals().</summary>
		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		///<summary>
		/// Get the index of this span within the overall LiftMultiText string.
		///</summary>
		public int Index
		{
			get { return _index; }
		}
		/// <summary>
		/// Get the length of this span.
		/// </summary>
		public int Length
		{
			get { return _length; }
		}
		/// <summary>
		/// Get the language of the data in this span.
		/// </summary>
		public string Lang
		{
			get { return _lang; }
		}
		/// <summary>
		/// Get the class (style) applied to the data in this span.
		/// </summary>
		public string Class
		{
			get { return _class; }
		}
		/// <summary>
		/// Get the underlying link URL of this span (if any).
		/// </summary>
		public string LinkURL
		{
			get { return _linkurl; }
		}

		/// <summary>
		/// Return the list of format specifications, if any.
		/// </summary>
		public List<LiftSpan> Spans
		{
			get { return _spans; }
		}
	}

	/// <summary>
	/// This class represents a string with optional embedded formatting information.
	/// </summary>
	public class LiftString
	{
		string _text;
		readonly List<LiftSpan> _spans = new List<LiftSpan>();

		/// <summary>
		/// Default constructor.
		/// </summary>
		public LiftString()
		{
		}
		/// <summary>
		/// Constructor with simple C# string data.
		/// </summary>
		public LiftString(string simpleContent)
		{
			Text = simpleContent;
		}

		/// <summary>
		/// Get the text of this string.
		/// </summary>
		public string Text
		{
			get { return _text; }
			set { _text = value; }
		}

		/// <summary>
		/// Return the list of format specifications, if any.
		/// </summary>
		public List<LiftSpan> Spans
		{
			get { return _spans; }
		}

		/// <summary>This must be overridden for tests to pass.</summary>
		public override bool Equals(object obj)
		{
			LiftString that = obj as LiftString;
			if (that == null)
				return false;
			if (Text != that.Text)
				return false;
			if (Spans.Count != that.Spans.Count)
				return false;
			for (int i = 0; i < Spans.Count; ++i)
			{
				if (!Spans[i].Equals(that.Spans[i]))
					return false;
			}
			return true;
		}

		/// <summary>Keep ReSharper from complaining about Equals().</summary>
		public override int GetHashCode()
		{
			return base.GetHashCode();
		}
	}

	/// <summary>
	/// This class represents a multilingual string, possibly with embedded formatting
	/// information in each of the alternatives.
	/// </summary>
	public class LiftMultiText : Dictionary<string, LiftString>
	{
		private List<Annotation> _annotations = new List<Annotation>();
		private readonly string _OriginalRawXml;

		/// <summary>
		/// Default constructor.
		/// </summary>
		public LiftMultiText()
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		public LiftMultiText(string rawXml)
		{
			_OriginalRawXml = rawXml;
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		public LiftMultiText(string key, string simpleContent)
		{
			Add(key, new LiftString(simpleContent));
		}

		/// <summary></summary>
		public override string ToString()
		{
			StringBuilder b = new StringBuilder();
			foreach (string key in Keys)
			{
				b.AppendFormat("{0}={1}|", key, this[key].Text);
			}
			return b.ToString();
		}

		/// <summary>This must be overridden for tests to pass.</summary>
		public override bool Equals(object obj)
		{
			LiftMultiText that = obj as LiftMultiText;
			if (that == null)
				return false;
			if (Keys.Count != that.Keys.Count)
				return false;
			foreach (string key in Keys)
			{
				LiftString otherString;
				if (!that.TryGetValue(key, out otherString))
					return false;
				if (!this[key].Equals(otherString))
					return false;
			}
			return true;
		}

		/// <summary>Keep ReSharper from complaining about Equals().</summary>
		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		/// <summary>
		/// For WeSay, which doesn't yet understand structured strings
		/// </summary>
		public Dictionary<string, string> AsSimpleStrings
		{
			get
			{
				Dictionary<string, string> result = new Dictionary<string, string>();
				foreach (KeyValuePair<string, LiftString> pair in this)
				{
					if (pair.Value != null)
					{
						result.Add(pair.Key, pair.Value.Text);
					}
				}

				return result;
			}
		}

		/// <summary>
		/// For WeSay, to allow spans to be carried along even if not used.
		/// </summary>
		public Dictionary<string, List<LiftSpan>> AllSpans
		{
			get
			{
				Dictionary<string, List<LiftSpan>> result = new Dictionary<string, List<LiftSpan>>();
				foreach (KeyValuePair<string, LiftString> pair in this)
				{
					if (pair.Value != null)
					{
						result.Add(pair.Key, pair.Value.Spans);
					}
				}
				return result;
			}
		}

		/// <summary>
		/// Check whether this LiftMultiText is empty.
		/// </summary>
		public bool IsEmpty
		{
			get
			{
				return Count == 0;
			}
		}

		///<summary>
		/// Return the first KeyValuePair in this LiftMultiText.
		///</summary>
		public KeyValuePair<string,LiftString> FirstValue
		{
			get
			{
				Enumerator enumerator = GetEnumerator();
				enumerator.MoveNext();
				return enumerator.Current;
			}
		}

		/// <summary>
		/// Get/set the annotations for this LiftMultiText.
		/// </summary>
		public List<Annotation> Annotations
		{
			get
			{
				return _annotations;
			}
			set
			{
				_annotations = value;
			}
		}

		/// <summary>
		/// Get the original XML string used to initialize this LiftMultiText (if any).
		/// </summary>
		public string OriginalRawXml
		{
			get { return _OriginalRawXml; }
		}

		/// <summary>
		/// Add data to the beginning of a particular alternative of this LiftMultiText.
		/// </summary>
		/// <remarks>
		/// TODO: update the offsets of any spans after the first in that alternative,
		/// and the length of the first span.
		/// </remarks>
		public void Prepend(string key, string prepend)
		{
			LiftString existing;
			if (TryGetValue(key, out existing))
			{
				this[key].Text = prepend + existing.Text;
				return;
			}
			Debug.Fail("Tried to prepend to empty alternative "); //don't need to stop in release versions
		}

		// Add this method if you think we really need backward compatibility...
		//public void Add(string key, string text)
		//{
		//    LiftString str = new LiftString();
		//    str.Text = text;
		//    this.Add(key, str);
		//}

		/// <summary>
		/// if we already have a form in the lang, add the new one after adding the delimiter e.g. "tube; hose"
		/// </summary>
		/// <param name="key"></param>
		/// <param name="newValue"></param>
		/// <param name="delimiter"></param>
		public void AddOrAppend(string key, string newValue, string delimiter)
		{
			LiftString existing;
			if (TryGetValue(key, out existing))
			{
				if (String.IsNullOrEmpty(existing.Text))
					this[key].Text = newValue;
				else
					this[key].Text = existing.Text + delimiter + newValue;
			}
			else
			{
				LiftString alternative = new LiftString();
				alternative.Text = newValue;
				this[key] = alternative;
			}
		}

		/// <summary>
		/// Merge another LiftMultiText with a common ancestor into this LiftMultiText.
		/// </summary>
		public void Merge(LiftMultiText theirs, LiftMultiText ancestor)
		{
			// TODO: implement this?!
		}

		/// <summary>
		/// Return the length of the text stored for the given language code, or zero if that
		/// alternative doesn't exist.
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public int LengthOfAlternative(string key)
		{
			LiftString existing;
			if (TryGetValue(key, out existing))
				return existing.Text == null ? 0 : existing.Text.Length;
			return 0;
		}

		/// <summary>
		/// Add another alternative to this LiftMultiText.
		/// </summary>
		public void Add(string key, string simpleContent)
		{
			Add(key, new LiftString(simpleContent));
		}

		/// <summary>
		/// Add another span to the given alternative, creating the alternative if needed.
		/// </summary>
		public LiftSpan AddSpan(string key, string lang, string style, string href, int length)
		{
			LiftString alternative;
			if (!TryGetValue(key, out alternative))
			{
				alternative = new LiftString();
				this[key] = alternative;
			}
			int start = alternative.Text?.Length ?? 0;
			if (lang == key)
				lang = null;
			var span = new LiftSpan(start, length, lang, style, href);
			alternative.Spans.Add(span);
			return span;
		}
	}
}