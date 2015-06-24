﻿using System;
using System.Linq;

namespace SIL.WritingSystems
{
	/// <summary>
	/// This class represents a subtag from the IANA language subtag registry.
	/// </summary>
	public abstract class Subtag
	{
		private readonly string _code;
		private readonly string _name;
		private readonly bool _isPrivateUse;
		private readonly int _hashCode;

		protected Subtag(string code, bool isPrivateUse)
			: this(code, null, isPrivateUse)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Subtag"/> class.
		/// </summary>
		/// <param name="code">The code.</param>
		/// <param name="name">The name.</param>
		/// <param name="isPrivateUse">if set to <c>true</c> this is a private use subtag.</param>
		protected Subtag(string code, string name, bool isPrivateUse)
		{
			if (code == null)
				throw new ArgumentNullException("code");
			if (code.Any(c => !IsValidChar(c)))
				throw new ArgumentException("The code contains invalid characters.", "code");

			_code = code;
			_name = name;
			_isPrivateUse = isPrivateUse;

			_hashCode = 23;
			_hashCode = _hashCode * 31 + _code.ToLowerInvariant().GetHashCode();
			_hashCode = _hashCode * 31 + Name.GetHashCode();
			_hashCode = _hashCode * 31 + _isPrivateUse.GetHashCode();
		}

		private static bool IsValidChar(char c)
		{
			return (c >= 'a' && c <= 'z')
				|| (c >= 'A' && c <= 'Z')
				|| (c >= '0' && c <= '9');
		}

		/// <summary>
		/// Gets the code.
		/// </summary>
		/// <value>The code.</value>
		public string Code
		{
			get { return _code; }
		}

		/// <summary>
		/// Gets the name.
		/// </summary>
		/// <value>The name.</value>
		public string Name
		{
			get { return _name ?? string.Empty; }
		}

		/// <summary>
		/// Gets or sets a value indicating whether this instance is private use.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this instance is private use; otherwise, <c>false</c>.
		/// </value>
		public bool IsPrivateUse
		{
			get { return _isPrivateUse; }
		}

		/// <summary>
		/// Determines whether the specified <see cref="T:System.Object"/> is equal to this instance.
		/// </summary>
		/// <param name="obj">The <see cref="T:System.Object"/> to compare with this instance.</param>
		/// <returns>
		/// 	<c>true</c> if the specified <see cref="T:System.Object"/> is equal to this instance; otherwise, <c>false</c>.
		/// </returns>
		/// <exception cref="T:System.NullReferenceException">
		/// The <paramref name="obj"/> parameter is null.
		/// </exception>
		public override bool Equals(object obj)
		{
			return Equals(obj as Subtag);
		}

		/// <summary>
		/// Determines whether the specified <see cref="T:Subtag"/> is equal to this instance.
		/// </summary>
		/// <param name="other">The other.</param>
		/// <returns></returns>
		public bool Equals(Subtag other)
		{
			return other != null && other._code.Equals(_code, StringComparison.InvariantCultureIgnoreCase) && other.Name == Name && other._isPrivateUse == _isPrivateUse;
		}

		/// <summary>
		/// Returns a hash code for this instance.
		/// </summary>
		/// <returns>
		/// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
		/// </returns>
		public override int GetHashCode()
		{
			return _hashCode;
		}

		/// <summary>
		/// Returns a <see cref="T:System.String"/> that represents this instance.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.String"/> that represents this instance.
		/// </returns>
		public override string ToString()
		{
			if (!string.IsNullOrEmpty(_name))
				return _name;
			return _code;
		}

		/// <summary>
		/// Implements the operator ==.
		/// </summary>
		/// <param name="x">The x.</param>
		/// <param name="y">The y.</param>
		/// <returns>The result of the operator.</returns>
		public static bool operator ==(Subtag x, Subtag y)
		{
			if (ReferenceEquals(x, y))
				return true;
			if ((object) x == null || (object) y == null)
				return false;
			return x.Equals(y);
		}

		/// <summary>
		/// Implements the operator !=.
		/// </summary>
		/// <param name="x">The x.</param>
		/// <param name="y">The y.</param>
		/// <returns>The result of the operator.</returns>
		public static bool operator !=(Subtag x, Subtag y)
		{
			return !(x == y);
		}

		public static implicit operator string(Subtag subtag)
		{
			return subtag == null ? null : subtag._code;
		}
	}
}
