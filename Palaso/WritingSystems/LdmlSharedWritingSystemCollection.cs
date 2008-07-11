using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Palaso.WritingSystems;

namespace Palaso.WritingSystems
{
	public sealed class LdmlSharedWritingSystemCollection : LdmlInFolderWritingSystemStore
	{
		static readonly LdmlSharedWritingSystemCollection _instance = new LdmlSharedWritingSystemCollection();

		public static IWritingSystemStore Singleton
		{
			get
			{
				return _instance;
			}
		}

		// Explicit static constructor to tell C# compiler
		// not to mark type as before fieldinit
		static LdmlSharedWritingSystemCollection()
		{
		}

		/// <summary>
		/// Use the default repository
		/// </summary>
		private LdmlSharedWritingSystemCollection()
		{
			string p = Path.Combine(
				Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SIL"
			);
			Directory.CreateDirectory(p);
			p = Path.Combine(p, "WritingSystemStore");
			Directory.CreateDirectory(p);
			PathToWritingSystems = p;
		}

	}
}