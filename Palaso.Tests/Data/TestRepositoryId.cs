using System.Collections.Generic;
using Palaso.Data;

namespace Palaso.Tests.Data
{
	internal class TestRepositoryId: RepositoryId
	{
		private readonly int id;

		public TestRepositoryId(int id)
		{
			this.id = id;
		}

		public override int CompareTo(RepositoryId other)
		{
			var otherAsMy = other as TestRepositoryId;
			if (otherAsMy == null)
			{
				return 1;
			}
			return Comparer<int>.Default.Compare(id, otherAsMy.id);
		}

		public override bool Equals(RepositoryId other)
		{
			var otherAsMy = other as TestRepositoryId;
			if (otherAsMy == null)
			{
				return false;
			}
			return id == otherAsMy.id;
		}

		public override string ToString()
		{
			return id.ToString();
		}
	}
}