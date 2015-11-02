using System;

namespace SIL.ObjectModel
{
	public abstract class Freezable<T> : IFreezable, IValueEquatable<T>
	{
		private int _hashCode;

		public bool IsFrozen { get; private set; }

		public void Freeze()
		{
			if (IsFrozen)
				return;

			IsFrozen = true;
			_hashCode = FreezeImpl();
		}

		public abstract bool ValueEquals(T other);

		protected abstract int FreezeImpl();

		protected void CheckFrozen()
		{
			if (IsFrozen)
				throw new InvalidOperationException("This object is immutable.");
		}

		public int GetFrozenHashCode()
		{
			if (!IsFrozen)
				throw new InvalidOperationException("This object does not have a valid hash code, because it is mutable.");
			return _hashCode;
		}
	}
}
