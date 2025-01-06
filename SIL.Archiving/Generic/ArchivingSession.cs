using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace SIL.Archiving.Generic
{
	/// <summary>Contains the materials being archived, and their metadata</summary>
	public interface IArchivingSession : IArchivingGenericObject
	{
		/// <summary></summary>
		void AddFile(ArchivingFile file);

		/// <summary></summary>
		[PublicAPI]
		void AddFileAccess(string fullFileName, ArchivingPackage package);

		/// <summary>The paths of all resource files in the session.</summary>
		IReadOnlyList<string> Files { get; }

		/// <summary>Set session date with DateTime object</summary>
		void SetDate(DateTime date);

		/// <summary>Set session date with a string. Can be a date range</summary>
		void SetDate(string date);

		/// <summary>Set session date with just the year</summary>
		void SetDate(int year);

		/// <summary></summary>
		[PublicAPI]
		void AddContentLanguage(ArchivingLanguage language, LanguageString description);

		/// <summary></summary>
		[PublicAPI]
		void AddActor(ArchivingActor actor);

		/// <summary></summary>
		[PublicAPI]
		void AddGroupKeyValuePair(string key, string value);

		/// <summary></summary>
		[PublicAPI]
		void AddContentKeyValuePair(string key, string value);

		/// <summary></summary>
		[PublicAPI]
		void AddFileKeyValuePair(string fullFileName, string key, string value);

		/// <summary></summary>
		[PublicAPI]
		void AddContentDescription(LanguageString description);

		/// <summary></summary>
		[PublicAPI]
		void AddActorDescription(ArchivingActor actor, LanguageString description);

		/// <summary></summary>
		[PublicAPI]
		void AddFileDescription(string fullFileName, LanguageString description);

		/// <summary></summary>
		[PublicAPI]
		void AddActorContact(ArchivingActor actor, ArchivingContact contact);

		/// <summary></summary>
		[PublicAPI]
		void AddMediaFileTimes(string fullFileName, string start, string stop);

		/// <summary></summary>
		[PublicAPI]
		void AddProject(ArchivingPackage package);

		/// <summary></summary>
		string Genre { get; set; }

		/// <summary></summary>
		string SubGenre { get; set; }

		/// <summary></summary>
		string Interactivity { get; set; }

		/// <summary>Indicates in how far the researcher was involved in the linguistic event</summary>
		[PublicAPI]
		string Involvement { get; set; }

		/// <summary>Degree of planning of the event</summary>
		string PlanningType { get; set; }

		/// <summary></summary>
		string SocialContext { get; set; }

		/// <summary></summary>
		string Task { get; set; }
	}
}
