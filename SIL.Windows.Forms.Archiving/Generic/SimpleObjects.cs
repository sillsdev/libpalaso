
namespace SIL.Windows.Forms.Archiving.Generic
{
	/// <summary>Location information for the data in the package</summary>
	public class ArchivingLocation
	{
		/// <summary />
		public string Continent;
		/// <summary />
		public string Country;
		/// <summary />
		public string Region;
		/// <summary />
		public string Address;
	}


	/// <summary>Information on the funding for this project</summary>
	public class ArchivingProject
	{
		/// <summary>Name of the funding project</summary>
		public string Name;

		/// <summary>Title of the funding project</summary>
		public string Title;

		/// <summary>Author of the funding project</summary>
		public string Author;

		/// <summary />
		public ArchivingContactCollection Contacts;

		/// <summary />
		public ArchivingProject()
		{
			Contacts = new ArchivingContactCollection();
		}
	}
}
