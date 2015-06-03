using System.Collections.Generic;

namespace SIL.Lift.Parsing
{
	/// <summary>
	/// This class implements the LIFT concept of a trait.
	/// </summary>
	public class Trait
	{
		private string _forFormInWritingSystem;
		private string _name;
		private string _value;

		private List<Annotation> _annotations = new List<Annotation>();

		/// <summary>
		/// Constructor.
		/// </summary>
		public Trait(string name, string value)
		{
			_name = name;
			_value = value;
		}
		/// <summary>
		/// Get/set the name of the trait.
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
		/// Get/set the value of the trait.
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
		/// Get/set the list of annotations for this trait.
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

		/// <summary></summary>
		public override bool Equals(object obj)
		{
			Trait that = obj as Trait;
			if (that == null)
				return false;
			if (_name != that._name)
				return false;
			if (_value != that._value)
				return false;
			if((_annotations==null) != (that._annotations==null))
				return false;
			if (that._annotations != null && _annotations != null &&
				_annotations.Count != that._annotations.Count)
			{
				return false;
			}
			int matches = 0;
			if (_annotations != null)
			{
				foreach (Annotation annotation in _annotations)
				{
					foreach (Annotation thatAnnotation in that.Annotations)
					{
						if(annotation == thatAnnotation)
						{
							matches++;
							break;
						}
					}
				}
				if(matches < _annotations.Count)
					return false;
			}
			return true;
		}

		// Keep ReSharper from complaining.  Maybe this should be revised?
		/// <summary></summary>
		public override int GetHashCode()
		{
			return base.GetHashCode();
		}
	}
}