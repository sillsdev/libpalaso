using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Xml;

namespace SIL.Archiving.IMDI.Lists
{
	/// <summary>
	///
	/// </summary>
	public class ListConstructor
	{
		private static string _listPath;
		private static readonly Dictionary<string, IMDIItemList> _loadedLists = new Dictionary<string, IMDIItemList>();

		/// <summary>
		/// Returns a list of IMDIListItems that can also be used as the data source of a combo or list box
		/// </summary>
		/// <param name="listName">Name of the XML file that contains the desired list. It is suggested to
		/// use values from IMDI_Schema.ListTypes class. If not found on the local system, we will attempt
		/// to download from http://www.mpi.nl/IMDI/Schema.
		/// </param>
		/// <returns>List of IMDIListItems</returns>
		public static IMDIItemList GetList(string listName)
		{
			listName = CleanListName(listName);

			if (!_loadedLists.ContainsKey(listName))
				_loadedLists.Add(listName, new IMDIItemList(GetNodeList(listName)));

			return _loadedLists[listName];
		}

		/// <summary>
		/// Gets a list of the Entry nodes from the selected XML file.
		/// </summary>
		/// <param name="listName">Name of the XML file that contains the desired list. It is suggested to
		/// use values from IMDI_Schema.ListTypes class. If not found on the local system, we will attempt
		/// to download from http://www.mpi.nl/IMDI/Schema.
		/// </param>
		/// <returns></returns>
		public static XmlNodeList GetNodeList(string listName)
		{
			listName = CleanListName(listName);

			var listFileName = CheckFile(listName);

			// if the file was not found, thrwo an exception
			if (string.IsNullOrEmpty(listFileName))
				throw new FileNotFoundException(string.Format("The list {0} was not found.", listName));

			XmlDocument doc = new XmlDocument();
			doc.Load(listFileName);

			var nsmgr = new XmlNamespaceManager(doc.NameTable);
			nsmgr.AddNamespace("imdi", "http://www.mpi.nl/IMDI/Schema/IMDI");

			// if not a valid XML file, throw an exception
			if (doc.DocumentElement == null)
				throw new XmlException(string.Format("The file {0} was not a valid XML file.", listFileName));

			var nodes = doc.DocumentElement.SelectNodes("//imdi:VocabularyDef/imdi:Entry", nsmgr);

			// if no entries were found, throw an exception
			if (nodes == null)
				throw new XmlException(string.Format("The file {0} does not contain any list entries.", listFileName));

			return nodes;
		}

		private static string ListPath
		{
			get
			{
				if (!string.IsNullOrEmpty(_listPath)) return _listPath;

				var thisPath = CheckFolder(Path.Combine(IMDIDataFolder, "lists"));

				// check if path exists
				if (!Directory.Exists(thisPath))
					throw new DirectoryNotFoundException("Not able to find the IMDI lists directory.");

				_listPath = thisPath;

				return _listPath;
			}
		}

		private static string SilCommonDataFolder
		{
			get { return CheckFolder(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "SIL")); }
		}

		private static string IMDIDataFolder
		{
			get { return CheckFolder(Path.Combine(SilCommonDataFolder, "IMDI")); }
		}

		private static string CheckFolder(string folderName)
		{
			if (!Directory.Exists(folderName)) Directory.CreateDirectory(folderName);
			return folderName;
		}

		private static string CheckFile(string listName)
		{
			var listFileName = Path.Combine(ListPath, listName);

			// if file already exists locally, return it now
			if (File.Exists(listFileName))
				return listFileName;

			// attempt to download if not already in list folder
			var url = "http://www.mpi.nl/IMDI/Schema/" + listName;
			Debug.WriteLine("Downloading from {0} to {1}", url, listFileName);
			var wc = new WebClient();
			wc.DownloadFile(url, listFileName);

			// return full name, or null if not able to download
			return File.Exists(listFileName) ? listFileName : null;
		}

		private static string CleanListName(string listName)
		{
			if (!listName.EndsWith(".xml"))
				listName += ".xml"; // make sure the name has .xml extension
			return listName;
		}
	}
}
