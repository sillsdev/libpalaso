using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Palaso.Annotations;
using Palaso.Code;


namespace Palaso.Text
{
	/// <summary>
	/// A LanguageForm is a unicode string plus the id of its writing system
	/// </summary>
	public class LanguageForm : Annotatable, IComparable<LanguageForm>
	{
		private string _writingSystemId;
		private string _form;
		private readonly List<FormatSpan> _spans = new List<FormatSpan>();

		/// <summary>
		/// See the comment on MultiText._parent for information on
		/// this field.
		/// </summary>
		private MultiTextBase _parent;

		/// <summary>
		/// for netreflector
		/// </summary>
		public LanguageForm()
		{
		}

		public LanguageForm(string writingSystemId, string form, MultiTextBase parent)
		{
			_parent = parent;
			_writingSystemId = writingSystemId;
			_form =  form;
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
		public string Form
		{
			get { return _form; }
			set { _form = value; }
		}

		[XmlIgnore]
		public List<FormatSpan> Spans
		{
			get { return _spans; }
		}

		/// <summary>
		/// See the comment on MultiText._parent for information on
		/// this field.
		/// </summary>
		[XmlIgnore]
		public MultiTextBase Parent
		{
			get { return _parent; }
		}

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
			if ((_annotation != null && !_annotation.Equals(other._annotation)) || (other._annotation != null && !other._annotation.Equals(_annotation))) return false;
			if (_spans != other.Spans)
			{
				if (_spans == null || other.Spans == null || _spans.Count != other.Spans.Count) return false;
				for (int i = 0; i < _spans.Count; ++i)
				{
					if (!_spans[i].Equals(other.Spans[i])) return false;
				}
			}
			return true;
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
			clone._form = _form;
			clone._annotation = _annotation == null ? null : _annotation.Clone();
			foreach (var span in _spans)
				clone._spans.Add(new FormatSpan{Index=span.Index, Length=span.Length, Class=span.Class, Lang=span.Lang, LinkURL=span.LinkURL});
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
				var that = other as FormatSpan;
				if (that == null)
					return false;
				if (this.Index != that.Index || this.Length!= that.Length)
					return false;
				return (this.Class == that.Class && this.Lang == that.Lang && this.LinkURL == that.LinkURL);
			}
		}

		public void AddSpan(int index, int length, string lang, string style, string url)
		{
			var span = new FormatSpan {Index=index, Length=length, Lang=lang, Class=style, LinkURL=url};
			_spans.Add(span);
		}
	}
}