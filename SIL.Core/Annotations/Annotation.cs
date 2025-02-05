// Copyright (c) 2007-2025 SIL Global
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using SIL.ObjectModel;

namespace SIL.Annotations
{
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
			get => _status != 0;
			set => _status = value?1:0;
		}

		public override bool Equals(object obj)
		{
			return Equals(obj as Annotation);
		}

		public bool Equals(Annotation other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			if(!IsOn.Equals(other.IsOn)) return false;
			if (!_status.Equals(other._status)) return false;
			return true;
		}

		public override int GetHashCode()
		{
			// https://stackoverflow.com/a/263416/487503
			unchecked // Overflow is fine, just wrap
			{
				var hash = 41;
				hash *= 61 + _status.GetHashCode();
				return hash;
			}
		}
	}
}