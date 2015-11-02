using System;
using System.IO;
using NMock2;
using SIL.Lift.Parsing;

namespace SIL.Lift.Tests.Parsing
{
	class ExtensibleMatcher:Matcher
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
		public override bool Matches(object o)
		{
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

		public override void DescribeTo(TextWriter writer)
		{
			writer.Write(string.Format("ExtensibleMatcher(expectedId={0}, expectedGuid={1}, expectedCreationTime={2}, expectedModificationTime={3})",
				_expectedId,
				(_expectedGuid==Guid.Empty)?"anything": _expectedGuid.ToString(),
				(_expectedCreationTime == DateTime.MinValue) ? "anything" : _expectedCreationTime.ToString(),
				(_expectedModificationTime == DateTime.MinValue) ? "anything" : _expectedModificationTime.ToString()));
		}
	}
}
