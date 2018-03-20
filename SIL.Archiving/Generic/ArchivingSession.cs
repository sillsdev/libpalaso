using System;
using System.Collections.Generic;

namespace SIL.Archiving.Generic
{
	/// <summary>Contains the materials being archived, and their metadata</summary>
	public interface IArchivingSession : IArchivingGenericObject
	{
		/// <summary></summary>
		void AddFile(ArchivingFile file);

		/// <summary></summary>
		void AddFileAccess(string fullFileName, ArchivingPackage package, LanguageString conditions, LanguageString restrictions);

		/// <summary></summary>
		List<string> Files { get; }

		/// <summary>Set session date with DateTime object</summary>
		void SetDate(DateTime date);

		/// <summary>Set session date with a string. Can be a date range</summary>
		void SetDate(string date);

		/// <summary>Set session date with just the year</summary>
		void SetDate(int year);

		/// <summary></summary>
		void AddContentLanguage(ArchivingLanguage language, LanguageString description);

		/// <summary></summary>
		void AddActor(ArchivingActor actor);

		/// <summary></summary>
		void AddGroupKeyValuePair(string key, string value);

		/// <summary></summary>
		void AddContentKeyValuePair(string key, string value);

		/// <summary></summary>
		void AddFileKeyValuePair(string fullFileName, string key, string value);

		/// <summary></summary>
		void AddContentDescription(LanguageString description);

		/// <summary></summary>
		void AddActorDescription(ArchivingActor actor, LanguageString description);

		/// <summary></summary>
		void AddFileDescription(string fullFileName, LanguageString description);

		/// <summary></summary>
		void AddActorContact(ArchivingActor actor, ArchivingContact contact);

		/// <summary></summary>
		void AddMediaFileTimes(string fullFileName, string start, string stop);

		/// <summary></summary>
		void AddProject(ArchivingPackage package);

		/// <summary></summary>
		string Genre { get; set; }

		/// <summary></summary>
		string SubGenre { get; set; }

		/// <summary></summary>
		string Interactivity { get; set; }

		/// <summary></summary>
		string Involvement { get; set; }

		/// <summary></summary>
		string PlanningType { get; set; }

		/// <summary></summary>
		string SocialContext { get; set; }

		/// <summary></summary>
		string Task { get; set; }
	}
}
