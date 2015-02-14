using System;
using System.Collections.Specialized;
using System.IO;
using System.Linq;

namespace SIL.WindowsForms.PortableSettingsProvider
{
	/// ----------------------------------------------------------------------------------------
	/// <summary>
	/// Encapsulates a class to manage the list of most recently used project paths.
	/// </summary>
	/// ----------------------------------------------------------------------------------------
	public static class MruFiles
	{
		private static StringCollection s_paths = new StringCollection();
		public static int MaxMRUListSize { get; private set; }

		/// ------------------------------------------------------------------------------------
		/// <summary>
		///
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public static StringCollection Initialize(StringCollection paths)
		{
			return Initialize(paths, 9);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		///
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public static StringCollection Initialize(StringCollection paths, int maxMruListSize)
		{
			MaxMRUListSize = maxMruListSize;
			s_paths = (paths ?? new StringCollection());
			RemoveStalePaths();

			while (s_paths.Count > MaxMRUListSize)
				s_paths.RemoveAt(s_paths.Count - 1);

			return s_paths;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets the path to the most recently used (i.e. opened) project.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public static string Latest
		{
			get { return (s_paths.Count == 0 ? null : s_paths[0]); }
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets the list of most recently used files as a string array.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public static string[] Paths
		{
			get { return s_paths.Cast<string>().ToArray(); }
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Removes non existant paths from the MRU list.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		private static void RemoveStalePaths()
		{
			if (s_paths.Count > 0)
			{
				for (int i = s_paths.Count - 1; i >= 0; i--)
				{
					if (!File.Exists(s_paths[i]))
						s_paths.RemoveAt(i);
				}
			}
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Adds the specified file path to top of list of most recently used files if it
		/// exists (returns false if it doesn't exist)
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public static bool AddNewPath(string path)
		{
			return AddNewPath(path, false);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Adds the specified file path to top of list of most recently used files if it
		/// exists (returns false if it doesn't exist)
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public static bool AddNewPath(string path, bool addToEnd)
		{
			if (path == null)
				throw new ArgumentNullException("path");

			if (!File.Exists(path))
				return false;

			// Remove the path from the list if it exists already.
			s_paths.Remove(path);

			// Make sure inserting a new path at the beginning will not exceed our max.
			if (s_paths.Count >= MaxMRUListSize)
				s_paths.RemoveAt(s_paths.Count - 1);

			if (addToEnd)
				s_paths.Add(path);
			else
				s_paths.Insert(0, path);

			return true;
		}
	}
}