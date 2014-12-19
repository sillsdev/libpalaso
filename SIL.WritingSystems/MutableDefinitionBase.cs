using System;
using System.Collections.Generic;
using Palaso.Code;

namespace SIL.WritingSystems
{
	public abstract class MutableDefinitionBase<T> : ICloneable<T>
	{
		public virtual bool Modified { get; protected set; }

		public virtual void ResetModified()
		{
			Modified = false;
		}

		public abstract bool ValueEquals(T other);
		public abstract T Clone();

		protected bool UpdateString(ref string field, string value)
		{
			//count null as same as ""
			if (String.IsNullOrEmpty(field) && String.IsNullOrEmpty(value))
				return false;

			return UpdateField(ref field, value);
		}

		/// <summary>
		/// Updates the specified field and marks the writing system as modified.
		/// </summary>
		protected bool UpdateField<TField>(ref TField field, TField value)
		{
			if (EqualityComparer<TField>.Default.Equals(field, value))
				return false;

			Modified = true;
			field = value;
			return true;
		}
	}
}
