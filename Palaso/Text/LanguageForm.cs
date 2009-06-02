using System;
using System.Xml.Serialization;
using Palaso.Annotations;


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
			if (parent == null)
			{
				throw new ArgumentException("Parent cannot be null unless using for non-db4o purposes (e.g. netreflector an options)", "parent");
			}
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

		#region IEquatable<LanguageForm> Members

		public bool Equals(LanguageForm other)
		{
			if (IsStarred != other.IsStarred)
			{
				return false;
			}
			if (WritingSystemId != other.WritingSystemId)
			{
				return false;
			}
			if(Form != other.Form)
			{
				return false;
			}
			return true;
		}

		#endregion

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
	}
}