/*
 * Original code from https://github.com/JohnThomson/Snowcode.S3BuildPublisher
 * but it has been simplified and modified
 * License: Microsoft Public License (Ms-PL)
 */

using System;
using System.IO;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Util;

namespace Palaso.BuildTasks.AWS
{
	/// <summary>
	/// Helper class to connect to Amazon aws S3 and store files.
	/// </summary>
	public class S3Helper : IDisposable
	{
		private bool _disposed;

		#region Constructors
		public S3Helper(AWSCredentials credentials)
		{
			var config = new AmazonS3Config
			{
				ForcePathStyle = true,
				RegionEndpoint = Amazon.RegionEndpoint.USEast1 // todo: this won't work for all clients!
			};
			Client = new AmazonS3Client(credentials, config);
		}

		~S3Helper()
		{
			Dispose(false);
		}

		#endregion

		#region Properties

		protected IAmazonS3 Client
		{
			get;
			set;
		}

		#endregion

		#region Public methods

		/// <summary>
		/// Publish a file to a S3 bucket, in the folder specified, optionally making it publically readable.
		/// </summary>
		public void Publish(string[] files, string bucketName, string folder, bool publicRead, string contentType = null, string contentEncoding = null)
		{
			CreateBucketIfNeeded(bucketName);

			string destinationFolder = GetDestinationFolder(folder);

			StoreFiles(files, bucketName, destinationFolder, publicRead, contentType, contentEncoding);
		}

		/// <summary>
		/// Creates a S3 Bucket.
		/// </summary>
		/// <param name="bucketName"></param>
		private void CreateBucket(string bucketName)
		{
			var request = new PutBucketRequest { BucketName = bucketName };
			Client.PutBucket(request);
		}

		#endregion

		#region Private Methods

		private void CreateBucketIfNeeded(string bucketName)
		{
			if (!AmazonS3Util.DoesS3BucketExist(Client, bucketName))
			{
				CreateBucket(bucketName);
			}
		}

		private void StoreFiles(string[] files, string bucketName, string destinationFolder, bool publicRead,
			string contentType = null, string contentEncoding = null)
		{
			foreach (string file in files)
			{
				// Use the filename as the key (aws filename).
				string key = Path.GetFileName(file);
				StoreFile(file, destinationFolder + key, bucketName, publicRead, contentType, contentEncoding);
			}
		}

		private string GetDestinationFolder(string folder)
		{
			string destinationFolder = folder ?? string.Empty;

			// Append a folder seperator if a folder has been specified without one.
			if (!string.IsNullOrEmpty(destinationFolder) && !destinationFolder.EndsWith("/"))
			{
				destinationFolder += "/";
			}

			return destinationFolder;
		}

		private void StoreFile(string file, string key, string bucketName, bool publicRead, string contentType = null, string contentEncoding = null)
		{
			S3CannedACL acl = publicRead ? S3CannedACL.PublicRead : S3CannedACL.Private;

			var request = new PutObjectRequest() { CannedACL = acl, FilePath = file, BucketName = bucketName, Key = key };
			if (contentType != null) // probably harmless to just set to null, but feels safer not to set at all if not specified.
				request.ContentType = contentType;
			if (contentEncoding != null)
				request.Headers.ContentEncoding = contentEncoding;

			Client.PutObject(request);
		}

		#endregion

		#region Implementation of IDisposable

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (_disposed)
			{
				if (!disposing)
				{
					try
					{
						Client?.Dispose();
					}
					finally
					{
						_disposed = true;
					}
				}
			}
		}

		#endregion

	}
}
