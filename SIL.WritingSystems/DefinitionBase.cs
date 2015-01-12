using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Palaso.Code;

namespace SIL.WritingSystems
{
	public abstract class DefinitionBase<T> : IChangeTracking, ICloneable<T>
	{
		public virtual bool IsChanged { get; protected set; }

		public virtual void AcceptChanges()
		{
			IsChanged = false;
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

			IsChanged = true;
			field = value;
			return true;
		}

		protected static void ChildrenAcceptChanges(IEnumerable<IChangeTracking> children)
		{
			foreach (IChangeTracking child in children)
				child.AcceptChanges();
		}


		protected static bool ChildrenIsChanged(IEnumerable<IChangeTracking> children)
		{
			return children.Any(child => child.IsChanged);
		}
	}
}
