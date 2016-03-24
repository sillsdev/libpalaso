using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace SIL.ObjectModel
{
	public static class UnorderedTuple
	{
		public static UnorderedTuple<T1> Create<T1>(T1 item1)
		{
			return new UnorderedTuple<T1>(item1);
		}

		public static UnorderedTuple<T1, T2> Create<T1, T2>(T1 item1, T2 item2)
		{
			return new UnorderedTuple<T1, T2>(item1, item2);
		}

		public static UnorderedTuple<T1, T2, T3> Create<T1, T2, T3>(T1 item1, T2 item2, T3 item3)
		{
			return new UnorderedTuple<T1, T2, T3>(item1, item2, item3);
		}

		public static UnorderedTuple<T1, T2, T3, T4> Create<T1, T2, T3, T4>(T1 item1, T2 item2, T3 item3, T4 item4)
		{
			return new UnorderedTuple<T1, T2, T3, T4>(item1, item2, item3, item4);
		}

		public static UnorderedTuple<T1, T2, T3, T4, T5> Create<T1, T2, T3, T4, T5>(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5)
		{
			return new UnorderedTuple<T1, T2, T3, T4, T5>(item1, item2, item3, item4, item5);
		}
	}

	public class UnorderedTuple<T1> : IStructuralEquatable
	{
		private readonly T1 _item1;

		public UnorderedTuple(T1 item1)
		{
			_item1 = item1;
		}

		public T1 Item1
		{
			get { return _item1; }
		}

		public override bool Equals(object obj)
		{
			return ((IStructuralEquatable) this).Equals(obj, EqualityComparer<object>.Default);
		}

		public override int GetHashCode()
		{
			return ((IStructuralEquatable) this).GetHashCode(EqualityComparer<object>.Default);
		}

		bool IStructuralEquatable.Equals(object other, IEqualityComparer comparer)
		{
			if (other == null)
				return false;
			var tuple = other as UnorderedTuple<T1>;
			return tuple != null && comparer.Equals(_item1, tuple._item1);
		}

		int IStructuralEquatable.GetHashCode(IEqualityComparer comparer)
		{
			return comparer.GetHashCode(_item1);
		}

		public override string ToString()
		{
			var sb = new StringBuilder();
			sb.Append("(");
			sb.Append(_item1);
			sb.Append(")");
			return sb.ToString();
		}
	}

	public class UnorderedTuple<T1, T2> : IStructuralEquatable
	{
		private readonly T1 _item1;
		private readonly T2 _item2;

		public UnorderedTuple(T1 item1, T2 item2)
		{
			_item1 = item1;
			_item2 = item2;
		}

		public T1 Item1
		{
			get { return _item1; }
		}

		public T2 Item2
		{
			get { return _item2; }
		}

		public override bool Equals(object obj)
		{
			return ((IStructuralEquatable) this).Equals(obj, EqualityComparer<object>.Default);
		}

		public override int GetHashCode()
		{
			return ((IStructuralEquatable) this).GetHashCode(EqualityComparer<object>.Default);
		}

		bool IStructuralEquatable.Equals(object other, IEqualityComparer comparer)
		{
			if (other == null)
				return false;
			var tuple = other as UnorderedTuple<T1, T2>;
			return tuple != null && ((comparer.Equals(_item1, tuple._item1) && comparer.Equals(_item2, tuple._item2))
				|| (comparer.Equals(_item1, tuple._item2) && comparer.Equals(_item2, tuple._item1)));
		}

		int IStructuralEquatable.GetHashCode(IEqualityComparer comparer)
		{
			return comparer.GetHashCode(_item1) ^ comparer.GetHashCode(_item2);
		}

		public override string ToString()
		{
			var sb = new StringBuilder();
			sb.Append("(");
			sb.Append(_item1);
			sb.Append(",");
			sb.Append(_item2);
			sb.Append(")");
			return sb.ToString();
		}
	}

	public class UnorderedTuple<T1, T2, T3> : IStructuralEquatable
	{
		private readonly T1 _item1;
		private readonly T2 _item2;
		private readonly T3 _item3;

		public UnorderedTuple(T1 item1, T2 item2, T3 item3)
		{
			_item1 = item1;
			_item2 = item2;
			_item3 = item3;
		}

		public T1 Item1
		{
			get { return _item1; }
		}

		public T2 Item2
		{
			get { return _item2; }
		}

		public T3 Item3
		{
			get { return _item3; }
		}

		public override bool Equals(object obj)
		{
			return ((IStructuralEquatable) this).Equals(obj, EqualityComparer<object>.Default);
		}

		public override int GetHashCode()
		{
			return ((IStructuralEquatable) this).GetHashCode(EqualityComparer<object>.Default);
		}

		bool IStructuralEquatable.Equals(object other, IEqualityComparer comparer)
		{
			if (other == null)
				return false;
			var tuple = other as UnorderedTuple<T1, T2, T3>;
			var wrapper = new WrapperEqualityComparer<object>(comparer);
			return tuple != null && new HashSet<object>(wrapper) {_item1, _item2, _item3}.SetEquals(new HashSet<object>(wrapper) {tuple._item1, tuple._item2, tuple._item3});
		}

		int IStructuralEquatable.GetHashCode(IEqualityComparer comparer)
		{
			return comparer.GetHashCode(_item1) ^ comparer.GetHashCode(_item2) ^ comparer.GetHashCode(_item3);
		}

		public override string ToString()
		{
			var sb = new StringBuilder();
			sb.Append("(");
			sb.Append(_item1);
			sb.Append(",");
			sb.Append(_item2);
			sb.Append(",");
			sb.Append(_item3);
			sb.Append(")");
			return sb.ToString();
		}
	}

	public class UnorderedTuple<T1, T2, T3, T4> : IStructuralEquatable
	{
		private readonly T1 _item1;
		private readonly T2 _item2;
		private readonly T3 _item3;
		private readonly T4 _item4;

		public UnorderedTuple(T1 item1, T2 item2, T3 item3, T4 item4)
		{
			_item1 = item1;
			_item2 = item2;
			_item3 = item3;
			_item4 = item4;
		}

		public T1 Item1
		{
			get { return _item1; }
		}

		public T2 Item2
		{
			get { return _item2; }
		}

		public T3 Item3
		{
			get { return _item3; }
		}

		public T4 Item4
		{
			get { return _item4; }
		}

		public override bool Equals(object obj)
		{
			return ((IStructuralEquatable) this).Equals(obj, EqualityComparer<object>.Default);
		}

		public override int GetHashCode()
		{
			return ((IStructuralEquatable) this).GetHashCode(EqualityComparer<object>.Default);
		}

		bool IStructuralEquatable.Equals(object other, IEqualityComparer comparer)
		{
			if (other == null)
				return false;
			var tuple = other as UnorderedTuple<T1, T2, T3, T4>;
			var wrapper = new WrapperEqualityComparer<object>(comparer);
			return tuple != null && new HashSet<object>(wrapper) {_item1, _item2, _item3, _item4}.SetEquals(new HashSet<object>(wrapper) {tuple._item1, tuple._item2, tuple._item3, tuple._item4});
		}

		int IStructuralEquatable.GetHashCode(IEqualityComparer comparer)
		{
			return comparer.GetHashCode(_item1) ^ comparer.GetHashCode(_item2) ^ comparer.GetHashCode(_item3) ^ comparer.GetHashCode(_item4);
		}

		public override string ToString()
		{
			var sb = new StringBuilder();
			sb.Append("(");
			sb.Append(_item1);
			sb.Append(",");
			sb.Append(_item2);
			sb.Append(",");
			sb.Append(_item3);
			sb.Append(",");
			sb.Append(_item4);
			sb.Append(")");
			return sb.ToString();
		}
	}

	public class UnorderedTuple<T1, T2, T3, T4, T5> : IStructuralEquatable
	{
		private readonly T1 _item1;
		private readonly T2 _item2;
		private readonly T3 _item3;
		private readonly T4 _item4;
		private readonly T5 _item5;

		public UnorderedTuple(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5)
		{
			_item1 = item1;
			_item2 = item2;
			_item3 = item3;
			_item4 = item4;
			_item5 = item5;
		}

		public T1 Item1
		{
			get { return _item1; }
		}

		public T2 Item2
		{
			get { return _item2; }
		}

		public T3 Item3
		{
			get { return _item3; }
		}

		public T4 Item4
		{
			get { return _item4; }
		}

		public T5 Item5
		{
			get { return _item5; }
		}

		public override bool Equals(object obj)
		{
			return ((IStructuralEquatable) this).Equals(obj, EqualityComparer<object>.Default);
		}

		public override int GetHashCode()
		{
			return ((IStructuralEquatable) this).GetHashCode(EqualityComparer<object>.Default);
		}

		bool IStructuralEquatable.Equals(object other, IEqualityComparer comparer)
		{
			if (other == null)
				return false;
			var tuple = other as UnorderedTuple<T1, T2, T3, T4, T5>;
			var wrapper = new WrapperEqualityComparer<object>(comparer);
			return tuple != null && new HashSet<object>(wrapper) {_item1, _item2, _item3, _item4, _item5}.SetEquals(new HashSet<object>(wrapper) {tuple._item1, tuple._item2, tuple._item3, tuple._item4, tuple.Item5});
		}

		int IStructuralEquatable.GetHashCode(IEqualityComparer comparer)
		{
			return comparer.GetHashCode(_item1) ^ comparer.GetHashCode(_item2) ^ comparer.GetHashCode(_item3) ^ comparer.GetHashCode(_item4) ^ comparer.GetHashCode(_item5);
		}

		public override string ToString()
		{
			var sb = new StringBuilder();
			sb.Append("(");
			sb.Append(_item1);
			sb.Append(",");
			sb.Append(_item2);
			sb.Append(",");
			sb.Append(_item3);
			sb.Append(",");
			sb.Append(_item4);
			sb.Append(",");
			sb.Append(_item5);
			sb.Append(")");
			return sb.ToString();
		}
	}
}
