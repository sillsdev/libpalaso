using System.Collections;
using Microsoft.Build.Framework;

namespace BuildTaskTests
{
	public class MockTaskItem : ITaskItem
	{

		public MockTaskItem(string itemSpec)
		{
			ItemSpec = itemSpec;
		}

		public string GetMetadata(string metadataName)
		{
			return "";
		}

		public void SetMetadata(string metadataName, string metadataValue)
		{
		}

		public void RemoveMetadata(string metadataName)
		{
		}

		public void CopyMetadataTo(ITaskItem destinationItem)
		{
		}

		public IDictionary CloneCustomMetadata()
		{
			return null;
		}

		public string ItemSpec { get; set; }

		public ICollection MetadataNames
		{
			get { return null; }
		}

		public int MetadataCount
		{
			get { return 0; }
		}
	}
}