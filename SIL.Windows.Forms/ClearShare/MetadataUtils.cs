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
using SIL.Core.ClearShare;
using SIL.Extensions;
using SIL.IO;
using TagLib;
using TagLib.IFD;
using TagLib.Image;
using TagLib.Xmp;
using static System.String;

namespace SIL.Windows.Forms.ClearShare
{
	public class MetadataUtils
	{
		/// <summary>
		/// Create a MetadataAccess by reading an existing media file
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public static Metadata FromFile(string path)
		{
			var m = new Metadata { MediaFilePath = path };
			LoadProperties(path, m);
			return m;
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
				destinationMetadata.OriginalTaglibMetadata = RetryUtility.Retry(() =>
				  TagLib.File.Create(path) as TagLib.Image.File,
				  memo:$"LoadProperties({path})");
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
			LoadProperties(destinationMetadata.OriginalTaglibMetadata.ImageTag, destinationMetadata);
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
		/// public to allow unit testing
		/// </remarks>
		public static void LoadProperties(ImageTag tagMain, Metadata destinationMetadata)
		{
			destinationMetadata.CopyrightNotice = tagMain.Copyright;
			destinationMetadata.Creator = tagMain.Creator;
			XmpTag xmpTag = tagMain as XmpTag ?? ((CombinedImageTag) tagMain).Xmp;
			var licenseProperties = new Dictionary<string, string>();
			if (xmpTag != null)
			{
				destinationMetadata.CollectionUri = xmpTag.GetTextNode(Metadata.kNsCollections,
					"CollectionURI");
				destinationMetadata.CollectionName = xmpTag.GetTextNode(
					Metadata.kNsCollections,
					"CollectionName");
				destinationMetadata.AttributionUrl = xmpTag.GetTextNode(Metadata.kNsCc, "attributionURL");

				var licenseUrl = xmpTag.GetTextNode(Metadata.kNsCc, "license");
				if (!IsNullOrWhiteSpace(licenseUrl))
					licenseProperties["license"] = licenseUrl;
				var rights = Metadata.GetRights(xmpTag);
				if (rights != null)
					licenseProperties["rights (en)"] = rights;
			}
			destinationMetadata.License = CreativeCommonsLicense.FromXmp(licenseProperties);

			//NB: we're losing non-ascii somewhere... the copyright symbol is just the most obvious
			if (!IsNullOrEmpty(destinationMetadata.CopyrightNotice))
			{
				destinationMetadata.CopyrightNotice = destinationMetadata.CopyrightNotice.Replace("Copyright �", "Copyright ©");
			}

			//clear out the change-setting we just caused, because as of right now, we are clean with respect to what is on disk, no need to save.
			destinationMetadata.HasChanges = false;
		}

		public static void SetupReasonableLicenseDefaultBeforeEditing(Metadata metadata)
		{
			if (metadata.IsLicenseNotSet)
			{
				metadata.License = new CreativeCommonsLicense(true, true, CreativeCommonsLicense.DerivativeRules.Derivatives);
			}
		}

		/// <summary>
		/// Loads all metadata found in the XMP file.
		/// </summary>
		/// <example>LoadXmpFile("c:\dir\metadata.xmp")</example>
		public static void LoadXmpFile(string path, Metadata destinationMetadata)
		{
			if (!RobustFile.Exists(path))
				throw new FileNotFoundException(path);

			var xmp = new XmpTag(RobustFile.ReadAllText(path, Encoding.UTF8), null);
			LoadProperties(xmp, destinationMetadata);
		}

		/// <summary>
		/// Get previously saved values from a file in the user setting.
		/// This is used to quickly populate metadata with the values used in the past (e.g. many images will have the same illustrator, license, etc.)
		/// </summary>
		/// <param name="category">e.g. "image", "document"</param>
		public static void LoadFromStoredExemplar(Metadata.FileCategory category, Metadata destinationMetadata)
		{
			LoadXmpFile(Metadata.GetExemplarPath(category), destinationMetadata);
			destinationMetadata.HasChanges = true;
		}

		/// <summary>
		/// For use on a hyperlink/button
		/// </summary>
		/// <returns></returns>
		public static string GetStoredExemplarSummaryString(Metadata.FileCategory category)
		{
			try
			{
				var m = new Metadata();
				LoadFromStoredExemplar(category, m);
				return $"{m.Creator}/{m.CopyrightNotice}/{m.License}";
			}
			catch (Exception)
			{
				return Empty;
			}
		}
	}
}
