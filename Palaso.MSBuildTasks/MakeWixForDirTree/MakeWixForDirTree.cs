/*
 * A custom task that walks a directory tree and creates a WiX fragment containing
 * components to recreate it in an installer.
 *
 * From John Hall <john.hall@xjtag.com>, originally named "PackageTree" and posted on the wix-users mailing list
 *
 * John Hatton modified a bit to make it more general and started cleaning it up.
 *
 * Places a "".guidsForInstaller.xml" in each directory.  THIS SHOULD BE CHECKED INTO VERSION CONTROL.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using ILogger=Palaso.BuildTasks.MakeWixForDirTree.ILogger;


namespace Palaso.BuildTasks.MakeWixForDirTree
{
	public class MakeWixForDirTree : Task, ILogger
	{

		static public string kFileNameOfGuidDatabase = ".guidsForInstaller.xml";

	#region Private data

		private string _rootDir;
		private string _outputFilePath;
		private string[] _filesAndDirsToExclude = null;
		private Regex _fileMatchPattern = new Regex(@".*");

		//todo: this should just be a list
		private Dictionary<string, string> m_exclude = new Dictionary<string, string>();

		private bool m_checkOnly = false;

		private List<string> m_components = new List<string>();
		private Dictionary<string, int> m_suffixes = new Dictionary<string, int>();
		private DateTime m_refDate = DateTime.MinValue;
		private bool m_filesChanged = false;
		private bool _hasLoggedErrors=false;
		private string _directoryReferenceId;
		private string _componentGroupId;
		private bool _giveAllPermissions;

		private const string XMLNS = "http://schemas.microsoft.com/wix/2006/wi";

		#endregion


		#region Public members


		[Required]
		public string RootDirectory
		{
			get { return _rootDir; }
			set { _rootDir = value; }
		}


		/// <summary>
		/// Subfolders and files to exclude.
		/// </summary>
		public string[] Exclude
		{
			get { return _filesAndDirsToExclude; }
			set { _filesAndDirsToExclude = value; }
		}

		/// <summary>
		/// Allow normal non-administrators to write and delete the files
		/// </summary>
		public bool GiveAllPermissions
		{
			get { return _giveAllPermissions; }
			set { _giveAllPermissions = value; }
		}


		/*
		 * Regex pattern to match files. Defaults to .*
		 */
		public string MatchRegExPattern
		{
			get { return _fileMatchPattern.ToString(); }
			set { _fileMatchPattern = new Regex(value, RegexOptions.IgnoreCase); }
		}

		/// <summary>
		/// Whether to just check that all the metadata is uptodate or not. If this is true then no file is output.
		/// </summary>
		public bool CheckOnly
		{
			get { return m_checkOnly; }
			set { m_checkOnly = value; }
		}




		[Output, Required]
		public string OutputFilePath
		{
			get { return _outputFilePath; }
			set { _outputFilePath = value; }
		}

		public override bool Execute()
		{

			if (!Directory.Exists(_rootDir))
			{
				LogError("Directory not found: " + _rootDir);
				return false;
			}

			LogMessage(MessageImportance.High, "Creating Wix fragment for " + _rootDir);
			//make it an absolute path
			_outputFilePath = Path.GetFullPath(_outputFilePath);

			/* hatton removed this... it would leave deleted files referenced in the wxs file
			 if (File.Exists(_outputFilePath))
			{
				DateTime curFileDate = File.GetLastWriteTime(_outputFilePath);
				m_refDate = curFileDate;

				// if this assembly has been modified since the existing file was created then
				// force the output to be updated
				Assembly thisAssembly = Assembly.GetExecutingAssembly();
				DateTime assemblyTime = File.GetLastWriteTime(thisAssembly.Location);
				if (assemblyTime > curFileDate)
					m_filesChanged = true;
			}
			*/
			//instead, start afresh every time.

			if(File.Exists(_outputFilePath))
			{
				File.Delete(_outputFilePath);
			}

			SetupExclusions();

			try
			{
				XmlDocument doc = new XmlDocument();
				XmlElement elemWix = doc.CreateElement("Wix", XMLNS);
				doc.AppendChild(elemWix);

				XmlElement elemFrag = doc.CreateElement("Fragment", XMLNS);
				elemWix.AppendChild(elemFrag);

				XmlElement elemDirRef = doc.CreateElement("DirectoryRef", XMLNS);
				elemDirRef.SetAttribute("Id", DirectoryReferenceId);
				elemFrag.AppendChild(elemDirRef);

				// recurse through the tree add elements
				ProcessDir(elemDirRef, Path.GetFullPath(_rootDir), DirectoryReferenceId);

				// write out components into a group
				XmlElement elemGroup = doc.CreateElement("ComponentGroup", XMLNS);
				elemGroup.SetAttribute("Id", _componentGroupId);
				elemFrag.AppendChild(elemGroup);

				AddComponentRefsToDom(doc, elemGroup);

				WriteDomToFile(doc);
			}
			catch (IOException e)
			{
				LogErrorFromException(e);
				return false;
			}

			return !HasLoggedErrors;
		}

		private void WriteDomToFile(XmlDocument doc)
		{
// write the XML out onlystringles have been modified
			if (!m_checkOnly && m_filesChanged)
			{
				var settings = new XmlWriterSettings();
				settings.Indent = true;
				settings.IndentChars = "    ";
				settings.Encoding = Encoding.UTF8;
				using (var xmlWriter = XmlWriter.Create(_outputFilePath, settings))
				{
					doc.WriteTo(xmlWriter);
				}
			}
		}

		private void AddComponentRefsToDom(XmlDocument doc, XmlElement elemGroup)
		{
			foreach (string c in m_components)
			{
				XmlElement elem = doc.CreateElement("ComponentRef", XMLNS);
				elem.SetAttribute("Id", c);
				elemGroup.AppendChild(elem);
			}
		}

		private void SetupExclusions()
		{
			if (_filesAndDirsToExclude != null)
			{
				foreach (string s in _filesAndDirsToExclude)
				{
					string key;
					if (Path.IsPathRooted(s))
						key = s.ToLower();
					else
						key = Path.GetFullPath(Path.Combine(_rootDir, s)).ToLower();
					m_exclude.Add(key, s);
				}
			}
		}

		public bool HasLoggedErrors
		{
			get { return _hasLoggedErrors; }
		}

		/// <summary>
		///   will show up as: DirectoryRef Id="this property"
		/// </summary>
		public string DirectoryReferenceId
		{
			get { return _directoryReferenceId; }
			set { _directoryReferenceId = value; }
		}

		public string ComponentGroupId
		{
			get { return _componentGroupId; }
			set { _componentGroupId = value; }
		}

		public void LogErrorFromException(Exception e)
		{
			_hasLoggedErrors = true;
			Log.LogErrorFromException(e);
		}


		public void LogError(string s)
		{
			_hasLoggedErrors = true;
			Log.LogError(s);
		}


		public void LogWarning(string s)
		{
			Log.LogWarning(s);
		}

		private void ProcessDir(XmlElement parent, string dirPath, string outerDirectoryId)
		{
			LogMessage(MessageImportance.Low, "Processing dir {0}", dirPath);

			XmlDocument doc = parent.OwnerDocument;
			List<string> files = new List<string>();

			IdToGuidDatabase guidDatabase = IdToGuidDatabase.Create(Path.Combine(dirPath, kFileNameOfGuidDatabase), this); ;

			SetupDirectoryPermissions(dirPath, parent, outerDirectoryId, doc, guidDatabase);

			// Build a list of the files in this directory removing any that have been exluded
			foreach (string f in Directory.GetFiles(dirPath))
			{
				if (_fileMatchPattern.IsMatch(f) && !m_exclude.ContainsKey(f.ToLower())
					&& !f.Contains(kFileNameOfGuidDatabase) )
					files.Add(f);
			}

			// Process all files
			bool isFirst = true;
			foreach (string path in files)
			{
				ProcessFile(parent, path, doc, guidDatabase, isFirst);
				isFirst = false;
			}

			// Recursively process any subdirectories
			foreach (string d in Directory.GetDirectories(dirPath))
			{
				string shortName = Path.GetFileName(d);
				if (!m_exclude.ContainsKey(d.ToLower()) && shortName != ".svn" && shortName != "CVS")
				{
					string id = GetSafeDirectoryId(d, outerDirectoryId);

					XmlElement elemDir = doc.CreateElement("Directory", XMLNS);
					elemDir.SetAttribute("Id", id);
					elemDir.SetAttribute("Name", shortName);
					parent.AppendChild(elemDir);

					ProcessDir(elemDir, d, id);

					if (elemDir.ChildNodes.Count == 0)
						parent.RemoveChild(elemDir);
				}
			}
		}

		private void SetupDirectoryPermissions(string dirPath, XmlElement parent, string parentDirectoryId, XmlDocument doc, IdToGuidDatabase guidDatabase)
		{
			if (_giveAllPermissions)
			{
				/*	Need to add one of these in order to set the permissions on the directory
					 * <Component Id="biatahCacheDir" Guid="492F2725-9DF9-46B1-9ACE-E84E70AFEE99">
							<CreateFolder Directory="biatahCacheDir">
								<Permission GenericAll="yes" User="Everyone" />
							</CreateFolder>
						</Component>
					 */

				string id = GetSafeDirectoryId(string.Empty, parentDirectoryId);

				XmlElement componentElement = doc.CreateElement("Component", XMLNS);
				componentElement.SetAttribute("Id", id);
				componentElement.SetAttribute("Guid", guidDatabase.GetGuid(id, this.CheckOnly));

				XmlElement createFolderElement = doc.CreateElement("CreateFolder", XMLNS);
				createFolderElement.SetAttribute("Directory", id);
				AddPermissionElement(doc, createFolderElement);

				componentElement.AppendChild(createFolderElement);
				parent.AppendChild(componentElement);

				m_components.Add(id);
			}
		}

		private string GetSafeDirectoryId(string directoryPath, string parentDirectoryId)
		{
			string id = parentDirectoryId;
			//bit of a hack... we don't want our id to have this prefix.dir form fo the top level,
			//where it is going to be referenced by other wix files, that will just be expecting the id
			//the msbuild target gave for the id of this directory

			//I don't have it quite right, though. See the test file, where you get
			// <Component Id="common.bin.bin" (the last bin is undesirable)

			if (Path.GetFullPath(_rootDir) != directoryPath)
			{
				id+="." + Path.GetFileName(directoryPath);
				id = id.TrimEnd('.'); //for the case where directoryPath is intentionally empty
			}
			id = Regex.Replace(id, @"[^\p{Lu}\p{Ll}\p{Nd}._]", "_");
			return id;
		}

		private void ProcessFile(XmlElement parent, string path, XmlDocument doc, IdToGuidDatabase guidDatabase, bool isFirst)
		{

			string guid;
			string name = Path.GetFileName(path);
			string id = name;
			if (!Char.IsLetter(id[0]) && id[0] != '_')
				id = '_' + id;
			id = Regex.Replace(id, @"[^\p{Lu}\p{Ll}\p{Nd}._]", "_");

			LogMessage(MessageImportance.Normal, "Adding file {0} with id {1}", path, id);
			string key = id.ToLower();
			if (m_suffixes.ContainsKey(key))
			{
				int suffix = m_suffixes[key] + 1;
				m_suffixes[key] = suffix;
				id += suffix.ToString();
			}
			else
			{
				m_suffixes[key] = 0;
			}

			// Create <Component> and <File> for this file
			XmlElement elemComp = doc.CreateElement("Component", XMLNS);
			elemComp.SetAttribute("Id", id);
			guid = guidDatabase.GetGuid(id,this.CheckOnly);
			if (guid == null)
				m_filesChanged = true;        // this file is new
			else
				elemComp.SetAttribute("Guid", guid.ToUpper());
			parent.AppendChild(elemComp);

			XmlElement elemFile = doc.CreateElement("File", XMLNS);
			elemFile.SetAttribute("Id", id);
			elemFile.SetAttribute("Name", name);
			if (isFirst)
			{
				elemFile.SetAttribute("KeyPath", "yes");
			}
			string relativePath = PathUtil.RelativePathTo(Path.GetDirectoryName(_outputFilePath), path);
			elemFile.SetAttribute("Source", relativePath);

			if (GiveAllPermissions)
			{
				AddPermissionElement(doc, elemFile);
			}


			elemComp.AppendChild(elemFile);

			m_components.Add(id);

			// check whether the file is newer
			if (File.GetLastWriteTime(path) > m_refDate)
				m_filesChanged = true;
		}

		private void AddPermissionElement(XmlDocument doc, XmlElement elementToAddPermissionTo)
		{
			XmlElement persmission = doc.CreateElement("Permission", XMLNS);
			persmission.SetAttribute("GenericAll", "yes");
			persmission.SetAttribute("User", "Everyone");
			elementToAddPermissionTo.AppendChild(persmission);
		}

		private void LogMessage(string message, params object[] args)
		{
			LogMessage(MessageImportance.Normal, message, args);
		}

		public void LogMessage(MessageImportance importance, string message)
		{
			try
			{
				Log.LogMessage(importance.ToString(), message);
			}
			catch (InvalidOperationException)
			{
				// Swallow exceptions for testing
			}
		}

		private void LogMessage(MessageImportance importance, string message, params object[] args)
		{
			try
			{
				Log.LogMessage(importance.ToString(), message, args);
			}
			catch (InvalidOperationException)
			{
				// Swallow exceptions for testing
			}
		}

		private void LogError(string message, params object[] args)
		{
			try
			{
				Log.LogError(message, args);
			}
			catch (InvalidOperationException)
			{
				// Swallow exceptions for testing
			}
		}



		#endregion
	}
}