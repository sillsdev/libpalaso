using System;

namespace Palaso.Data
{
	public abstract class RepositoryId: IComparable<RepositoryId>, IEquatable<RepositoryId>
	{
		private static readonly RepositoryId _emptyRepositoryId = new EmptyRepositoryId();

		public static RepositoryId Empty
		{
			get { return _emptyRepositoryId; }
		}

		private class EmptyRepositoryId: RepositoryId
		{
			public override int CompareTo(RepositoryId other)
			{
				if (other == null)
				{
					return 1;
				}
				if (other is EmptyRepositoryId)
				{
					return 0;
				}
				return -1;
			}

			public override bool Equals(RepositoryId other)
			{
				return ReferenceEquals(this, other);
			}
		}

		public static bool operator !=(RepositoryId repositoryId1, RepositoryId repositoryId2)
		{
			return !Equals(repositoryId1, repositoryId2);
		}

		public static bool operator ==(RepositoryId repositoryId1, RepositoryId repositoryId2)
		{
			return Equals(repositoryId1, repositoryId2);
		}

		public abstract int CompareTo(RepositoryId other);
		public abstract bool Equals(RepositoryId other);

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(this, obj))
			{
				return true;
			}
			return Equals(obj as RepositoryId);
		}

		public override int GetHashCode()
		{
			return 0;
		}
	}
}