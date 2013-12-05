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
		List<string> Files { get; }

		/// <summary>Set session date with DateTime object</summary>
		void SetDate(DateTime date);

		/// <summary>Set session date with a string. Can be a date range</summary>
		void SetDate(string date);

		/// <summary>Set session date with just the year</summary>
		void SetDate(int year);

		/// <summary></summary>
		void AddActor(ArchivingActor actor);

		/// <summary></summary>
		void AddKeyValuePair(string key, string value);

		/// <summary></summary>
		string Genre { get; set; }
	}
}
