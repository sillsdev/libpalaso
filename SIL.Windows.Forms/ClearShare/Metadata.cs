// Copyright (c) 2018-2025 SIL Global
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using SIL.Code;
using SIL.Extensions;
using SIL.IO;
using SIL.Core.ClearShare;
using TagLib;
using TagLib.IFD;
using TagLib.Image;
using TagLib.Xmp;
using static System.String;

namespace SIL.Windows.Forms.ClearShare
{
	/// <summary>
	/// Provides reading and writing of metadata, currently for any file which TagLib can read AND write (images, pdf).
	/// Where multiple metadata formats are in a file (XMP, EXIF, IPTC-IIM), we read the first one we find (that has a non-empty value) and write them all.
	/// Working Group guidelines: http://www.metadataworkinggroup.org/pdf/mwg_guidance.pdf
	///
	/// Microsoft Pro Photo Tools: http://www.microsoft.com/download/en/details.aspx?id=13518
	///
	/// A previous version of this class used exiftool.exe to read and write this data. The exact fields chosen to store each piece of ClearShare metadata
	/// were chosen when working with exiftool as the best matches to the data we want to store; when switching to taglib, the same names were used
	/// as precisely as possible in order to ensure the greatest possible data interchange with exiftool and anything using it, especially programs
	/// using old versions of this library.
	/// Backwards compatibility was achieved for all fields: we can read anything written by the old version of MetaData.
	/// Forwards compatibility was also fully achieved for files with no existing metadata: if the new library is used to add metadata
	/// to a file which previously had none, old versions of this library (and exiftool generally) should be able to read all of it correctly.
	/// There is however one case I haven't been able to fix:
	///   - Add metadata to a file using exiftool (or an old version of this library, or possibly other tools that write EXIF:Copyright)
	///   - Modify the copyright using this new version
	///   - Attempt to read it using the old version.
	/// In that scenario, exiftool continues to find the old copyright notice.
	/// Apparently, in addition to storing it in the XMP dc:rights/default field and (typically) PNG Copyright field, exiftool stores it
	/// in yet another tag, which taglib does not support, at least for PNG files. Running exiftool with arguments -a -u - args -g
	/// indicates that the unchanged version is in  EXIF:Copyright. And if this value is present, it is what ExifTool (8.5.6.0) returns
	/// when it is simply asked for Copyright, even though the new value is stored in two other copyright fields.
	/// </summary>
	public class Metadata : MetadataCore
	{
		public static Metadata FromFile(string path)
		{
			var m = new Metadata() { _path = path };
			LoadProperties(path, m);
			return m;
		}

		public new Metadata DeepCopy()
		{
			return (Metadata)CloneObject(this);
		}

		/*/// <summary>
		/// Saves all the metadata that fits in XMP to a file.
		/// </summary>
		/// <example>SaveXmplFile("c:\dir\metadata.xmp")</example>
		public new void SaveXmpFile(string path)
		{
			var tag = new XmpTag();
			SaveInImageTag(tag);
			RobustFile.WriteAllText(path, tag.Render(), Encoding.UTF8);
		}*/

		/// <summary>
		/// Loads all metadata found in the XMP file.
		/// </summary>
		/// <example>LoadXmpFile("c:\dir\metadata.xmp")</example>
		public override void LoadXmpFile(string path)
		{
			if (!RobustFile.Exists(path))
				throw new FileNotFoundException(path);

			var xmp = new XmpTag(RobustFile.ReadAllText(path, Encoding.UTF8), null);
			LoadProperties(xmp, this);
		}

		/// <summary>
		/// NB: this is used in 2 places; one is loading from the image we are linked to, the other from a sample image we are copying metadata from
		/// </summary>
		/// <param name="path"></param>
		/// <param name="destinationMetadata"></param>
		private static void LoadProperties(string path, Metadata destinationMetadata)
		{
			try
			{
				destinationMetadata.ExceptionCaughtWhileLoading = null;
				destinationMetadata._originalTaglibMetadata = RetryUtility.Retry(() =>
				  TagLib.File.Create(path) as TagLib.Image.File,
				  memo: $"LoadProperties({path})");
			}
			catch (TagLib.UnsupportedFormatException ex)
			{
				// TagLib throws this exception when the file doesn't have any metadata, sigh.
				// So since I don't see a way to differentiate between that case and the case
				// where something really is wrong, we're just gonna have to swallow this,
				// even in DEBUG mode, because else a lot of simple image tests fail
				destinationMetadata.ExceptionCaughtWhileLoading = ex;
				return;
			}
			catch (NotImplementedException ex)
			{
				// TagLib throws this exception if it encounters (private?) metadata that it doesn't
				// understand.  This prevents us from even looking at images that have such metadata,
				// which seems unreasonable.  Other packages like MetadataExtractor don't have this
				// problem, but have other limitations.
				// See https://issues.bloomlibrary.org/youtrack/issue/BL-8706 for a user complaint.
				System.Diagnostics.Debug.WriteLine($"TagLib exception: {ex}");
				destinationMetadata.ExceptionCaughtWhileLoading = ex;
				return;
			}
			catch (ArgumentOutOfRangeException ex)
			{
				// TagLib can throw this if it can't read some part of the metadata.  This
				// prevents us from even looking at images that have such metadata, which
				// seems unreasonable. (TagLib doesn't fully understand IPTC profiles, for
				// example, which can lead to this exception.)
				// See https://issues.bloomlibrary.org/youtrack/issue/BL-11933 for a user complaint.
				System.Diagnostics.Debug.WriteLine($"TagLib exception: {ex}");
				destinationMetadata.ExceptionCaughtWhileLoading = ex;
				return;
			}
			LoadProperties(destinationMetadata._originalTaglibMetadata.ImageTag, destinationMetadata);
		}

		/// <summary>
		/// Load the properties of the specified MetaData object from the specified ImageTag.
		/// tagMain may be a CombinedImageTag (when working with a real image file) or an XmpTag (when working with an XMP file).
		/// Most of the data is read simply from the XmpTag (which is the Xmp property of the combined tag, if it is not tagMain itself).
		/// But, we don't want to pass combinedTag.Xmp when working with a file, because some files may have CopyRightNotice or Creator
		/// stored (only) in some other tag;
		/// and we need to handle the case where we only have an XmpTag, because there appears to be no way to create a
		/// combinedTag that just has an XmpTag inside it (or indeed any way to create any combinedTag except as part of
		/// reading a real image file).
		/// </summary>
		/// <remarks>
		/// internal to allow unit testing
		/// </remarks>
		internal static void LoadProperties(ImageTag tagMain, Metadata destinationMetadata)
		{
			destinationMetadata.CopyrightNotice = tagMain.Copyright;
			destinationMetadata.Creator = tagMain.Creator;
			XmpTag xmpTag = tagMain as XmpTag ?? ((CombinedImageTag)tagMain).Xmp;
			var licenseProperties = new Dictionary<string, string>();
			if (xmpTag != null)
			{
				destinationMetadata.CollectionUri = xmpTag.GetTextNode(kNsCollections,
					"CollectionURI");
				destinationMetadata.CollectionName = xmpTag.GetTextNode(
					kNsCollections,
					"CollectionName");
				destinationMetadata.AttributionUrl = xmpTag.GetTextNode(kNsCc, "attributionURL");

				var licenseUrl = xmpTag.GetTextNode(kNsCc, "license");
				if (!IsNullOrWhiteSpace(licenseUrl))
					licenseProperties["license"] = licenseUrl;
				var rights = GetRights(xmpTag);
				if (rights != null)
					licenseProperties["rights (en)"] = rights;
			}
			destinationMetadata.License = LicenseWithImageUtils.FromXmp(licenseProperties);

			//NB: we're losing non-ascii somewhere... the copyright symbol is just the most obvious
			if (!IsNullOrEmpty(destinationMetadata.CopyrightNotice))
			{
				destinationMetadata.CopyrightNotice = destinationMetadata.CopyrightNotice.Replace("Copyright �", "Copyright ©");
			}

			//clear out the change-setting we just caused, because as of right now, we are clean with respect to what is on disk, no need to save.
			destinationMetadata.HasChanges = false;
		}
	}
}