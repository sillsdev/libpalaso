/*
 * Original code from https://code.google.com/archive/p/snowcode/
 * License: MIT (http://www.opensource.org/licenses/mit-license.php)
 *
 * The code has been modified (mostly in the authentication area)
 * and greatly simplified (extracting only the bits we are using).
 */

using System;
using Amazon.Runtime;
using Microsoft.Build.Framework;

namespace SIL.BuildTasks.AWS.S3
{
	/// <summary>
	/// MSBuild task to publish a set of files to a S3 bucket.
	/// </summary>
	/// <remarks>If made public the files will be available at https://s3.amazonaws.com/bucket_name/folder/file_name</remarks>
	public class S3BuildPublisher : AwsTaskBase
	{
		#region Properties

		/// <summary>
		/// Gets or sets the source files to be stored.
		/// Must provide one and only one of SourceFiles or SourceFolder.
		/// </summary>
		/// <remarks>Subfolders are not supported.</remarks>
		public string[] SourceFiles { get; set; }

		/// <summary>
		/// Gets or sets the source folder to be uploaded.
		/// Must provide one and only one of SourceFiles or SourceFolder.
		/// </summary>
		public string SourceFolder { get; set; }

		/// <summary>
		/// Gets or sets the destination folder.
		/// </summary>
		public string DestinationFolder { get; set; }

		/// <summary>
		/// Gets or sets the AWS S3 bucket to store the files in
		/// </summary>
		[Required]
		public string DestinationBucket { get; set; }

		/// <summary>
		/// Gets or sets if the files should be publically readable
		/// </summary>
		public bool IsPublicRead { get; set; }

		/// <summary>
		/// Gets or sets the content type that should be set in the S3 metadata for the files
		/// </summary>
		public string ContentType { get; set; }

		/// <summary>
		/// Gets or sets the content encoding that should be set in the S3 metadata for the files
		/// (e.g., gzip)
		/// </summary>
		public string ContentEncoding { get; set; }

		#endregion

		public override bool Execute()
		{
			if (SourceFiles == null && SourceFolder == null)
				throw new ArgumentException("SourceFiles or SourceFolder is required.");

			if (SourceFiles != null && SourceFolder != null)
				throw new ArgumentException("Only SourceFiles or SourceFolder can be set.");

			if (SourceFolder != null)
				return ProcessDirectory();

			return ProcessFiles();
		}

		#region Private methods

		private bool ProcessFiles()
		{
			Log.LogMessage(MessageImportance.Normal, "Publishing Sourcefiles={0} to {1}", Join(SourceFiles), DestinationBucket);

			ShowAclWarnings();

			try
			{
				ValidateFolder();

				PublishFiles(GetAwsCredentials());

				return true;
			}
			catch (Exception ex)
			{
				Log.LogErrorFromException(ex);
				return false;
			}
		}

		private bool ProcessDirectory()
		{
			Log.LogMessage(MessageImportance.Normal, "Publishing SourceFolder={0} to {1}", SourceFolder, DestinationBucket);

			ShowAclWarnings();

			try
			{
				PublishDirectory(GetAwsCredentials());
				return true;
			}
			catch (Exception ex)
			{
				Log.LogErrorFromException(ex);
				return false;
			}
		}

		private void ShowAclWarnings()
		{
			if (IsPublicRead)
			{
				Log.LogMessage(MessageImportance.High, "File(s) will be public accessible");
			}
		}

		private void ValidateFolder()
		{
			// ignore if null.
			if (DestinationFolder == null)
			{
				return;
			}

			if (DestinationFolder.StartsWith(@"\") || DestinationFolder.StartsWith("/"))
			{
				throw new Exception(@"Folder should not start with a \ or /");
			}

			if (DestinationFolder.EndsWith(@"\"))
			{
				throw new Exception(@"Folder should not end with a \");
			}
		}

		private void PublishFiles(AWSCredentials credentials)
		{
			using (var helper = new S3Helper(credentials))
			{
				helper.Publish(SourceFiles, DestinationBucket, DestinationFolder, IsPublicRead, ContentType, ContentEncoding);
				Log.LogMessage(MessageImportance.Normal, "Published {0} files to S3", SourceFiles.Length);
			}
		}

		private void PublishDirectory(AWSCredentials credentials)
		{
			using (var helper = new S3Helper(credentials))
			{
				helper.PublishDirectory(SourceFolder, DestinationBucket, DestinationFolder, IsPublicRead);
				Log.LogMessage(MessageImportance.Normal, "Published to S3");
			}
		}

		#endregion
	}
}
