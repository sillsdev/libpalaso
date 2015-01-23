using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Palaso.Code;

namespace SIL.WritingSystems
{
	public abstract class DefinitionBase<T> : IChangeTracking, ICloneable<T>, INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		public virtual bool IsChanged { get; protected set; }

		public virtual void AcceptChanges()
		{
			IsChanged = false;
		}

		public abstract bool ValueEquals(T other);
		public abstract T Clone();

		protected bool UpdateString(Expression<Func<string>> propertyExpression, ref string field, string value)
		{
			//count null as same as ""
			if (String.IsNullOrEmpty(field) && String.IsNullOrEmpty(value))
				return false;

			return UpdateField(propertyExpression, ref field, value);
		}

		/// <summary>
		/// Updates the specified field and marks the writing system as modified.
		/// </summary>
		protected bool UpdateField<TField>(Expression<Func<TField>> propertyExpression, ref TField field, TField value)
		{
			if (EqualityComparer<TField>.Default.Equals(field, value))
				return false;

			IsChanged = true;
			field = value;
			OnPropertyChanged(new PropertyChangedEventArgs(GetPropertyName(propertyExpression)));
			return true;
		}

		protected static string GetPropertyName<TField>(Expression<Func<TField>> propertyExpression)
		{
			if (propertyExpression == null)
				throw new ArgumentNullException("propertyExpression");

			var body = propertyExpression.Body as MemberExpression;

			if (body == null)
				throw new ArgumentException("Invalid argument", "propertyExpression");

			var property = body.Member as PropertyInfo;

			if (property == null)
				throw new ArgumentException("Argument is not a property", "propertyExpression");

			return property.Name;
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

		protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, e);
		}
	}
}
