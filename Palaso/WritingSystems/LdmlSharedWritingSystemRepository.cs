using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Palaso.WritingSystems;

namespace Palaso.WritingSystems
{
	public sealed class LdmlSharedWritingSystemRepository : LdmlInFolderWritingSystemRepository
	{
		static readonly LdmlSharedWritingSystemRepository _instance = new LdmlSharedWritingSystemRepository();

		public static IWritingSystemRepository Singleton
		{
			get
			{
				return _instance;
			}
		}

		// Explicit static constructor to tell C# compiler
		// not to mark type as before fieldinit
		static LdmlSharedWritingSystemRepository()
		{
		}

		/// <summary>
		/// Use the default repository
		/// </summary>
		private LdmlSharedWritingSystemRepository()
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