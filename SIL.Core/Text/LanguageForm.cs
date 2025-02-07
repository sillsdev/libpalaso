using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using SIL.Annotations;

namespace SIL.Text
{
	/// <summary>
	/// A LanguageForm is a unicode string plus the id of its writing system
	/// </summary>
	public class LanguageForm : Annotatable, IComparable<LanguageForm>
	{
		private string _writingSystemId;

		/// <summary>
		/// for netreflector
		/// </summary>
		public LanguageForm()
		{
		}

		public LanguageForm(string writingSystemId, string form, MultiTextBase parent)
		{
			Parent = parent;
			_writingSystemId = writingSystemId;
			Form =  form;
		}

		public LanguageForm(LanguageForm form, MultiTextBase parent)
			: this(form.WritingSystemId, form.Form, parent)
		{
			foreach (var span in form.Spans)
				AddSpan(span.Index, span.Length, span.Lang, span.Class, span.LinkURL);
		}

		[XmlAttribute("ws")]
		public string WritingSystemId
		{
			get { return _writingSystemId; }

			// needed for depersisting with netreflector
			set
			{
				_writingSystemId = value;
			}
		}

		[XmlText]
		public string Form { get; set; }

		[XmlIgnore]
		public List<FormatSpan> Spans { get; } = new List<FormatSpan>();

		/// <summary>
		/// See the comment on MultiText._parent for information on
		/// this field.
		/// </summary>
		[XmlIgnore]
		public MultiTextBase Parent { get; }

		public override bool Equals(object other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			if (other.GetType() != typeof(LanguageForm)) return false;
			return Equals((LanguageForm)other);
		}

		public bool Equals(LanguageForm other)
		{
			if(other == null) return false;
			if (!IsStarred.Equals(other.IsStarred)) return false;
			if ((WritingSystemId != null && !WritingSystemId.Equals(other.WritingSystemId)) || (other.WritingSystemId != null && !other.WritingSystemId.Equals(WritingSystemId))) return false;
			if ((Form != null && !Form.Equals(other.Form)) || (other.Form != null && !other.Form.Equals(Form))) return false;
			if ((Annotation != null && !Annotation.Equals(other.Annotation)) || (other.Annotation != null && !other.Annotation.Equals(Annotation))) return false;
			if (Spans != other.Spans)
			{
				if (Spans == null || other.Spans == null || Spans.Count != other.Spans.Count) return false;
				for (int i = 0; i < Spans.Count; ++i)
				{
					if (!Spans[i].Equals(other.Spans[i])) return false;
				}
			}
			return true;
		}

		public override int GetHashCode()
		{
			// https://stackoverflow.com/a/263416/487503
			unchecked // Overflow is fine, just wrap
			{
				var hash = 61;
				hash *= 71 + IsStarred.GetHashCode();
				hash *= 71 + WritingSystemId?.GetHashCode() ?? 0;
				hash *= 71 + Form?.GetHashCode() ?? 0;
				hash *= 71 + Annotation?.GetHashCode() ?? 0;
				hash *= 71 + Spans?.GetHashCode() ?? 0;
				hash *= 71 + base.GetHashCode();
				return hash;
			}
		}

		public int CompareTo(LanguageForm other)
		{
			if(other == null)
			{
				return 1;
			}
			int writingSystemOrder = this.WritingSystemId.CompareTo(other.WritingSystemId);
			if (writingSystemOrder != 0)
			{
				return writingSystemOrder;
			}
			int formOrder = this.Form.CompareTo(other.Form);
			return formOrder;
		}

		public override Annotatable Clone()
		{
			var clone = new LanguageForm();
			clone._writingSystemId = _writingSystemId;
			clone.Form = Form;
			clone.Annotation = Annotation == null ? null : Annotation.Clone();
			foreach (var span in Spans)
				clone.Spans.Add(new FormatSpan{Index=span.Index, Length=span.Length, Class=span.Class, Lang=span.Lang, LinkURL=span.LinkURL});
			return clone;
		}

		/// <summary>
		/// Store formatting information for one span of characters in the form.
		/// (This information is not used, but needs to be maintained for output.)
		/// </summary>
		/// <remarks>
		/// Although the LIFT standard officially allows nested spans, we don't
		/// bother because FieldWorks doesn't support them.
		/// </remarks>
		public class FormatSpan
		{
			/// <summary>Store the starting index of the span</summary>
			public int Index { get; set; }
			/// <summary>Store the length of the span</summary>
			public int Length { get; set; }
			/// <summary>Store the language of the data in the span</summary>
			public string Lang { get; set; }
			/// <summary>Store the class (style) applied to the data in the span</summary>
			public string Class { get; set; }
			/// <summary>Store the underlying URL link of the span</summary>
			public string LinkURL { get; set; }

			public override bool Equals(object other)
			{
				if (!(other is FormatSpan that))
					return false;
				if (this.Index != that.Index || this.Length!= that.Length)
					return false;
				return (this.Class == that.Class && this.Lang == that.Lang && this.LinkURL == that.LinkURL);
			}

			public override int GetHashCode()
			{
				return Index.GetHashCode() ^ Length.GetHashCode() ^ Class.GetHashCode() ^
					Lang.GetHashCode() ^ LinkURL.GetHashCode();
			}
		}

		public void AddSpan(int index, int length, string lang, string style, string url)
		{
			var span = new FormatSpan {Index=index, Length=length, Lang=lang, Class=style, LinkURL=url};
			Spans.Add(span);
		}

		/// <summary>
		/// Adjusts the spans for the changes from oldText to newText.  Assume that only one chunk of
		/// text has changed between the two strings.
		/// </summary>
		public static void AdjustSpansForTextChange(string oldText, string newText, List<FormatSpan> spans)
		{
			// Be paranoid and check for null strings...  Just convert them to empty strings for our purposes.
			// (I don't think they'll ever be null, but that fits the category of famous last words...)
			if (oldText == null)
				oldText = String.Empty;
			if (newText == null)
				newText = String.Empty;
			// If there are no spans, the text hasn't actually changed, or the text length hasn't changed,
			// then we'll assume we don't need to do anything.
			if (spans == null || spans.Count == 0 || newText == oldText || newText.Length == oldText.Length)
				return;
			// Get the locations of the first changing character in the old string.
			// We assume that OnTextChanged gets called for every single change, so that
			// pinpoints the location, and the change in string length tells us the length
			// of the change itself. (>0 = insertion, <0 = deletion)
			int minLength = Math.Min(newText.Length, oldText.Length);
			int diffLocation = minLength;
			for (int i = 0; i < minLength; ++i)
			{
				if (newText[i] != oldText[i])
				{
					diffLocation = i;
					break;
				}
			}
			int diffLength = newText.Length - oldText.Length;
			foreach (var span in spans)
				AdjustSpan(span, diffLocation, diffLength);
		}

		private static void AdjustSpan(FormatSpan span, int location, int length)
		{
			if (span.Length <= 0)
				return;		// we've already deleted all the characters in the span.
			if (location > span.Index + span.Length || length == 0)
				return;		// the change doesn't affect the span.
			// Adding characters to the end of the span is probably desirable if they differ.
			if (length > 0)
			{
				// Inserting characters is fairly easy to deal with.
				if (location <= span.Index)
					span.Index += length;
				else
					span.Length += length;
			}
			// The span changes only if the deletion starts before the end of the span.
			else if (location < span.Index + span.Length)
			{
				// Deleting characters has a number of cases to deal with.
				// Remember, length is a negative number here!
				if (location < span.Index)
				{
					if (span.Index + length >= location)
					{
						span.Index += length;
					}
					else
					{
						int before = span.Index - location;
						span.Index = location;
						span.Length += (before + length);
					}
				}
				else
				{
					span.Length += length;
					if (span.Length < location - span.Index)
						span.Length = location - span.Index;
				}
			}
		}
	}
}