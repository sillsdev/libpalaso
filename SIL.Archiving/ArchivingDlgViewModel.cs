using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using SIL.Archiving.IMDI.Lists;
using SIL.Code;
using SIL.EventsAndDelegates;
using SIL.IO;
using static System.String;
// ReSharper disable InconsistentNaming

namespace SIL.Archiving
{
	/// ------------------------------------------------------------------------------------
	public abstract class ArchivingDlgViewModel : IDisposable
	{
		public enum Standard
		{
			REAP,
			IMDI,
			Other,
		}

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
		private readonly string _id; // ID/Name of the top-level element being archived (can be either a session or a project)
		private readonly Dictionary<string, string> _titles = new Dictionary<string, string>(); //Titles of elements being archived (keyed by element id)
		private readonly Dictionary<string, MetadataProperties> _propertiesSet = new Dictionary<string, MetadataProperties>(); // Metadata properties that have been set (keyed by element id)
		private readonly Action<ArchivingDlgViewModel, CancellationToken> _setFilesToArchive;
		private readonly Dictionary<string, Tuple<IEnumerable<string>, string>> _fileLists = new Dictionary<string, Tuple<IEnumerable<string>, string>>();

		/// <summary>
		/// Keyed and grouped according to whatever logical grouping makes sense in the
		/// calling application. The key for each group will be supplied back to the calling app
		/// for use in "normalizing" file names. In the Tuple for each group, Item1 contains the
		/// enumerable list of files to include, and Item2 contains a progress message to be
		/// displayed when that group of files is being processed.
		/// </summary>
		protected IDictionary<string, Tuple<IEnumerable<string>, string>> FileLists => _fileLists;
		#endregion

		#region Delegates and Events
		/// ------------------------------------------------------------------------------------
		public enum StringId
		{
			PreArchivingStatus,
			SearchingForArchiveUploadingProgram,
			ArchiveUploadingProgramNotFound,
			IMDIPackageInvalid,
			ErrorStartingArchivalProgram,
			PreparingFiles,
			SavingFilesInPackage,
			FileExcludedFromPackage,
			PathNotWritable,
			ReadyToCallRampMsg,
			ErrorCreatingArchive,
			IMDIActorsGroup,
			CopyingFiles,
			FailedToMakePackage,
			ErrorCreatingMetsFile,
			RampPackageRemoved
		}

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
			/// <summary>Like Normal, but with preceding new line</summary>
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

		/// <summary>Delegate for <see cref="OnReportMessage"/> event</summary>
		/// <param name="msg">Message to display</param>
		/// <param name="type">Type of message (which handler can use to determine appropriate
		/// presentation formatting to use).</param>
		public delegate void MessageEventHandler(string msg, MessageType type);

		/// <summary>
		/// Notifies subscribers of a message to display.
		/// </summary>
		public event MessageEventHandler OnReportMessage;

		/// <summary>Delegate for <see cref="OnError"/> event</summary>
		/// <param name="msg">Error message to display</param>
		/// <param name="packageTitle">Title of package being created</param>
		/// <param name="e">Exception (can be null)</param>
		public delegate void ErrorEventHandler(string msg, string packageTitle, Exception e);

		/// <summary>
		/// Notifiers subscribers of an error message to report.
		/// </summary>
		public event ErrorEventHandler OnError;

		/// <summary>Delegate for <see cref="OnExceptionDuringLaunch"/> event</summary>
		/// <param name="args">Event args that hold the exception that occurred</param>
		public delegate void ExceptionHandler(EventArgs<Exception> args);

		/// <summary>
		/// Notifies subscribers of an exception thrown during launch of archive uploader program.
		/// </summary>
		public event ExceptionHandler OnExceptionDuringLaunch;

		protected IArchivingProgressDisplay Progress { get; private set; }
		#endregion

		#region properties

		protected string PackageId => _id;
		protected string PackageTitle => _titles[_id];

		/// ------------------------------------------------------------------------------------
		public abstract Standard ArchiveType { get; }

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Short name of the archiving program to launch once package is created.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public abstract string NameOfProgramToLaunch { get; }

		/// ------------------------------------------------------------------------------------
		public abstract string ArchiveInfoUrl { get; }

		/// ------------------------------------------------------------------------------------
		/// <summary>Path to the program to launch</summary>
		/// ------------------------------------------------------------------------------------
		public string PathToProgramToLaunch { get; set; }

		/// ------------------------------------------------------------------------------------
		/// <summary>Path to the generated package</summary>
		/// <remarks>This is also returned by CreatePackage if successful.</remarks>
		/// ------------------------------------------------------------------------------------
		public string PackagePath { get; protected set; }

		/// ------------------------------------------------------------------------------------
		public string AppName { get; }

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
		/// Callback to allow application to handle display of initial summary (in log box). If
		/// the application implements this, then the default summary display will be suppressed.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public Action<IDictionary<string, Tuple<IEnumerable<string>, string>>, CancellationToken> OverrideDisplayInitialSummary { private get; set; }

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Delegate function that can be set to override the default pre-archiving message
		/// shown in <see cref="DisplayInitialSummary"/>. By default, only a single message (as
		/// determined by the <see cref="IArchivingProgressDisplay.GetMessage"/>) is displayed,
		/// so this override is particularly useful if an application needs to display more than
		/// one message before the archival creation begins.
		/// </summary>
		/// <remarks>This is used only in the normal default implementation of
		/// <see cref="DisplayInitialSummary"/></remarks>, so if
		/// <see cref="OverrideDisplayInitialSummary"/> is set, then there is no point in also
		/// setting this delegate.
		/// ------------------------------------------------------------------------------------
		public Func<IDictionary<string, Tuple<IEnumerable<string>, string>>, IEnumerable<Tuple<string, MessageType>>> GetOverriddenPreArchivingMessages { private get; set; }

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// This can be set to override the default <see cref="MessageType"/> used for displaying
		/// information about each file group in <see cref="DisplayInitialSummary"/>. The default
		/// type is <see cref="MessageType.Indented"/>.
		/// </summary>
		/// <remarks>This is used only in the normal default implementation of
		/// <see cref="DisplayInitialSummary"/></remarks>, so if
		/// <see cref="OverrideDisplayInitialSummary"/> is set, then there is no point in also
		/// setting this property.
		/// ------------------------------------------------------------------------------------
		public MessageType InitialFileGroupDisplayMessageType { private get; set; } = MessageType.Indented;

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Delegate function that can be set to override the default "message" displayed for
		/// file groups in <see cref="DisplayInitialSummary"/>. The default is simply the file
		/// groupId (as set in <see cref="AddFileGroup"/>.
		/// </summary>
		/// <remarks>This is used only in the normal default implementation of
		/// <see cref="DisplayInitialSummary"/></remarks>, so if
		/// <see cref="OverrideDisplayInitialSummary"/> is set, then there is no point in also
		/// setting this delegate.
		/// ------------------------------------------------------------------------------------
		public Func<string, string> OverrideGetFileGroupDisplayMessage { private get; set; }
		#endregion

		#region construction and initialization
		/// ------------------------------------------------------------------------------------
		/// <summary>Constructor</summary>
		/// <param name="appName">The application name</param>
		/// <param name="title">Title of the submission</param>
		/// <param name="id">Identifier (used as filename) for the package being created</param>
		/// <param name="setFilesToArchive">Delegate to request client to call methods to set
		/// which files should be archived (this is deferred to allow display of progress
		/// message). Clients will normally do this by calling <see cref="AddFileGroup"/> one or
		/// more times.</param>
		/// ------------------------------------------------------------------------------------
		protected ArchivingDlgViewModel(string appName, string title, string id,
			Action<ArchivingDlgViewModel, CancellationToken> setFilesToArchive)
		{
			AppName = appName ?? throw new ArgumentNullException(nameof(appName));
			_id = id ?? throw new ArgumentNullException(nameof(id));
			_setFilesToArchive = setFilesToArchive;
			_titles[id] = title ?? throw new ArgumentNullException(nameof(title));
			_propertiesSet[id] = MetadataProperties.Title;
			AdditionalMessages = new Dictionary<string, MessageType>();
		}

		/// ------------------------------------------------------------------------------------
		public async Task<bool> Initialize(IArchivingProgressDisplay progress, CancellationToken cancellationToken)
		{
			Progress = progress ?? throw new ArgumentNullException(nameof(progress));

			Console.WriteLine($"{ArchiveType} Tests TEMP: {nameof(Initialize)} start");

			if (!DoArchiveSpecificInitialization())
				return false;

			Console.WriteLine($"{ArchiveType} Tests TEMP: {nameof(Initialize)} archive-specific initialization complete");

			await SetFilesToArchive(cancellationToken);

			Console.WriteLine($"{ArchiveType} Tests TEMP: {nameof(Initialize)} {nameof(SetFilesToArchive)} complete");

			DisplayInitialSummary(cancellationToken);

			Console.WriteLine($"{ArchiveType} Tests TEMP: {nameof(Initialize)} end");

			return true;
		}

		protected virtual async Task SetFilesToArchive(CancellationToken cancellationToken)
		{
			Console.WriteLine($"{ArchiveType} Tests TEMP: {nameof(SetFilesToArchive)} start");

			await Task.Run(() =>
			{
				_setFilesToArchive(this, cancellationToken);
				Console.WriteLine($"{ArchiveType} Tests TEMP: calling _setFilesToArchive finished");
			}, cancellationToken);

			Console.WriteLine($"{ArchiveType} Tests TEMP: {nameof(SetFilesToArchive)} end");
		}

		/// ------------------------------------------------------------------------------------
		protected abstract bool DoArchiveSpecificInitialization();

		/// ------------------------------------------------------------------------------------
		public abstract int CalculateMaxProgressBarValue();

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Adds a group of related files to be included when creating the archive package.
		/// </summary>
		/// <param name="groupId">A string that uniquely identifies the group of files.</param>
		/// <param name="files">The collection fo files (paths)</param>
		/// <param name="addingToArchiveProgressMessage">A progress message that would be
		/// appropriate to display (if relevant to the type of archive package) when the file is
		/// actually being added.</param>
		/// <exception cref="ArgumentException">Thrown if a duplicate file group ID is specified
		/// </exception>
		/// ------------------------------------------------------------------------------------
		public virtual void AddFileGroup(string groupId, IEnumerable<string> files,
			string addingToArchiveProgressMessage)
		{
			Guard.AgainstNull(groupId, nameof(groupId));
			if (FileLists.ContainsKey(groupId))
				throw new ArgumentException("Duplicate file group ID: " + groupId, nameof(groupId));
			FileLists[groupId] = Tuple.Create(files, addingToArchiveProgressMessage);
		}

		/// ------------------------------------------------------------------------------------
		private void DisplayInitialSummary(CancellationToken cancellationToken)
		{
			Console.WriteLine($"{ArchiveType} Tests TEMP: {nameof(DisplayInitialSummary)} start");

			if (OverrideDisplayInitialSummary != null)
			{
				OverrideDisplayInitialSummary(FileLists, cancellationToken);
				return;
			}

			Console.WriteLine($"{ArchiveType} Tests TEMP: {nameof(DisplayInitialSummary)} after override");

			foreach (var message in AdditionalMessages)
				DisplayMessage(message.Key + "\n", message.Value);

			Console.WriteLine($"{ArchiveType} Tests TEMP: {nameof(DisplayInitialSummary)} after additional");

			if (GetOverriddenPreArchivingMessages != null)
			{
				bool firstMsg = true;
				foreach (var msg in GetOverriddenPreArchivingMessages(FileLists))
				{
					if (firstMsg)
					{
						ReportProgress(msg.Item1, msg.Item2, cancellationToken);
						firstMsg = false;
					}
					else
						DisplayMessage(msg.Item1, msg.Item2);
				}

				Console.WriteLine($"{ArchiveType} Tests TEMP: {nameof(DisplayInitialSummary)} after overridden Pre-Archiving messages");
			}
			else
			{
				ReportProgress(Progress.GetMessage(StringId.PreArchivingStatus),
					MessageType.Normal, cancellationToken);

				Console.WriteLine($"{ArchiveType} Tests TEMP: {nameof(DisplayInitialSummary)} after normal Pre-Archiving message");
			}

			foreach (var kvp in FileLists)
			{
				if (cancellationToken.IsCancellationRequested)
					throw new OperationCanceledException();

				string msg = FileGroupDisplayMessage(kvp.Key);
				if (msg != Empty)
					DisplayMessage(msg, InitialFileGroupDisplayMessageType);

				foreach (var file in kvp.Value.Item1)
					DisplayMessage(Path.GetFileName(file), MessageType.Bullet);
			}

			Console.WriteLine($"{ArchiveType} Tests TEMP: {nameof(DisplayInitialSummary)} end");
		}

		protected virtual string FileGroupDisplayMessage(string groupKey)
		{
			return OverrideGetFileGroupDisplayMessage?.Invoke(groupKey) ?? groupKey;
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
			if (_propertiesSet.TryGetValue(elementId, out var propertiesSet))
			{
				if (propertiesSet.HasFlag(property))
					throw new InvalidOperationException($"{property} has already been set");
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
			if (!_propertiesSet.TryGetValue(elementId, out var propertiesSet))
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
				throw new ArgumentNullException(nameof(descriptions));

			if (descriptions.Count == 0)
				return;

			if (descriptions.Count > 1)
			{
				var invalidLanguageCode = descriptions.Keys.FirstOrDefault(k => k.Length != 3);
				if (invalidLanguageCode != null)
					throw new InvalidLanguageCodeException(invalidLanguageCode);
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
		protected void DisplayMessage(StringId msgId, MessageType type, params object[] fmtParams)
		{
			if (OnReportMessage != null)
			{
				var msg = Progress.GetMessage(msgId);
				if (fmtParams?.Length > 0)
					msg = Format(msg, fmtParams);
				if (msg != null)
					OnReportMessage(msg, type);
			}
		}

		/// ------------------------------------------------------------------------------------
		[PublicAPI]
		public void DisplayMessage(string msg, MessageType type)
		{
			OnReportMessage?.Invoke(msg, type);
		}

		/// ------------------------------------------------------------------------------------
		[PublicAPI]
		public abstract string GetMetadata();

		/// ------------------------------------------------------------------------------------
		public virtual void LaunchArchivingProgram()
		{
			if (IsNullOrEmpty(PathToProgramToLaunch) || !File.Exists(PathToProgramToLaunch))
				return;

			var prs = new Process();
			prs.StartInfo.FileName = PathToProgramToLaunch;
			if (!IsNullOrEmpty(PackagePath))
				prs.StartInfo.Arguments = "\"" + PackagePath + "\"";
			LaunchArchivingProgram(prs);
		}

		/// ------------------------------------------------------------------------------------
		protected void LaunchArchivingProgram(Process prs)
		{
			try
			{
				prs.Start();
			}
			catch (Exception e)
			{
				OnExceptionDuringLaunch?.Invoke(new EventArgs<Exception>(e));

				ReportError(e, Progress.GetMessage(StringId.ErrorStartingArchivalProgram));
			}
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>Creates the archiving package asynchronously</summary>
		/// <returns>A task, which when completed, provides the "result" (typically a file path);
		/// or null if package creation failed.
		/// </returns>
		/// ------------------------------------------------------------------------------------
		public abstract Task<string> CreatePackage(CancellationToken cancellationToken);

		/// ------------------------------------------------------------------------------------
		/// <summary>Report archiving progress at a major (success) point and check to see if
		/// user has requested cancellation</summary>
		/// ------------------------------------------------------------------------------------
		protected void ReportMajorProgressPoint(StringId id, CancellationToken cancellationToken,
			bool isAtCancelablePoint = true)
		{
			ReportProgress(Progress.GetMessage(id), MessageType.Success, cancellationToken,
				isAtCancelablePoint);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>Report archiving progress and check to see if user has requested
		/// cancellation</summary>
		/// ------------------------------------------------------------------------------------
		protected void ReportProgress(string message, MessageType type,
			CancellationToken cancellationToken, bool isAtCancelablePoint = true)
		{
			if (cancellationToken.IsCancellationRequested)
			{
				if (!isAtCancelablePoint)
					return;
				CleanUp();
				throw new OperationCanceledException();
			}

			DisplayMessage(message, type);
			Progress.IncrementProgress();
		}

		public virtual void Dispose()
		{
			CleanUp();
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>Performs any needed clean-up after creating a package or when creation is
		/// canceled</summary>
		/// ------------------------------------------------------------------------------------
		protected internal virtual void CleanUp()
		{
			// delete temp files, etc.
		}

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
		private const int kCopyBufferSize = 64 * 1024;
		/// ------------------------------------------------------------------------------------
		protected static void CopyFile(string src, string dest)
		{
			using (var outputFile = File.OpenWrite(dest))
			{
				using (var inputFile = File.OpenRead(src))
				{
					var buffer = new byte[kCopyBufferSize];
					int bytesRead;
					while ((bytesRead = inputFile.Read(buffer, 0, kCopyBufferSize)) != 0)
					{
						outputFile.Write(buffer, 0, bytesRead);
					}
				}
			}
		}

		/// ------------------------------------------------------------------------------------
		protected void ReportError(Exception e, string msg)
		{
			if (OnError != null)
				OnError(msg, _titles[_id], e);
			else if (e != null)
				throw e;

			HandleNonFatalError?.Invoke(e, msg);
		}

		protected string GetFileExcludedMsg(string file) =>
			Progress.GetMessage(StringId.FileExcludedFromPackage) + file;

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// The file locations are different on Linux than on Windows
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public static bool IsMono => (Type.GetType("Mono.Runtime") != null);

		/// <remarks/>
		public Dictionary<string, MessageType> AdditionalMessages { get; }

		public bool IsPathWritable(string directory)
		{
			try
			{
				if (DirectoryUtilities.IsDirectoryWritable(directory))
					return true;
			}
			catch (Exception e)
			{
				OnReportMessage?.Invoke(e.Message, MessageType.Warning);
				return false;
			}

			DisplayMessage(StringId.PathNotWritable, MessageType.Warning, directory);

			return false;
		}
	}

	public interface ISupportMetadataOnly
	{
		bool MetadataOnly { get; set; }
	}
}
