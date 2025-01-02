using System;
using SIL.Lift.Parsing;

namespace SIL.Lift.Tests.Parsing
{
	class ExtensibleMatcher : Extensible
	{
		private readonly string _expectedId;
		private readonly Guid _expectedGuid;
		private readonly DateTime _expectedCreationTime;
		private readonly DateTime _expectedModificationTime;

		public ExtensibleMatcher(string expectedId, Guid expectedGuid, DateTime expectedCreationTime, DateTime expectedModificationTime)
		{
			this._expectedId = expectedId;
			this._expectedModificationTime = expectedModificationTime;
			this._expectedCreationTime = expectedCreationTime;
			this._expectedGuid = expectedGuid;
		}

		public ExtensibleMatcher(string expectedId, DateTime expectedCreationTime, DateTime expectedModificationTime)
			:this(expectedId, Guid.Empty, expectedCreationTime, expectedModificationTime)
		{
		}

		public ExtensibleMatcher(string expectedId, Guid expectedGuid)
			: this(expectedId, expectedGuid, DateTime.MinValue, DateTime.MinValue)
		{
		}

		public ExtensibleMatcher(Guid expectedGuid)
			: this(string.Empty, expectedGuid)
		{
		}


		public ExtensibleMatcher(string expectedId)
			:this(expectedId, Guid.Empty)
		{
		}

		public ExtensibleMatcher()
			: this(string.Empty)
		{
		}

		public override bool Equals(object o)
		{
			if (o == null)
			{
				throw new NullReferenceException();
			}

			Extensible e = (Extensible) o;

			if (e.Id != _expectedId)
			{
				return false;
			}

			if (_expectedGuid != Guid.Empty
				&& e.Guid != _expectedGuid)
			{
				return false;
			}

			if (_expectedCreationTime != DateTime.MinValue
				&& e.CreationTime != _expectedCreationTime)
			{
				return false;
			}

			if (_expectedModificationTime != DateTime.MinValue
				&& e.ModificationTime != _expectedModificationTime)
			{
				return false;
			}
			return true;
		}

		public override int GetHashCode()
		{
			unchecked
			{
				var hashCode = (_expectedId != null ? _expectedId.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ _expectedGuid.GetHashCode();
				hashCode = (hashCode * 397) ^ _expectedCreationTime.GetHashCode();
				hashCode = (hashCode * 397) ^ _expectedModificationTime.GetHashCode();
				return hashCode;
			}
		}
	}
}
