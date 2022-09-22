// Copyright (c) 2014 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Win32;
using SIL.PlatformUtilities;
using SIL.Reporting;

namespace SIL.IO
{
	public interface IFileLocator
	{
		string LocateFile(string fileName);
		string LocateFile(string fileName, string descriptionForErrorMessage);
		string LocateOptionalFile(string fileName);
		string LocateFileWithThrow(string fileName);
		string LocateDirectory(string directoryName);
		string LocateDirectoryWithThrow(string directoryName);
		string LocateDirectory(string directoryName, string descriptionForErrorMessage);
		IFileLocator CloneAndCustomize(IEnumerable<string> addedSearchPaths);
	}

	public interface IChangeableFileLocator : IFileLocator
	{
		void AddPath(string path);
		void RemovePath(string path);
	}

	public class FileLocator : IChangeableFileLocator
	{
		private readonly List<string> _searchPaths;

		public FileLocator(IEnumerable<string> searchPaths)
		{
			this._searchPaths = new List<string>(searchPaths);
		}

		public string LocateFile(string fileName)
		{
			foreach (var path in GetSearchPaths())
			{
				var fullPath = Path.Combine(path, fileName);
				if(File.Exists(fullPath))
					return fullPath;
			}
			return string.Empty;
		}

		/// <summary>
		/// Subclasses (e.g. in Bloom) override this to provide a dynamic set of paths
		/// </summary>
		/// <returns></returns>
		virtual protected IEnumerable<string> GetSearchPaths()
		{
			return _searchPaths;
		}

		public string LocateFile(string fileName, string descriptionForErrorMessage)
		{

			var path = LocateFile(fileName);
			if (string.IsNullOrEmpty(path) || !File.Exists(path))
			{
				ErrorReport.NotifyUserOfProblem(
					"{0} could not find the {1}.  It expected to find it in one of these locations: {2}",
					UsageReporter.AppNameToUseInDialogs, descriptionForErrorMessage, string.Join(", ", GetSearchPaths())
					);
			}
			return path;
		}

		public string LocateDirectory(string directoryName)
		{
			foreach (var path in GetSearchPaths())
			{
				var fullPath = Path.Combine(path, directoryName);
				if (Directory.Exists(fullPath))
					return fullPath;
			}
			return string.Empty;
		}
		public string LocateDirectory(string directoryName, string descriptionForErrorMessage)
		{

			var path = LocateDirectory(directoryName);
			if (string.IsNullOrEmpty(path) || !Directory.Exists(path))
			{
				ErrorReport.NotifyUserOfProblem(
					"{0} could not find the {1}.  It expected to find it in one of these locations: {2}",
					UsageReporter.AppNameToUseInDialogs, descriptionForErrorMessage, string.Join(", ", _searchPaths)
					);
			}
			return path;
		}
		public string LocateDirectoryWithThrow(string directoryName)
		{
			var path = LocateDirectory(directoryName);
			if (string.IsNullOrEmpty(path) || !Directory.Exists(path))
			{
				throw new ApplicationException(String.Format("Could not find {0}.  It expected to find it in one of these locations: {1}",
					directoryName, string.Join(Environment.NewLine, _searchPaths)));
			}
			return path;
		}

		/// <summary>
		///
		/// </summary>
		/// <param name="fileName"></param>
		/// <returns>null if not found</returns>
		public string LocateOptionalFile(string fileName)
		{
			var path = LocateFile(fileName);
			if (string.IsNullOrEmpty(path) || !File.Exists(path))
			{
				return null;
			}
			return path;
		}

		/// <summary>
		/// Throws ApplicationException if not found.
		/// </summary>
		public string LocateFileWithThrow(string fileName)
		{
			var path = LocateFile(fileName);
			if (string.IsNullOrEmpty(path) || !File.Exists(path))
			{
				throw new ApplicationException("Could not find " + fileName + ". It expected to find it in one of these locations: " + Environment.NewLine + string.Join(Environment.NewLine, _searchPaths));
			}
			return path;
		}

		protected List<string> SearchPaths
		{
			get { return _searchPaths; }
		}

		/// <summary>
		/// Use this when you can't mess with the whole application's filelocator, but you want to add a path or two, e.g., the folder of the current book in Bloom.
		/// </summary>
		/// <param name="addedSearchPaths"></param>
		/// <returns></returns>
		public virtual IFileLocator CloneAndCustomize(IEnumerable<string> addedSearchPaths)
		{
			return new FileLocator(new List<string>(_searchPaths.Concat(addedSearchPaths)));
		}

		#region Methods for locating file in program files folders
		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Searches the registry and returns the full path to the application program used to
		/// open files having the specified extention. The fileExtension can be with or without
		/// the preceding period. If the command cannot be found in the registry, then null is
		/// returned. If a command in the registry is found, but it refers to a program file
		/// that does not exist, null is returned.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public static string GetFromRegistryProgramThatOpensFileType(string fileExtension)
		{
			if (!Platform.IsWindows)
			{
				//------------------------------------------------------------------------------------
				// The following command will output the mime type of an existing file, Phil.html:
				//    file -b --mime-type ~/Phil.html
				//
				// This command will tell you the default application to open the file Phil.html:
				//    ext=$(grep "$(file -b --mime-type ~/Phil.html)" /etc/mime.types
				//        | awk '{print $1}') && xdg-mime query default $ext
				//
				// This command will open the file Phil.html using the default application:
				//    xdg-open ~/Page.html
				//------------------------------------------------------------------------------------

				throw new NotImplementedException(
					"GetFromRegistryProgramThatOpensFileType not implemented on Mono yet.");
			}

			var ext = fileExtension.Trim();
			if (!ext.StartsWith("."))
				ext = "." + ext;

			var key = Registry.ClassesRoot.OpenSubKey(ext);
			if (key == null)
				return null;

			var value = key.GetValue(string.Empty) as string;
			key.Dispose();

			if (value == null)
				return null;

			key = Registry.ClassesRoot.OpenSubKey(string.Format("{0}\\shell\\open\\command", value));

			if (key == null && value.ToLower() == "ramp.package")
			{
				key = Registry.ClassesRoot.OpenSubKey(string.Format("{0}\\shell\\open\\command", "ramp"));
				if (key == null)
					return null;
			}

			value = key?.GetValue(string.Empty) as string;
			key?.Dispose();

			if (value == null)
				return null;

			value = value.Trim('\"', '%', '1', ' ');
			return (!File.Exists(value) ? null : value);
		}

		#endregion

		public virtual void AddPath(string path)
		{
			_searchPaths.Add(path);
		}

		public void RemovePath(string path)
		{
			_searchPaths.Remove(path);
		}
	}
}
