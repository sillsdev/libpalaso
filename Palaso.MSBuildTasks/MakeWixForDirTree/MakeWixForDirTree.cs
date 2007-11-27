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
		public string OutputFileName
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

			LogMessage(MessageImportance.Low, "Walking " + _rootDir);
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

			// Set up the exclusion lookup table if necessary
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
				ProcessDir(elemDirRef, Path.GetFullPath(_rootDir), "Lib");//review (jdh): what does this "lib" do and what should it really be?

				// write out components into a group
				XmlElement elemGroup = doc.CreateElement("ComponentGroup", XMLNS);
				elemGroup.SetAttribute("Id", _componentGroupId);
				elemFrag.AppendChild(elemGroup);

				foreach (string c in m_components)
				{
					XmlElement elem = doc.CreateElement("ComponentRef", XMLNS);
					elem.SetAttribute("Id", c);
					elemGroup.AppendChild(elem);
				}

				// write the XML out onlystringles have been modified
				if (!m_checkOnly && m_filesChanged)
				{
					XmlWriterSettings settings = new XmlWriterSettings();
					settings.Indent = true;
					settings.IndentChars = "    ";
					settings.Encoding = Encoding.UTF8;
					using (XmlWriter xmlWriter = XmlWriter.Create(_outputFilePath, settings))
					{
						doc.WriteTo(xmlWriter);
					}
				}
			}
			catch (IOException e)
			{
				LogErrorFromException(e);
				return false;
			}

			return !HasLoggedErrors;
		}

		public void LogMessage(MessageImportance messageImportance, string s)
		{
			Log.LogMessage(messageImportance, s);
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



		private void ProcessDir(XmlElement parent, string dirName, string compPrefix)
		{
			Log.LogMessage(MessageImportance.Low, "Processing dir {0}", dirName);

			XmlDocument doc = parent.OwnerDocument;
			List<string> files = new List<string>();
			IdToGuidDatabase metadata = IdToGuidDatabase.Create(Path.Combine(dirName, ".guidsForInstaller.xml"), this); ;
			// Build a list of the files in this directory removing any that have been exluded
			foreach (string f in Directory.GetFiles(dirName))
			{
				if (_fileMatchPattern.IsMatch(f) && !m_exclude.ContainsKey(f.ToLower()))
					files.Add(f);
			}

			// Process all files
			bool isFirst = true;
			foreach (string path in files)
			{
				ProcessFile(parent, path, doc, metadata, isFirst);
				isFirst = false;
			}

			// Recursively process any subdirectories
			foreach (string d in Directory.GetDirectories(dirName))
			{
				string shortName = Path.GetFileName(d);
				if (!m_exclude.ContainsKey(d.ToLower()) && shortName != ".svn" && shortName != "CVS")
				{
					string id = compPrefix + "." + shortName;
					id = Regex.Replace(id, @"[^\p{Lu}\p{Ll}\p{Nd}._]", "_");

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

		private void ProcessFile(XmlElement parent, string path, XmlDocument doc, IdToGuidDatabase guidDatabase, bool isFirst)
		{
			string guid;
			string name = Path.GetFileName(path);
			string id = name;
			if (!Char.IsLetter(id[0]) && id[0] != '_')
				id = '_' + id;
			id = Regex.Replace(id, @"[^\p{Lu}\p{Ll}\p{Nd}._]", "_");

			Log.LogMessage(MessageImportance.Low, "Adding file {0} with id {1}", path, id);
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
			elemFile.SetAttribute("Source", path);
			elemComp.AppendChild(elemFile);

			m_components.Add(id);

			// check whether the file is newer
			if (File.GetLastWriteTime(path) > m_refDate)
				m_filesChanged = true;
		}

		#endregion
	}
}