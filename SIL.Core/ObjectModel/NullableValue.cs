using System.Collections;

namespace SIL.ObjectModel
{
	public struct NullableValue<T> : IStructuralEquatable
	{
		private T _value;
		private bool _hasValue;

		public NullableValue(T value)
		{
			_value = value;
			_hasValue = true;
		}

		public bool HasValue
		{
			get { return _hasValue; }
			set
			{
				_hasValue = value;
				if (!_hasValue)
					_value = default(T);
			}
		}

		public T Value
		{
			get { return _value; }
			set
			{
				_value = value;
				_hasValue = true;
			}
		}

		public override string ToString()
		{
			if (_hasValue)
				return _value.ToString();
			return "null";
		}

		bool IStructuralEquatable.Equals(object other, IEqualityComparer comparer)
		{
			if (other == null || !(other is NullableValue<T>))
				return false;

			var otherVal = (NullableValue<T>) other;
			if (!_hasValue && !otherVal._hasValue)
				return true;

			return _hasValue && otherVal._hasValue && comparer.Equals(_value, otherVal._value);
		}

		int IStructuralEquatable.GetHashCode(IEqualityComparer comparer)
		{
			if (_hasValue)
				return comparer.GetHashCode(_value);
			return 0;
		}
	}
}
