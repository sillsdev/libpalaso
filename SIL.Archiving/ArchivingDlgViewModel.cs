using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using L10NSharp;
using SIL.Archiving.Generic;
using SIL.IO;

namespace SIL.Archiving
{
	/// ------------------------------------------------------------------------------------
	public abstract class ArchivingDlgViewModel
	{
		[Flags]
		protected enum MetadataProperties
		{
			Audience = 1 << 0,
			Domains = 1 << 1,
			ContentLanguages = 1 << 2,
			CreationDate = 1 << 3,
			ModifiedDate = 1 << 4,
			SchemaConformance = 1 << 5,
			DatasetExtent = 1 << 6,
			SubjectLanguage = 1 << 7,
			SoftwareRequirements = 1 << 8,
			Contributors = 1 << 9,
			RecordingExtent = 1 << 10,
			GeneralDescription = 1 << 11,
			AbstractDescription = 1 << 12,
			Promotion = 1 << 13,
			Stage = 1 << 14,
			Type = 1 << 15,
			Title = 1 << 16,
			Files = 1 << 17,
		}

		#region Data members
		protected readonly string _id; // ID/Name of the top-level element being archived (can be either a session or a project)
		protected readonly string _appSpecificArchivalProcessInfo;
		protected readonly Dictionary<string, string> _titles = new Dictionary<string, string>(); //Titles of elements being archived (keyed by element id)
		private Dictionary<string, MetadataProperties> _propertiesSet = new Dictionary<string, MetadataProperties>(); // Metadata properties that have been set (keyed by element id)
		private Action<ArchivingDlgViewModel> _setFilesToArchive;

		protected bool _cancelProcess;
		protected readonly Dictionary<string, string> _progressMessages = new Dictionary<string, string>();
		/// <summary>
		/// Keyed and grouped according to whatever logical grouping makes sense in the
		/// calling application. The key for each group will be supplied back to the calling app
		/// for use in "normalizing" file names. In the Tuple for each group, Item1 contains the
		/// enumerable list of files to include, and Item2 contains a progress message to be
		/// displayed when that group of files is being processed.
		/// </summary>
		protected IDictionary<string, Tuple<IEnumerable<string>, string>> _fileLists = new Dictionary<string, Tuple<IEnumerable<string>, string>>();
		protected BackgroundWorker _worker;
		#endregion

		#region Delegates and Events
		/// ------------------------------------------------------------------------------------
		public enum MessageType
		{
			/// <summary>Normal (bold) text</summary>
			Normal,
			/// <summary>Blue text, with "Warning:" label (not localizable)</summary>
			Warning,
			/// <summary>Red text, followed by new line</summary>
			Error,
			/// <summary>Non-bold, indented with tab</summary>
			Detail,
			/// <summary>New line</summary>
			Progress,
			/// <summary>Non-bold, indented 8 spaces, with bullet character (U+00B7)</summary>
			Bullet,
			/// <summary>New line, Dark Green text</summary>
			Success,
			/// <summary>New line, indented 4 spaces</summary>
			Indented,
			/// <summary>Normal text, which will cause display to be cleared when the next message is to be displayed</summary>
			Volatile,
		}

		/// <summary>Delegate for OnDisplayMessage event</summary>
		/// <param name="msg">Message to display</param>
		/// <param name="type">Type of message (which handler can use to determine appropriate color, style, indentation, etc.</param>
		public delegate void DisplayMessageEventHandler(string msg, MessageType type);

		/// <summary>
		/// Notifiers subscribers of a message to display.
		/// </summary>
		public event DisplayMessageEventHandler OnDisplayMessage;

		/// <summary>Delegate for DisplayError event</summary>
		/// <param name="msg">Message to display</param>
		/// <param name="packageTitle">Title of package being created</param>
		/// <param name="e">Exception (can be null)</param>
		public delegate void DisplayErrorEventHandler(string msg, string packageTitle, Exception e);

		/// <summary>
		/// Notifiers subscribers of an error message to report.
		/// </summary>
		public event DisplayErrorEventHandler OnDisplayError;

		/// <summary>Action raised when progress happens</summary>
		public Action IncrementProgressBarAction { protected get; set; }
		#endregion

		#region properties

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Short name/description of the archiving program or standard used for this type of
		/// archiving. (Should fit in the frame "Archive using ___".)
		/// </summary>
		/// ------------------------------------------------------------------------------------
		internal abstract string ArchiveType { get; }
		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Short name of the archiving program to launch once package is created.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public abstract string NameOfProgramToLaunch { get; }

		/// ------------------------------------------------------------------------------------
		public abstract string InformativeText { get; }

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Implement ArchiveInfoHyperlinkText to define text (first occurrence only) in the
		/// InformativeText that will be makred as a hyperlink to ArchiveInfoUrl.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public abstract string ArchiveInfoHyperlinkText { get; }
		/// ------------------------------------------------------------------------------------
		public abstract string ArchiveInfoUrl { get; }

		/// ------------------------------------------------------------------------------------
		/// <summary>Path to the program to launch</summary>
		/// ------------------------------------------------------------------------------------
		public string PathToProgramToLaunch { get; set; }

		/// ------------------------------------------------------------------------------------
		/// <summary>Path to the generated package</summary>
		/// ------------------------------------------------------------------------------------
		public string PackagePath { get; protected set; }

		/// ------------------------------------------------------------------------------------
		public string AppName { get; private set; }

		/// ------------------------------------------------------------------------------------
		public bool IsBusy { get; protected set; }
		#endregion

		#region callbacks
		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Callback function to allow the application to modify the contents of a file rather
		/// than merely copying it. If application performs the "copy" for the given file,
		/// it should return true; otherwise, false.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public Func<ArchivingDlgViewModel, string, string, bool> FileCopyOverride { protected get; set; }

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Callback to do application-specific normalization of filenames to be added to
		/// archive based on the file-list key (param 1) and the filename (param 2).
		/// The StringBuilder (param 3) has the normalized name which the app can alter as needed.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public Action<string, string, StringBuilder> AppSpecificFilenameNormalization { private get; set; }

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Callback to allow application to do special handling of exceptions or other error
		/// conditions. The exception parameter can be null.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public Action<Exception, string> HandleNonFatalError { private get; set; }

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Callback to allow application to handle display of initial summary in log box. If
		/// the application implements this, then the default summary display will be suppressed.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public Action<IDictionary<string, Tuple<IEnumerable<string>, string>>> OverrideDisplayInitialSummary { private get; set; }
		#endregion

		#region construction and initialization
		/// ------------------------------------------------------------------------------------
		/// <summary>Constructor</summary>
		/// <param name="appName">The application name</param>
		/// <param name="title">Title of the submission</param>
		/// <param name="id">Identifier (used as filename) for the package being created</param>
		/// <param name="appSpecificArchivalProcessInfo">Application can use this to pass
		/// additional information that will be displayed to the user in the dialog to explain
		/// any application-specific details about the archival process.</param>
		/// <param name="setFilesToArchive">Delegate to request client to call methods to set
		/// which files should be archived (this is deferred to allow display of progress message)</param>
		/// ------------------------------------------------------------------------------------
		protected ArchivingDlgViewModel(string appName, string title, string id,
			string appSpecificArchivalProcessInfo, Action<ArchivingDlgViewModel> setFilesToArchive)
		{
			if (appName == null)
				throw new ArgumentNullException("appName");
			AppName = appName;
			if (title == null)
				throw new ArgumentNullException("title");
			if (id == null)
				throw new ArgumentNullException("id");
			_id = id;
			_appSpecificArchivalProcessInfo = appSpecificArchivalProcessInfo;
			_setFilesToArchive = setFilesToArchive;
			_titles[id] = title;
			_propertiesSet[id] = MetadataProperties.Title;
			AdditionalMessages = new Dictionary<string, MessageType>();
		}

		/// ------------------------------------------------------------------------------------
		public bool Initialize()
		{
			IsBusy = true;

			try
			{
				if (!DoArchiveSpecificInitialization())
					return false;

				_setFilesToArchive(this);
				foreach (var fileList in _fileLists.Where(fileList => fileList.Value.Item1.Any()))
				{
					string normalizedName = NormalizeFilename(fileList.Key, Path.GetFileName(fileList.Value.Item1.First()));
					_progressMessages[normalizedName] = fileList.Value.Item2;
				}
				DisplayInitialSummary();

				return true;
			}
			finally
			{
				IsBusy = false;
			}
		}

		/// ------------------------------------------------------------------------------------
		protected abstract bool DoArchiveSpecificInitialization();

		/// ------------------------------------------------------------------------------------
		public abstract int CalculateMaxProgressBarValue();

		/// ------------------------------------------------------------------------------------
		public virtual void AddFileGroup(string groupId, IEnumerable<string> files, string progressMessage)
		{
			if (_fileLists.ContainsKey(groupId))
				throw new ArgumentException("Duplicate file group ID.", "groupId");
			_fileLists[groupId] = Tuple.Create(files, progressMessage);
		}

		/// ------------------------------------------------------------------------------------
		private void DisplayInitialSummary()
		{
			if (OverrideDisplayInitialSummary != null)
				OverrideDisplayInitialSummary(_fileLists);
			else if (OnDisplayMessage != null)
			{
				OnDisplayMessage(LocalizationManager.GetString("DialogBoxes.ArchivingDlg.PrearchivingStatusMsg",
						"The following files will be added to the archive:"), MessageType.Normal);

				foreach (var kvp in _fileLists)
				{
					string msg = FileGroupDisplayMessage(kvp.Key);
					if (msg != string.Empty)
						OnDisplayMessage(msg, MessageType.Indented);

					foreach (var file in kvp.Value.Item1)
						OnDisplayMessage(Path.GetFileName(file), MessageType.Bullet);
				}
			}
		}

		protected virtual string FileGroupDisplayMessage(string groupKey)
		{
			return groupKey;
		}
		#endregion

		#region Helper methods
		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Helper method to detect when caller tries to set a property (for the top-level
		/// element) that has already been set and throw an InvalidOperationException if so.
		/// </summary>
		/// <param name="property">The property to check (and add to the list of properties that
		/// can no longer be set again).</param>
		/// ------------------------------------------------------------------------------------
		protected void PreventDuplicateMetadataProperty(MetadataProperties property)
		{
			PreventDuplicateMetadataProperty(_id, property);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Helper method to detect when caller tries to set a property (for the specified
		/// element) that has already been set and throw an InvalidOperationException if so.
		/// </summary>
		/// <param name="elementId">The element id </param>
		/// <param name="property">The property to check (and add to the list of properties that
		/// can no longer be set again).</param>
		/// ------------------------------------------------------------------------------------
		protected void PreventDuplicateMetadataProperty(string elementId, MetadataProperties property)
		{
			MetadataProperties propertiesSet;
			if (_propertiesSet.TryGetValue(elementId, out propertiesSet))
			{
				if (propertiesSet.HasFlag(property))
					throw new InvalidOperationException(string.Format("{0} has already been set", property.ToString()));
				_propertiesSet[elementId] |= property;
			}
			else
				_propertiesSet[elementId] = property;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Helper method to determine if the given property (for the top-level element) has
		/// already been set.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		protected bool IsMetadataPropertySet(MetadataProperties property)
		{
			return IsMetadataPropertySet(_id, property);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Helper method to determine if the given property (for the specified element) has
		/// already been set.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		protected bool IsMetadataPropertySet(string elementId, MetadataProperties property)
		{
			MetadataProperties propertiesSet;
			if (!_propertiesSet.TryGetValue(elementId, out propertiesSet))
				return false;
			return propertiesSet.HasFlag(property);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Helper method to note that the given property (for the top-level element) has
		/// been set.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		protected void MarkMetadataPropertyAsSet(MetadataProperties property)
		{
			MarkMetadataPropertyAsSet(_id, property);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Helper method to note that the given property (for the specified element) has
		/// been set.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		protected void MarkMetadataPropertyAsSet(string elementId, MetadataProperties property)
		{
			if (!_propertiesSet.ContainsKey(elementId))
				_propertiesSet[elementId] = property;
			_propertiesSet[elementId] |= property;
		}
		#endregion

		#region Methods for setting common fields
		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Sets an abstract/description for this resource in a single language
		/// </summary>
		/// <param name="description">The abstract description</param>
		/// <param name="language">ISO 639-2 3-letter language code (can be left empty if
		/// language is not known)</param>
		/// ------------------------------------------------------------------------------------
		public void SetAbstract(string description, string language)
		{
			IDictionary<string, string> dictionary = new Dictionary<string, string>(1);
			dictionary[language] = description;
			SetAbstract(dictionary);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Sets abstracts/descriptions for this resource in (potentially) multiple languages
		/// </summary>
		/// <param name="descriptions">Dictionary of language->abstract, where the keys are ISO
		/// 639-2 3-letter language codes</param>
		/// ------------------------------------------------------------------------------------
		public void SetAbstract(IDictionary<string, string> descriptions)
		{
			if (descriptions == null)
				throw new ArgumentNullException("descriptions");

			if (descriptions.Count == 0)
				return;

			if (descriptions.Count > 1)
			{
				if (descriptions.Keys.Any(k => k.Length != 3))
					throw new ArgumentException();
			}
			PreventDuplicateMetadataProperty(MetadataProperties.AbstractDescription);
			SetAbstract_Impl(descriptions);
		}

		#region Abstract versions of methods
		/// ------------------------------------------------------------------------------------
		protected abstract void SetAbstract_Impl(IDictionary<string, string> descriptions);

		#endregion

		#endregion

		/// ------------------------------------------------------------------------------------
		public void DisplayMessage(string msg, MessageType type)
		{
			if (OnDisplayMessage != null)
				OnDisplayMessage(msg, type);
		}

		/// ------------------------------------------------------------------------------------
		public abstract string GetMetadata();

		/// ------------------------------------------------------------------------------------
		internal abstract void LaunchArchivingProgram();

		/// ------------------------------------------------------------------------------------
		protected void LaunchArchivingProgram(Action HandleInvalidOperation)
		{
			if (string.IsNullOrEmpty(PathToProgramToLaunch) || !File.Exists(PathToProgramToLaunch))
				return;

			try
			{
				var prs = new Process();
				prs.StartInfo.FileName = PathToProgramToLaunch;
				if (!string.IsNullOrEmpty(PackagePath))
					prs.StartInfo.Arguments = "\"" + PackagePath + "\"";
				prs.Start();
			}
			catch (Exception e)
			{
				if (e is InvalidOperationException && HandleInvalidOperation != null)
					HandleInvalidOperation();
				else
				{
					ReportError(e, string.Format(LocalizationManager.GetString("DialogBoxes.ArchivingDlg.StartingRampErrorMsg",
						"There was an error attempting to open the archive package in {0}."), PathToProgramToLaunch));
				}
			}
		}

		/// ------------------------------------------------------------------------------------
		public abstract bool CreatePackage();

		/// ------------------------------------------------------------------------------------
		protected virtual StringBuilder DoArchiveSpecificFilenameNormalization(string key, string fileName)
		{
			return AppSpecificFilenameNormalization != null ? new StringBuilder(fileName) : null;
		}

		/// ------------------------------------------------------------------------------------
		public virtual string NormalizeFilename(string key, string fileName)
		{
			StringBuilder bldr = DoArchiveSpecificFilenameNormalization(key, fileName);
			if (AppSpecificFilenameNormalization != null)
				AppSpecificFilenameNormalization(key, fileName, bldr);
			return bldr.ToString();
		}

		/// ------------------------------------------------------------------------------------
		const int CopyBufferSize = 64 * 1024;
		/// ------------------------------------------------------------------------------------
		protected static void CopyFile(string src, string dest)
		{
			using (var outputFile = File.OpenWrite(dest))
			{
				using (var inputFile = File.OpenRead(src))
				{
					var buffer = new byte[CopyBufferSize];
					int bytesRead;
					while ((bytesRead = inputFile.Read(buffer, 0, CopyBufferSize)) != 0)
					{
						outputFile.Write(buffer, 0, bytesRead);
					}
				}
			}
		}

		/// ------------------------------------------------------------------------------------
		public virtual void Cancel()
		{
			if (_cancelProcess)
				return;

			_cancelProcess = true;

			if (_worker != null)
			{
				DisplayMessage(Environment.NewLine + LocalizationManager.GetString(
						"DialogBoxes.ArchivingDlg.CancellingMsg", "Canceling..."), MessageType.Error);

				_worker.CancelAsync();
				while (_worker.IsBusy)
					Application.DoEvents();
			}
		}

		/// ------------------------------------------------------------------------------------
		protected void ReportError(Exception e, string msg)
		{
			if (OnDisplayError != null)
				OnDisplayError(msg, _titles[_id], e);
			else if (e != null)
				throw e;

			if (HandleNonFatalError != null)
				HandleNonFatalError(e, msg);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// The file locations are different on Linux than on Windows
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public static bool IsMono
		{
			get { return (Type.GetType("Mono.Runtime") != null); }
		}

		/// <summary></summary>
		/// <param name="sessionId"></param>
		public abstract IArchivingSession AddSession(string sessionId);

		/// <summary></summary>
		public abstract IArchivingPackage ArchivingPackage { get; }

		/// <remarks/>
		public Dictionary<string, MessageType> AdditionalMessages { get; private set; }

		public bool PathIsAccessible(string directory)
		{
			try
			{
				var isWritable = DirectoryUtilities.IsDirectoryWritable(directory);

				if (isWritable)
					return true;

				var msg = LocalizationManager.GetString(
					"DialogBoxes.ArchivingDlg.DirectoryNotWritableMsg",
					"The path is not accessible: {0}");

				DisplayMessage(string.Format(msg, directory), MessageType.Normal);

				return false;
			}
			catch (Exception e)
			{
				DisplayMessage(e.Message, MessageType.Warning);
				return false;
			}
		}
	}

	public interface ISupportMetadataOnly
	{
		bool MetadataOnly { get; set; }
	}
}
