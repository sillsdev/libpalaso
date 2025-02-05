// Copyright (c) 2007-2025 SIL Global
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.Xml.Serialization;
using SIL.ObjectModel;

namespace SIL.Annotations
{
	public class Annotatable : IAnnotatable, ICloneable<Annotatable>, IEquatable<Annotatable>
	{
		[XmlAttribute("starred")]
		public bool IsStarred
		{
			get
			{
				if (Annotation == null)
				{
					return false; // don't bother making one yet
				}
				return Annotation.IsOn;
			}
			set
			{
				if (!value)
				{
					Annotation = null; //free it up
				}
				else if (Annotation == null)
				{
					Annotation = new Annotation { IsOn = true };
				}
			}
		}

		protected Annotation Annotation { get; set; }

		public virtual Annotatable Clone()
		{
			var clone = new Annotatable { Annotation = Annotation?.Clone() };
			return clone;
		}

		public override bool Equals(object obj)
		{
			return Equals(obj as Annotatable);
		}

		public bool Equals(Annotatable other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			if ((Annotation != null && !Annotation.Equals(other.Annotation)) || (other.Annotation != null && !other.Annotation.Equals(Annotation))) return false;
			return true;
		}

		public override int GetHashCode()
		{
			// https://stackoverflow.com/a/263416/487503
			unchecked // Overflow is fine, just wrap
			{
				var hash = 37;
				hash *= 7 + IsStarred.GetHashCode();
				hash *= 7 + Annotation?.GetHashCode() ?? 0;
				return hash;
			}
		}
	}
}