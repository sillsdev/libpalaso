using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Xml;
using SIL.Lift.Validation;
using SIL.Xml;

namespace SIL.Lift.Merging
{
	/// <summary>
	/// Class to merge two or more LIFT files that are created incrementally, such that
	/// 1) the data in previous ones is overwritten by data in newer ones
	/// 2) the *entire contents* of the new entry element replaces the previous contents
	/// I.e., merging is only on an entry level.
	/// </summary>
	public class SynchronicMerger
	{
		//  private string _pathToBaseLiftFile;
		///<summary></summary>
		public const  string ExtensionOfIncrementalFiles = ".lift.update";

		/// <summary>
		///
		/// </summary>
		/// <exception cref="IOException">If file is locked</exception>
		/// <exception cref="LiftFormatException">If there is an error and then file is found to be non-conformant.</exception>
		/// <param name="pathToBaseLiftFile"></param>
		public bool MergeUpdatesIntoFile(string pathToBaseLiftFile)
		{
			// _pathToBaseLiftFile = pathToBaseLiftFile;

			FileInfo[] files = GetPendingUpdateFiles(pathToBaseLiftFile);
			return MergeUpdatesIntoFile(pathToBaseLiftFile, files);
		}

		/// <summary>
		/// Given a LIFT file and a set of .lift.update files, apply the changes to the LIFT file and delete the .lift.update files.
		/// </summary>
		/// <param name="pathToBaseLiftFile">The LIFT file containing all the lexical entries for a paticular language project</param>
		/// <param name="files">These files contain the changes the user has made. Changes to entries; New entries; Deleted entries</param>
		/// <returns></returns>
		/// <exception cref="IOException">If file is locked</exception>
		/// <exception cref="LiftFormatException">If there is an error and then file is found to be non-conformant.</exception>
		///
		public bool MergeUpdatesIntoFile(string pathToBaseLiftFile, FileInfo[] files)
		{
			if (files.Length < 1)
			{
				return false;
			}
			Array.Sort(files, new FileInfoLastWriteTimeComparer());
			int count = files.Length;

			string pathToMergeInTo = pathToBaseLiftFile; // files[0].FullName;

			FileAttributes fa = File.GetAttributes(pathToBaseLiftFile);
			if ((fa & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
			{
				return false;
			}

			for (int i = 0; i < count; i++)
			{
				if (files[i].IsReadOnly)
				{
					//todo: "Cannot merge safely because at least one file is read only: {0}
					return true;
				}
			}
			bool mergedAtLeastOne = false;

			List<string> filesToDelete = new List<string>();
			for (int i = 0; i < count; i++)
			{
				//let empty files be as if they didn't exist.  They do represent that something
				//went wrong, but (at least in WeSay) we can detect that something by a more
				//accurate means, and these just get in the way
				if (files[i].Length < 100) //sometimes empty files register as having a few bytes
				{
					string contents = File.ReadAllText(files[i].FullName);
					if (contents.Trim().Length == 0)
					{
						File.Delete(files[i].FullName);
						continue;
					}
				}

				// We will want to call LiftSorter.SortLiftFile() with this temporary
				// filename, and it thus MUST end with ".lift".
				string outputPath = Path.GetTempFileName() + ".lift";
				try
				{
					MergeInNewFile(pathToMergeInTo, files[i].FullName, outputPath);
				}
				catch (IOException)
				{
					// todo: "Cannot most likely one of the files is locked
					File.Delete(outputPath);
					throw;
				}
				catch (Exception)
				{
					try
					{
						Validator.CheckLiftWithPossibleThrow(files[i].FullName);
					}
					catch (Exception e2)
					{
						File.Delete(outputPath);
						throw new BadUpdateFileException(pathToMergeInTo, files[i].FullName, e2);
					}
					//eventually we'll just check everything before-hand.  But for now our rng
					//validator is painfully slow in files which have date stamps,
					//because two formats are allowed an our mono rng validator
					//throws non-fatal exceptions for each one
					Validator.CheckLiftWithPossibleThrow(pathToBaseLiftFile);
					throw; //must have been something else
				}
				pathToMergeInTo = outputPath;
				filesToDelete.Add(outputPath);

				mergedAtLeastOne = true;
			}

			if (!mergedAtLeastOne)
			{
				return false;
			}

			//string pathToBaseLiftFile = Path.Combine(directory, BaseLiftFileName);
			Debug.Assert(File.Exists(pathToMergeInTo));

			MakeBackup(pathToBaseLiftFile, pathToMergeInTo);

			//delete all the non-base paths
			foreach (FileInfo file in files)
			{
				if (file.FullName != pathToBaseLiftFile && File.Exists(file.FullName))
				{
					file.Delete();
				}
			}

			//delete all our temporary files
			foreach (string s in filesToDelete)
			{
				File.Delete(s);
			}
			return true;
		}

		private static void MakeBackup(string pathToBaseLiftFile, string pathToMergeInTo)
		{
// File.Move works across volumes but the destination cannot exist.
			if (File.Exists(pathToBaseLiftFile))
			{
				string backupOfBackup;
				do
				{
					backupOfBackup = pathToBaseLiftFile + Path.GetRandomFileName();
				}
				while(File.Exists(backupOfBackup));


				string bakPath = pathToBaseLiftFile+".bak";
				if (File.Exists(bakPath))
				{
					// move the backup out of the way, if something fails here we have nothing to do
					if ((File.GetAttributes(bakPath) & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
					{
						bakPath = GetNextAvailableBakPath(bakPath);
					}
					else
					{
						try
						{
							File.Move(bakPath, backupOfBackup);
						}
						catch (IOException)
						{
							// back file is Locked. Create the next available backup Path
							bakPath = GetNextAvailableBakPath(bakPath);
						}
					}
				}

				try
				{
					File.Move(pathToBaseLiftFile, bakPath);

					try
					{
						File.Move(pathToMergeInTo, pathToBaseLiftFile);
					}
					catch
					{
						// roll back to prior state
						File.Move(bakPath, pathToBaseLiftFile);
						throw;
					}
				}
				catch
				{
					// roll back to prior state
					if (File.Exists(backupOfBackup))
					{
						File.Move(backupOfBackup, bakPath);
					}
					throw;
				}

				//everything was successful so can get rid of backupOfBackup
				if (File.Exists(backupOfBackup))
				{
					File.Delete(backupOfBackup);
				}
			}
			else
			{
				File.Move(pathToMergeInTo, pathToBaseLiftFile);
			}
		}

		///<summary>
		///</summary>
		///<exception cref="ArgumentException"></exception>
		public static FileInfo[] GetPendingUpdateFiles(string pathToBaseLiftFile)
		{
			//see ws-1035
		   if(!pathToBaseLiftFile.Contains(Path.DirectorySeparatorChar.ToString()))
		   {
			   throw new ArgumentException("pathToBaseLiftFile must be a full path, not just a file name. Path was "+pathToBaseLiftFile);
		   }
			// ReSharper disable AssignNullToNotNullAttribute
			var di = new DirectoryInfo(Path.GetDirectoryName(pathToBaseLiftFile));
			// ReSharper restore AssignNullToNotNullAttribute
			var files = di.GetFiles("*"+ExtensionOfIncrementalFiles, SearchOption.TopDirectoryOnly);
			//files comes back unsorted, sort by creation time before returning.
			Array.Sort(files, new Comparison<FileInfo>((a, b) => a.CreationTime.CompareTo(b.CreationTime)));
			return files;
		}

		static private void TestWriting(XmlWriter w)
		{
			//  w.WriteStartDocument();
			w.WriteStartElement("start");
			w.WriteElementString("one", "hello");
			w.WriteElementString("two", "bye");
			w.WriteEndElement();
			//   w.WriteEndDocument();
		}

		///<summary></summary>
		static public void TestWritingFile()
		{
			XmlWriterSettings settings = new XmlWriterSettings();
			settings.Indent = true;
			// nb:  don't use XmlTextWriter.Create, that's broken. Ignores the indent setting
			using (XmlWriter writer = XmlWriter.Create("C:\\test.xml", settings))
			{
				TestWriting(writer);
			}
		}

		private static string GetNextAvailableBakPath(string bakPath) {
			int i = 0;
			string newBakPath;
			do
			{
				i++;
				newBakPath = bakPath + i;
			}
			while (File.Exists(newBakPath));
			bakPath = newBakPath;
			return bakPath;
		}

		private void MergeInNewFile(string olderFilePath, string newerFilePath, string outputPath)
		{

			XmlDocument newerDoc = new XmlDocument();
			newerDoc.Load(newerFilePath);
			XmlWriterSettings settings = new XmlWriterSettings();
			settings.NewLineOnAttributes = true; //ugly, but great for merging with revision control systems
			settings.NewLineChars = "\r\n";
			settings.Indent = true;
			settings.IndentChars = "\t";
			settings.CheckCharacters = false;

			// nb:  don't use XmlTextWriter.Create, that's broken. Ignores the indent setting
			using (XmlWriter writer = XmlWriter.Create(outputPath /*Console.Out*/, settings))
			{
				//For each entry in the new guy, read through the whole base file
				XmlReaderSettings readerSettings = new XmlReaderSettings();
				readerSettings.CheckCharacters = false;//without this, we die if we simply encounter a (properly escaped) other-wise illegal unicode character, e.g. &#x1F;
				readerSettings.IgnoreWhitespace = true; //if the reader returns whitespace, the writer ceases to format xml after that point
				using (XmlReader olderReader = XmlReader.Create(olderFilePath, readerSettings))
				{
					//bool elementWasReplaced = false;
					while (!olderReader.EOF)
					{
						ProcessOlderNode(olderReader, newerDoc, writer);
					}
				}
			}
			// After writing the updated file, ensure that it ends up sorted correctly.
			LiftSorter.SortLiftFile(outputPath);
		}


		private void ProcessOlderNode(XmlReader olderReader, XmlDocument newerDoc, XmlWriter writer)
		{
			switch (olderReader.NodeType)
			{
				case XmlNodeType.EndElement:
				case XmlNodeType.Element:
					ProcessElement(olderReader, writer, newerDoc);
					break;
				default:
					Utilities.WriteShallowNode(olderReader, writer);
					break;
			}
		}

		private void ProcessElement(XmlReader olderReader, XmlWriter writer, XmlDocument newerDoc)
		{


			//empty lift file, write new elements

			if ( olderReader.Name == "lift" && olderReader.IsEmptyElement) //i.e., <lift/>
			{
				writer.WriteStartElement("lift");
				writer.WriteAttributes(olderReader, true);
				if (newerDoc != null)
				{
					var nodes = newerDoc.SelectNodes("//entry");
					if (nodes != null)
					{
						foreach (XmlNode n in nodes)
						{
							writer.WriteNode(n.CreateNavigator(), true /*REVIEW*/); //REVIEW CreateNavigator
						}
					}
				}
				//write out the closing lift element
				writer.WriteEndElement();
				olderReader.Read();
			}

				//hit the end, write out any remaing new elements

			else if (olderReader.Name == "lift" &&
					 olderReader.NodeType == XmlNodeType.EndElement)
			{
				var nodes = newerDoc.SelectNodes("//entry");
				if (nodes != null)
				{
					foreach (XmlNode n in nodes) //REVIEW CreateNavigator
					{
						writer.WriteNode(n.CreateNavigator(), true /*REVIEW*/);
					}
				}
				//write out the closing lift element
				writer.WriteNode(olderReader, true);
			}
			else
			{
				if (olderReader.Name == "entry")
				{
					MergeLiftUpdateEntryIntoLiftFile(olderReader, writer, newerDoc);
				}
				else
				{
					Utilities.WriteShallowNode(olderReader, writer);
				}

			}
		}

		protected virtual void MergeLiftUpdateEntryIntoLiftFile(XmlReader olderReader, XmlWriter writer, XmlDocument newerDoc)
		{
			string oldId = olderReader.GetAttribute("guid");
			if (String.IsNullOrEmpty(oldId))
			{
				throw new ApplicationException("All entries must have guid attributes in order for merging to work. " +
											   olderReader.Value);
			}
			XmlNode match = newerDoc.SelectSingleNode("//entry[@guid='" + oldId + "']");
			if (match != null)
			{
				olderReader.Skip(); //skip the old one
				writer.WriteNode(match.CreateNavigator(), true); //REVIEW CreateNavigator
				if (match.ParentNode != null)
					match.ParentNode.RemoveChild(match);
			}
			else
			{
				// The default XmlWriter.WriteNode() method is insufficient to write <text> nodes
				// properly if they start with a <span> node!
				// See https://jira.palaso.org/issues/browse/WS-34794.
				var element = olderReader.ReadOuterXml();
				XmlUtils.WriteNode(writer, element, LiftSorter.LiftSuppressIndentingChildren);
			}
		}

		internal class FileInfoLastWriteTimeComparer : IComparer<FileInfo>
		{
			public int Compare(FileInfo x, FileInfo y)
			{
				int timecomparison = DateTime.Compare(x.LastWriteTimeUtc, y.LastWriteTimeUtc);
				if (timecomparison == 0)
				{
					// if timestamps are the same, then sort by name
					return StringComparer.OrdinalIgnoreCase.Compare(x.FullName, y.FullName);
				}
				return timecomparison;
			}
		}
	}
}