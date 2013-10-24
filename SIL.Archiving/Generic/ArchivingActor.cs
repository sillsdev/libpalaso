
using System;
using System.Collections.Generic;
using Palaso.Extensions;

namespace SIL.Archiving.Generic
{
	class ArchivingActor
	{
		private string _primaryLanguageIso3Code;
		private string _birthDate;

		/// <summary>If needed but not given, FullName will be used</summary>
		public string Name;

		/// <summary>If needed but not given, Name will be used</summary>
		public string FullName;

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

		public ArchivingActor()
		{
			Iso3LanguageIds = new HashSet<string>();
		}

		public void SetBirthDate(string date)
		{
			_birthDate = date;
		}

		public void SetBirthDate(DateTime date)
		{
			_birthDate = date.ToISO8601DateOnlyString();
		}

		public string GetBirthDate()
		{
			return _birthDate;
		}

		public string Gender;
	}
}
