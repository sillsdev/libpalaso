using System;
using System.Xml.Serialization;
using Palaso.Annotations;
using Palaso.Code;


namespace Palaso.Text
{
	/// <summary>
	/// A LanguageForm is a unicode string plus the id of its writing system
	/// </summary>
   // [ReflectorType("alt")]
	public class LanguageForm : Annotatable, IComparable<LanguageForm>
	{
		private string _writingSystemId;
		private string _form;

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

		//[ReflectorProperty("ws", Required = true)]
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

		//[ReflectorProperty("form", Required = true)]
		[XmlText]
		public string Form
		{
			get { return _form; }
			set { _form = value; }
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
			return clone;
		}
	}
}