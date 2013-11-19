
using System;
using System.Collections.Generic;
using System.Linq;
using Palaso.Extensions;

namespace SIL.Archiving.Generic
{
	/// <summary>An Actor is someone who has contributed to the content as a speaker or writer</summary>
	public class ArchivingActor : IComparable
	{
		private string _primaryLanguageIso3Code;
		private string _motherTongueLanguageIso3Code;
		private string _birthDate;

		/// <summary>If needed but not given, FullName will be used</summary>
		public string Name;

		/// <summary>If needed but not given, Name will be used</summary>
		public string FullName;

		/// <summary></summary>
		public string Age;

		/// <summary>Languages this actor knows</summary>
		public HashSet<string> Iso3LanguageIds;

		/// <summary>The primary language for this actor</summary>
		public string PrimaryLanguageIso3Code
		{
			get { return _primaryLanguageIso3Code;  }
			set
			{
				_primaryLanguageIso3Code = value;
				Iso3LanguageIds.Add(_primaryLanguageIso3Code);
			}
		}

		/// <summary>The mother tongue language for this actor</summary>
		public string MotherTongueLanguageIso3Code
		{
			get { return _motherTongueLanguageIso3Code; }
			set
			{
				_motherTongueLanguageIso3Code = value;
				Iso3LanguageIds.Add(_motherTongueLanguageIso3Code);
			}
		}

		/// <summary>Files associated with this actor</summary>
		public List<IArchivingFile> Files;

		/// <summary>Default constructor</summary>
		public ArchivingActor()
		{
			Iso3LanguageIds = new HashSet<string>();
			Files = new List<IArchivingFile>();
		}

		/// <summary />
		public void SetBirthDate(string date)
		{
			_birthDate = date;
		}

		/// <summary />
		public void SetBirthDate(DateTime date)
		{
			_birthDate = date.ToISO8601DateOnlyString();
		}

		/// <summary />
		public string GetBirthDate()
		{
			return _birthDate;
		}

		/// <summary />
		public string Gender;

		/// <summary />
		public string GetName()
		{
			return Name ?? FullName;
		}

		/// <summary />
		public string GetFullName()
		{
			return FullName ?? Name;
		}

		/// <summary />
		public string Education;

		/// <summary />
		public string Occupation;

		/// <summary>Compare 2 ArchivingActor objects. They are identical if they have the same FullName</summary>
		public int CompareTo(object obj)
		{
			if (obj == null)
				return 1;

			ArchivingActor other = obj as ArchivingActor;

			if (other == null)
				throw new ArgumentException();

			return String.Compare(GetFullName(), other.GetFullName(), StringComparison.OrdinalIgnoreCase);
		}

		/// <summary>Compare 2 ArchivingActor objects. They are identical if they have the same FullName</summary>
		public static int Compare(ArchivingActor actorA, ArchivingActor actorB)
		{
			return actorA.CompareTo(actorB);
		}
	}

	/// <summary>Compare 2 ArchivingActor objects. They are identical if they have the same FullName</summary>
	public class ArchivingActorComparer : IEqualityComparer<ArchivingActor>
	{
		public bool Equals(ArchivingActor x, ArchivingActor y)
		{
			return (x.CompareTo(y) == 0);
		}

		public int GetHashCode(ArchivingActor obj)
		{
			return obj.Name.GetHashCode();
		}
	}

	/// <summary>Simplify creating and managing ArchivingActor collections</summary>
	public class ArchivingActorCollection : HashSet<ArchivingActor>
	{
		/// <summary>Default constructor</summary>
		public ArchivingActorCollection()
			: base(new ArchivingActorComparer())
		{
			// additional constructor code can go here
		}

		/// <summary>Count of all Actor files</summary>
		public int FileCount
		{
			get { return this.Sum(actor => actor.Files.Count); }
		}
	}
}
