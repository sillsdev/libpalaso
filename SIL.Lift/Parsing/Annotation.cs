using System;

namespace SIL.Lift.Parsing
{
	///<summary>
	/// This class implements the LIFT concept of an annotation.
	///</summary>
	public class Annotation
	{
		private string _name;
		private string _value;
		private DateTime _when;
		private string _who;
		private string _forFormInWritingSystem;

		/// <summary>
		/// Constructor.
		/// </summary>
		public Annotation(string name, string value, DateTime when, string who)
		{
			_name = name;
			_who = who;
			_when = when;
			_value = value;
		}

		/// <summary>
		/// Get/set the name of the annotation.
		/// </summary>
		public string Name
		{
			get
			{
				return _name;
			}
			set
			{
				_name = value;
			}
		}
		/// <summary>
		/// This is an index into the forms... !!!!!!!this probably not going to really work
		/// </summary>
		public string LanguageHint
		{
			get
			{
				return _forFormInWritingSystem;
			}
			set
			{
				_forFormInWritingSystem = value;
			}
		}
		/// <summary>
		/// Get/set the value of the annotation.
		/// </summary>
		public string Value
		{
			get
			{
				return _value;
			}
			set
			{
				_value = value;
			}
		}
		/// <summary>
		/// Get/set when the annotation was made.
		/// </summary>
		public DateTime When
		{
			get { return _when; }
			set { _when = value; }
		}
		/// <summary>
		/// Get/set who made the annotation.
		/// </summary>
		public string Who
		{
			get { return _who; }
			set { _who = value; }
		}

		/// <summary></summary>
		public override bool Equals(object obj)
		{
			Annotation that = obj as Annotation;
			if (that == null)
				return false;
			if (_name != that._name)
				return false;
			if (When != that.When)
				return false;
			if (_value != that._value)
				return false;
			if (Who != that.Who)
				return false;
			return true;
		}

		/// <summary>Keep ReSharper from complaining about Equals().</summary>
		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

	}
}