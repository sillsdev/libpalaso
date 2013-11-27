using System.IO;
using SIL.Archiving.Generic;
using SIL.Archiving.IMDI.Lists;
using SIL.Archiving.IMDI.Schema;

namespace SIL.Archiving.IMDI
{
	/// <summary>A file to add to the archive</summary>
	public class IMDIFile : ArchivingFile
	{
		private string _generalType;
		private string _normalizedName;

		/// <summary>Constructor</summary>
		/// <param name="fullFileNameAndPath"></param>
		public IMDIFile(string fullFileNameAndPath) : base(fullFileNameAndPath)
		{
		}

		/// <summary>Description used in the meta data</summary>
		protected override string GetTypeDescription()
		{
			if (string.IsNullOrEmpty(_generalType))
			{
				var mime = MimeType;

				if (string.IsNullOrEmpty(mime))
					_generalType = "document";
				else if (mime.Contains("x-eaf+xml"))
					_generalType = "annotation";
				else if (mime.StartsWith("image"))
					_generalType = "image";
				else if (mime.StartsWith("audio"))
					_generalType = "audio";
				else if (mime.StartsWith("video"))
					_generalType = "video";
				else if (mime.StartsWith("text"))
					_generalType = "text";
				else
					_generalType = "unspecified";
			}
			return _generalType;
		}

		/// <summary>File name normalized for archive package</summary>
		public string NormalizedName
		{
			get
			{
				if (string.IsNullOrEmpty(_normalizedName))
				{
					_normalizedName = IMDIArchivingDlgViewModel.NormalizeFileName(FileName);
				}
				return _normalizedName;
			}
		}

		/// <summary>Copies the file to the archive package</summary>
		public void Copy(string targetDirectoryName)
		{
			var newFileName = Path.Combine(targetDirectoryName, NormalizedName);
			File.Copy(FullName, newFileName);
		}

		/// <summary>Is this considered a media file</summary>
		public bool IsMediaFile
		{
			get
			{
				switch (GetTypeDescription())
				{
					case "image":
					case "audio":
					case "video":
						return true;

					default:
						return false;
				}
			}
		}

		/// <summary>Is this considered a written resource</summary>
		public bool IsWrittenResource
		{
			get { return !IsMediaFile; }
		}

		/// <summary>Convert to MediaFile_Type for IMDI meta data file</summary>
		public MediaFileType ToMediaFileType(string sessionDirectoryName, string directorySeparator)
		{
			var mediaFile = new MediaFileType();
			SetResourceProperties(mediaFile, sessionDirectoryName, directorySeparator);

			mediaFile.Quality = new QualityType { Type = VocabularyTypeValueType.ClosedVocabulary, Value = "Unspecified" };

			return mediaFile;
		}

		/// <summary>Convert to WrittenResource_Type for IMDI meta data file</summary>
		public WrittenResourceType ToWrittenResourceType(string sessionDirectoryName, string directorySeparator)
		{
			var written = new WrittenResourceType();
			SetResourceProperties(written, sessionDirectoryName, directorySeparator);

			if (!string.IsNullOrEmpty(DescribesAnotherFile))
				written.MediaResourceLink = new ResourceLinkType {
					Value = ResourceLink(sessionDirectoryName,
					directorySeparator,
					IMDIArchivingDlgViewModel.NormalizeFileName(DescribesAnotherFile))
				};

			return written;
		}

		/// <summary>Properly format the full name of the resource in the metadata file</summary>
		/// <param name="sessionDirectoryName"></param>
		/// <param name="directorySeparator"></param>
		/// <param name="fileName"></param>
		/// <returns></returns>
		private static string ResourceLink(string sessionDirectoryName, string directorySeparator, string fileName)
		{
			if (string.IsNullOrEmpty(sessionDirectoryName) || string.IsNullOrEmpty(directorySeparator))
				return fileName;

			return sessionDirectoryName + directorySeparator + fileName;
		}

		/// <summary>Sets values common to both media and written resources</summary>
		/// <param name="resource"></param>
		/// <param name="sessionDirectoryName"></param>
		/// <param name="directorySeparator"></param>
		private void SetResourceProperties(IIMDISessionFile resource, string sessionDirectoryName, string directorySeparator)
		{
			resource.ResourceLink = new ResourceLinkType { Value = ResourceLink(sessionDirectoryName, directorySeparator, NormalizedName) };
			resource.Format.SetValue(MimeType, false, ListType.Link(ListType.MediaFileFormat));
			resource.Type.SetValue(GeneralType, true, ListType.Link(ListType.MediaFileType));
			resource.Size = FileSize;

			foreach (var description in Descriptions)
				resource.Description.Add(description.ToIMDIDescriptionType());

			if (AccessProtocol != null)
			{
				resource.Access = new AccessType();
				resource.Access.Availability = AccessProtocol.GetAccessCode();
			}
		}
	}
}
