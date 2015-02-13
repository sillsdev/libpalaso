using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using SIL.ObjectModel;

namespace SIL.WritingSystems
{
	public abstract class DefinitionBase<T> : ObservableObject, IChangeTracking, ICloneable<T>, IValueEquatable<T>
	{
		public virtual bool IsChanged { get; protected set; }

		public virtual void AcceptChanges()
		{
			IsChanged = false;
		}

		public abstract bool ValueEquals(T other);
		public abstract T Clone();

		protected bool Set(Expression<Func<string>> propertyExpression, ref string field, string value)
		{
			//count null as same as ""
			if (String.IsNullOrEmpty(field) && String.IsNullOrEmpty(value))
				return false;

			return base.Set(propertyExpression, ref field, value);
		}

		protected override void OnPropertyChanged(PropertyChangedEventArgs e)
		{
			base.OnPropertyChanged(e);
			IsChanged = true;
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
