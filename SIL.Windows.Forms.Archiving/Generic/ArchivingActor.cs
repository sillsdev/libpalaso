using System;
using System.Collections.Generic;
using System.Linq;
using SIL.Extensions;

namespace SIL.Windows.Forms.Archiving.Generic
{
	/// <summary>An Actor is someone who has contributed to the content as a speaker or writer</summary>
	public class ArchivingActor : IComparable
	{
		private ArchivingLanguage _primaryLanguage;
		private ArchivingLanguage _motherTongueLanguage;
		private string _birthDate;

		/// <summary>If needed but not given, FullName will be used</summary>
		public string Name;

		/// <summary>If needed but not given, Name will be used</summary>
		public string FullName;

		///  <summary/>
		public string Code;

		/// <summary/>
		public string FamilySocialRole;

		/// <summary/>
		public string EthnicGroup;

		/// <summary />
		public string Age;

		/// <summary>Languages this actor knows</summary>
		public ArchivingLanguageCollection Iso3Languages;

		/// <summary>The primary language for this actor</summary>
		public ArchivingLanguage PrimaryLanguage
		{
			get { return _primaryLanguage;  }
			set
			{
				_primaryLanguage = value;
				Iso3Languages.Add(_primaryLanguage);
			}
		}

		/// <summary>The mother tongue language for this actor</summary>
		public ArchivingLanguage MotherTongueLanguage
		{
			get { return _motherTongueLanguage; }
			set
			{
				_motherTongueLanguage = value;
				Iso3Languages.Add(_motherTongueLanguage);
			}
		}

		/// <summary>Files associated with this actor</summary>
		public List<ArchivingFile> Files;

		/// <summary></summary>
		public List<KeyValuePair<string, string>> Keys;

		/// <summary />
		public bool Anonymize = false;

		/// <summary>Default constructor</summary>
		public ArchivingActor()
		{
			Iso3Languages = new ArchivingLanguageCollection();
			Files = new List<ArchivingFile>();
			Keys = new List<KeyValuePair<string, string>>();
		}

		/// <summary>Value can be either DateTime (birth date), int (birth year), or string</summary>
		public object BirthDate
		{
			set
			{
				if (value is DateTime)
					_birthDate = ((DateTime) value).ToISO8601TimeFormatDateOnlyString();
				else
					_birthDate = value.ToString();
			}
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

		/// <summary />
		public string Role;

		/// <summary />
		public void AddKeyValuePair(string key, string value)
		{
			Keys.Add(new KeyValuePair<string, string>(key, value));
		}

		/// <summary>Compare 2 ArchivingActor objects. They are identical if they have the same FullName</summary>
		public int CompareTo(object obj)
		{
			if (obj == null)
				return 1;

			ArchivingActor other = obj as ArchivingActor;

			if (other == null)
				throw new ArgumentException();

			var fullNameCompare = String.Compare(GetFullName(), other.GetFullName(), StringComparison.OrdinalIgnoreCase);
			var nameCompare = String.Compare(GetName(), other.GetName(), StringComparison.OrdinalIgnoreCase);

			return nameCompare == 0 ? 0 : fullNameCompare;
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
