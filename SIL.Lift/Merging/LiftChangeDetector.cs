using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Xml.XPath;
using SIL.Lift.Merging.xmldiff;

namespace SIL.Lift.Merging
{
	///<summary></summary>
	public interface ILiftChangeDetector
	{
		/// <summary>
		/// Makes the reference data just reflect exactly what is in the lift file.  Only changes made after calling this will be detected.
		/// </summary>
		void Reset();
		///<summary></summary>
		void ClearCache();
		///<summary></summary>
		bool CanProvideChangeRecord { get; }
		///<summary></summary>
		ILiftChangeReport GetChangeReport(IProgress progress);
	}

	///<summary></summary>
	public class LiftChangeDetector : ILiftChangeDetector
	{
		private readonly string _pathToLift;
		private readonly string _pathToCacheDir;

		///<summary></summary>
		public LiftChangeDetector(string pathToLift, string pathToCacheDir)
		{
			_pathToLift = pathToLift;
			_pathToCacheDir = pathToCacheDir;
			//no no no! Reset();
		}

		/// <summary>
		/// Makes the reference data just reflect exactly what is in the lift file.  Only changes made after calling this will be detected.
		/// </summary>
		public void Reset()
		{
			try
			{
				if (!Directory.Exists(_pathToCacheDir))
				{
					return; //if they don't have a cache directory yet, then it's proper for us to NOT have a reference copy
							//we'll get reset() again after they build the cache/load the db/whatever.
				}


				File.Copy(_pathToLift, PathToReferenceCopy, true);
			}
			catch (Exception error)
			{
				throw new ApplicationException(
					string.Format("LiftChangeDetector could not copy the working file to the reference file at ({0} to {1}).  {2}",
								  _pathToLift, PathToReferenceCopy, error.Message));
			}
		}

		///<summary></summary>
		public void ClearCache()
		{
			if (File.Exists(PathToReferenceCopy))
			{
				File.Delete(PathToReferenceCopy);
			}
		}

		/// <summary>
		/// Checks if both the Lift file and a cached reference copy exist.
		/// </summary>
		public bool CanProvideChangeRecord
		{
			get { return File.Exists(PathToReferenceCopy) && File.Exists(_pathToLift); }
		}

		///<summary></summary>
		public ILiftChangeReport GetChangeReport(IProgress progress)
		{
			StreamReader reference = null;
			StreamReader working = null;
			try
			{
				try
				{
					reference = new StreamReader(PathToReferenceCopy);
				}
				catch (Exception error)
				{
					throw new ApplicationException(
						string.Format("Could not open LiftChangeDetector Reference file at {0}.  {1}",
									  PathToReferenceCopy, error.Message));
				}

				working = new StreamReader(_pathToLift);

				return LiftChangeReport.DetermineChanges(reference, working, progress);
			}
			finally
			{
				if (reference != null)
				{
					reference.Dispose();
				}

				if (working != null)
				{
					working.Dispose();
				}
			}
		}

		private string PathToReferenceCopy
		{
			get { return Path.Combine(_pathToCacheDir, "reference.lift"); }
		}
	}

	///<summary></summary>
	public interface ILiftChangeReport
	{
		///<summary></summary>
		LiftChangeReport.ChangeType GetChangeType(string entryId);
		///<summary></summary>
		IList<string> IdsOfDeletedEntries { get; }
	}

	///<summary></summary>
	public class LiftChangeReport : ILiftChangeReport
	{
		private IList<string> _idsOfDeletedEntries;
		private IList<string> _idsOfAddedEntries;
		private IList<string> _idsOfEditedEntries;
		private IList<string> _idsInOriginal;
		///<summary></summary>
		public enum ChangeType
		{
			///<summary></summary>
			None,
			///<summary></summary>
			Editted,
			///<summary></summary>
			New,
			///<summary></summary>
			Deleted
		}

		///<summary></summary>
		public static LiftChangeReport DetermineChanges(TextReader original, TextReader modified, IProgress progress)
		{
			LiftChangeReport detector = new LiftChangeReport();
			detector.ComputeDiff(original, modified, progress);
			return detector;

		}

		///<summary></summary>
		public ChangeType GetChangeType(string entryId)
		{
			if (_idsOfEditedEntries.Contains(entryId))
				return ChangeType.Editted;
			if (_idsOfAddedEntries.Contains(entryId))
				return ChangeType.New;

			//a client is probably going to use the IdsOfDeletedEntries that to give us ids of the original file, but
			//we do this here for completeness
			if (_idsOfDeletedEntries.Contains(entryId))
				return ChangeType.Deleted;

			return ChangeType.None;
		}

		///<summary></summary>
		public IList<string> IdsOfDeletedEntries
		{
			get { return _idsOfDeletedEntries; }
		}

		private void ComputeDiff(TextReader original, TextReader modified, IProgress progress)
		{
			//enchance: just get a checksum for each entry in both files, use that to determine everything else

			_idsOfDeletedEntries = new List<string>();
			_idsOfAddedEntries = new List<string>();
			_idsOfEditedEntries = new List<string>();
			_idsInOriginal = new List<string>();

			XPathDocument modifiedDoc = new XPathDocument(modified);
			XPathNavigator modifiedNav = modifiedDoc.CreateNavigator();


			XPathDocument originalDoc = new XPathDocument(original);
			XPathNavigator originalNav = originalDoc.CreateNavigator();
			XPathNodeIterator liftElement = originalNav.SelectChildren(XPathNodeType.Element);
			liftElement.MoveNext();//move to the one and only <lift> element
			if (liftElement.Current == null)
				return;
			XPathNodeIterator originalChildren = liftElement.Current.SelectChildren(XPathNodeType.Element);
			StringDictionary idToContentsOfModifiedEntries = new StringDictionary();

			XPathNodeIterator liftOfModifiedFile = modifiedNav.SelectChildren(XPathNodeType.Element);
			liftOfModifiedFile.MoveNext();
			if (liftOfModifiedFile.Current != null)
			{
				XPathNodeIterator modifiedChildren = liftOfModifiedFile.Current.SelectChildren(XPathNodeType.Element);
				while (modifiedChildren.MoveNext())
				{
					//TODO: consider if there are benefits to using guid as the first key, then try id
					if (modifiedChildren.Current != null)
					{
						string id = modifiedChildren.Current.GetAttribute("id", string.Empty);
						idToContentsOfModifiedEntries.Add(id, modifiedChildren.Current.OuterXml);
					}
				}
			}

			while (originalChildren.MoveNext())
			{
				if (originalChildren.Current != null)
				{
					string id = originalChildren.Current.GetAttribute("id", string.Empty);
					_idsInOriginal.Add(id);

					if (!idToContentsOfModifiedEntries.ContainsKey(id))
					{
						_idsOfDeletedEntries.Add(id);
					}
					else
					{
						var diff = new XmlDiff(originalChildren.Current.OuterXml, idToContentsOfModifiedEntries[id]);
						DiffResult result = diff.Compare();
						if (!result.AreEqual)
						{
							_idsOfEditedEntries.Add(id);
						}
					}
				}
			}
			foreach (string id in idToContentsOfModifiedEntries.Keys)
			{
				if (!_idsInOriginal.Contains(id))
					_idsOfAddedEntries.Add(id);
			}
		}
	}

	///<summary>
	/// Minimal interface for progress reporting.
	///</summary>
	public interface IProgress
	{
		///<summary>
		/// Get/set the status message for a progress report.
		///</summary>
		string Status { set; get; }
	}

	///<summary>
	/// Nonfunctional progress reporting class: minimal implementation of IProgress.
	/// (possibly useful for tests)
	///</summary>
	public class NullProgress : IProgress
	{
		private string _status;

		///<summary></summary>
		public string Status
		{
			get { return _status; }
			set { _status = value; }
		}
	}
}
