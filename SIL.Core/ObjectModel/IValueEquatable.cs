using System;

namespace SIL.ObjectModel
{

	/// <summary>
	/// This interface is used to define value equality for a mutable type.
	/// 
	/// Typically, <see cref="IEquatable{T}"/> should be used to define value equality.
	/// Unfortunately, the semantics of <see cref="IEquatable{T}"/> does not allow it to 
	/// be used for mutable types. If <see cref="IEquatable{T}"/> is implemented by a
	/// class, it should also implement the <see cref="object.GetHashCode()"/> method to
	/// match. For a type whose value equality depends on a mutable field, this could result
	/// in the hash code changing throughout the life of the object, which can cause incorrect
	/// behavior when used with some collections. This interface does not have the same
	/// semantics, and can be used without requiring that <see cref="object.GetHashCode()"/> is
	/// implemented to match.
	/// 
	/// This interface is typically used to define value equality for freezable types.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public interface IValueEquatable<in T>
	{
		bool ValueEquals(T other);
	}
}
