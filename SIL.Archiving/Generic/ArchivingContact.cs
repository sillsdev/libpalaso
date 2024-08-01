using System;
using System.Collections.Generic;

namespace SIL.Archiving.Generic
{
	/// <summary>Information about a contact</summary>
	public class ArchivingContact : IComparable
	{
		/// <summary />
		public string Name;
		/// <summary />
		public string Address;
		/// <summary />
		public string Email;
		/// <summary />
		public string OrganizationName;

		/// <summary>Compare 2 ArchivingContact objects. They are identical if they have the same Name</summary>
		public int CompareTo(object obj)
		{
			if (obj == null)
				return 1;

			ArchivingContact other = obj as ArchivingContact;

			if (other == null)
				throw new ArgumentException();

			return String.Compare(Name, other.Name, StringComparison.OrdinalIgnoreCase);
		}

		/// <summary>Compare 2 ArchivingContact objects. They are identical if they have the same Name</summary>
		public static int Compare(ArchivingContact contactA, ArchivingContact contactB)
		{
			return contactA.CompareTo(contactB);
		}
	}

	/// <summary>Compare 2 ArchivingContact objects. They are identical if they have the same Name</summary>
	public class ArchivingContactComparer : IEqualityComparer<ArchivingContact>
	{
		public bool Equals(ArchivingContact x, ArchivingContact y)
		{
			return (x.CompareTo(y) == 0);
		}

		public int GetHashCode(ArchivingContact obj)
		{
			return obj.Name.GetHashCode();
		}
	}

	/// <summary>Simplify creating and managing ArchivingContact collections</summary>
	public class ArchivingContactCollection : HashSet<ArchivingContact>
	{
		/// <summary>Default constructor</summary>
		public ArchivingContactCollection()
			: base(new ArchivingContactComparer())
		{
			// additional constructor code can go here
		}
	}

}
