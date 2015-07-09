using System;
using System.Xml.Serialization;
using SIL.ObjectModel;

namespace SIL.Annotations
{
	public class Annotatable : IAnnotatable, ICloneable<Annotatable>, IEquatable<Annotatable>
	{
		protected Annotation _annotation;

		[XmlAttribute("starred")]
		public bool IsStarred
		{
			get
			{
				if (_annotation == null)
				{
					return false; // don't bother making one yet
				}
				return _annotation.IsOn;
			}
			set
			{
				if (!value)
				{
					_annotation = null; //free it up
				}
				else if (_annotation == null)
				{
					_annotation = new Annotation();
					_annotation.IsOn = true;
				}
			}
		}

		public virtual Annotatable Clone()
		{
			var clone = new Annotatable();
			clone._annotation = _annotation == null ? null : _annotation.Clone();
			return clone;
		}

		public override bool Equals(Object obj)
		{
			if (!(obj is Annotatable)) return false;
			return Equals((Annotatable)obj);
		}

		public bool Equals(Annotatable other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			if ((_annotation != null && !_annotation.Equals(other._annotation)) || (other._annotation != null && !other._annotation.Equals(_annotation))) return false;
			return true;
		}
	}

	/// <summary>
	/// An annotation is a like a "flag" on a field. You can say, e.g., "I'm not sure about this"
	/// </summary>
	public class Annotation : ICloneable<Annotation>, IEquatable<Annotation>
	{
		/// <summary>
		/// 0 means "off".  1 means "starred". In the future, other positive values could correspond to other icon.
		/// </summary>
		private int _status = 0;

		public Annotation()
		{
			IsOn = false;
		}

		public Annotation(Annotation annotation)
		{
			_status = annotation._status;
		}

		public Annotation Clone()
		{
			return new Annotation(this);
		}

		public bool IsOn
		{
			get
			{
				return _status != 0;
			}
			set
			{
				_status = value?1:0;
			}
		}

		public override bool Equals(Object obj)
		{
			if (!(obj is Annotation)) return false;
			return Equals((Annotation)obj);
		}

		public bool Equals(Annotation other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			if(!IsOn.Equals(other.IsOn)) return false;
			if (!_status.Equals(other._status)) return false;
			return true;
		}
	}
}