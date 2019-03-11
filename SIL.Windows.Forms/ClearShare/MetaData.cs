// Copyright (c) 2018 SIL International 
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT) 
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using L10NSharp;
using SIL.Extensions;
using TagLib;
using TagLib.IFD;
using TagLib.Image;
using TagLib.Xmp;
using File = System.IO.File;

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
	public class Metadata
	{
		public Metadata()
		{
			IsEmpty = true;
		}

		/// <summary>
		/// Create a MetadataAccess by reading an existing media file
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public static Metadata FromFile(string path)
		{
			var m = new Metadata();
			m._path = path;

			LoadProperties(path, m);
			return m;
		}

		/// <summary>
		/// If the MetaData was loaded from a file, this stores all the other metadata from that file,
		/// which will typically be useful to write to a file derived from it.
		/// </summary>
		TagLib.Image.File _originalTaglibMetadata;

		/// <summary>
		/// NB: this is used in 2 places; one is loading from the image we are linked to, the other from a sample image we are copying metadata from
		/// </summary>
		/// <param name="path"></param>
		/// <param name="destinationMetadata"></param>
		private static void LoadProperties(string path, Metadata destinationMetadata)
		{
			try
			{
				destinationMetadata._originalTaglibMetadata = TagLib.File.Create(path) as TagLib.Image.File;
			}
			catch (TagLib.UnsupportedFormatException)
			{
				// TagLib throws this exception when the file doesn't have any metadata, sigh.
				// So since I don't see a way to differentiate between that case and the case
				// where something really is wrong, we're just gonna have to swallow this,
				// even in DEBUG mode, because else a lot of simple image tests fail
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
		private static void LoadProperties(ImageTag tagMain, Metadata destinationMetadata)
		{
			destinationMetadata.CopyrightNotice = tagMain.Copyright;
			destinationMetadata.Creator = tagMain.Creator;
			XmpTag xmpTag = tagMain as XmpTag;
			if (xmpTag == null)
				xmpTag = ((CombinedImageTag) tagMain).Xmp;
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
				if (!string.IsNullOrWhiteSpace(licenseUrl))
					licenseProperties["license"] = licenseUrl;
				var rights = GetRights(xmpTag);
				if (rights != null)
					licenseProperties["rights (en)"] = rights;
			}
			destinationMetadata.License = LicenseInfo.FromXmp(licenseProperties);

			//NB: we're loosing non-ascii somewhere... the copyright symbol is just the most obvious
			if (!string.IsNullOrEmpty(destinationMetadata.CopyrightNotice))
			{
				destinationMetadata.CopyrightNotice = destinationMetadata.CopyrightNotice.Replace("Copyright �", "Copyright ©");
			}

			//clear out the change-setting we just caused, because as of right now, we are clean with respect to what is on disk, no need to save.
			destinationMetadata.HasChanges = false;
		}

		private LicenseInfo _license;

		///<summary>
		/// 0 or more licenses offered by the copyright holder
		///</summary>
		public LicenseInfo License
		{
			get { return _license; }
			set
			{
				if (value != _license)
					HasChanges = true;
				_license = value;
				if (!(value is NullLicense))
					IsEmpty = false;
			}
		}

		private string _copyrightNotice;
		public string CopyrightNotice
		{
			get { return _copyrightNotice; }
			set
			{
				if (value!=null && value.Trim().Length == 0)
					value = null;
				if (value != _copyrightNotice)
					HasChanges = true;
				_copyrightNotice = FixArtOfReadingCopyrightProblem(value);
				if (!String.IsNullOrEmpty(_copyrightNotice))
					IsEmpty = false;

			}
		}

		/// <summary>
		/// Somehow we shipped art of reading with a copyright which read:
		/// "Copyright, SIL International 2009. This work is licensed under a Creative Commons Attribution-NoDeriv" and so forth.
		/// This trims that off.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		private string FixArtOfReadingCopyrightProblem(string value)
		{
			if(string.IsNullOrEmpty(value))
				return string.Empty;
			var startOfProblem = value.IndexOf("This work");
			if (startOfProblem == -1)
				return value;
			return value.Substring(0, startOfProblem);
		}

		private string _creator;

		/// <summary>
		/// Use this for artist, photographer, company, whatever.  It is mapped to EXIF:Author and XMP:AttributionName
		/// </summary>
		public string Creator
		{
			get { return _creator; }
			set
			{
				if (value != null && value.Trim().Length == 0)
					value = null;
				if (value != _creator)
					HasChanges = true;
				_creator = value;
				if (!String.IsNullOrEmpty(_creator))
					IsEmpty = false;
			}
		}

		private string _attributionUrl;

		/// <summary>
		/// Use this for the site to link to in attribution.  It is mapped to XMP-Creative Commons--AttributionUrl, but may be used even if you don't have a creative commons license
		/// </summary>
		public string AttributionUrl
		{
			get { return _attributionUrl; }
			set
			{
				if (value != null && value.Trim().Length == 0)
					value = null;
				if (value != _attributionUrl)
					HasChanges = true;
				_attributionUrl = value;
				if (!String.IsNullOrEmpty(_attributionUrl))
					IsEmpty = false;
			}
		}


		private string _collectionName;

		/// <summary>
		/// E.g. "Art of Reading"
		/// </summary>
		public string CollectionName
		{
			get { return _collectionName; }
			set
			{
				if (value != null && value.Trim().Length == 0)
					value = null;
				if (value != _collectionName)
					HasChanges = true;
				_collectionName = value;
				if (!String.IsNullOrEmpty(_collectionName))
					IsEmpty = false;

			}
		}

		private string _collectionUri;
		public string CollectionUri
		{
			get { return _collectionUri; }
			set
			{
				if (value != null && value.Trim().Length == 0)
					value = null;
				if (value != _collectionUri)
					HasChanges = true;
				_collectionUri = value;
				if (!String.IsNullOrEmpty(_collectionUri))
					IsEmpty = false;

			}
		}

		/// <summary>
		/// Used during UI editing to know if we should enable the OK button
		/// </summary>
		public bool IsMinimallyComplete
		{
			get
			{
				//I'm thinking, license is secondary. Primary is who holds the copyright, and what year.
				return !String.IsNullOrEmpty(GetCopyrightYear()) && !String.IsNullOrEmpty(GetCopyrightBy());
			}
		}

		private class MetadataAssignement
		{
			public Func<Metadata, string> GetStringFunction { get; set; }
			public Func<Metadata, bool> ShouldSetValue { get; set; }
			public string Switch;
			public string ResultLabel;
			public Action<Metadata, string> AssignmentAction;

			public MetadataAssignement(string Switch, string resultLabel, Action<Metadata, string> assignmentAction, Func<Metadata, string> stringProvider)
				: this(Switch, resultLabel, assignmentAction, stringProvider, p => !String.IsNullOrEmpty(stringProvider(p)))
			{
			}

			public MetadataAssignement(string @switch, string resultLabel, Action<Metadata, string> assignmentAction, Func<Metadata, string> stringProvider, Func<Metadata, bool> shouldSetValueFunction)
			{
				GetStringFunction = stringProvider;
				ShouldSetValue = shouldSetValueFunction;
				Switch = @switch;
				ResultLabel = resultLabel;
				AssignmentAction = assignmentAction;
			}
		}

		private static List<MetadataAssignement> MetadataAssignments
		{
			get
			{
				var assignments = new List<MetadataAssignement>();
				assignments.Add(new MetadataAssignement("-copyright", "copyright", (p, value) => p.CopyrightNotice = value, p => p.CopyrightNotice));

				assignments.Add(new MetadataAssignement("-Author", "Author", (p, value) => p.Creator = value, p => p.Creator));
				assignments.Add(new MetadataAssignement("-XMP:CollectionURI", "Collection URI", (p, value) => p.CollectionUri = value, p => p.CollectionUri));
				assignments.Add(new MetadataAssignement("-XMP:CollectionName", "Collection Name", (p, value) => p.CollectionName = value, p => p.CollectionName));
				assignments.Add(new MetadataAssignement("-XMP-cc:AttributionURL", "Attribution URL", (p, value) => p.AttributionUrl = value, p => p.AttributionUrl));
				assignments.Add(new MetadataAssignement("-XMP-cc:License", "license",
													   (p, value) => { },//p.License=LicenseInfo.FromUrl(value), //we need to use for all the properties to set up the license
													   p => p.License.Url, p => p.License !=null));
				//NB: CC also has a custom one, for adding rights beyond the normal. THat's not what this is (at least right now). This is for custom licenses.
				assignments.Add(new MetadataAssignement("-XMP-dc:Rights-en", "Rights (en)",
													   (p, value) => { },//p.License=LicenseInfo.FromUrl(value), //we need to use for all the properties to set up the license
													   p => p.License.RightsStatement, p => p.License != null));

				return assignments;
			}
		}

		public bool IsEmpty { get; private set; }

		private bool _hasChanges;
		public bool HasChanges
		{
			get
			{
				return _hasChanges || (License!=null && License.HasChanges);
			}
			set
			{
				_hasChanges = value;
				if(!value && License!=null)
					License.HasChanges = false;
			}
		}

		/// <summary>
		/// Attempt to cut off notices like "Copyright Blah 2009. Blah blah blah"
		/// </summary>
		public string ShortCopyrightNotice
		{
			get {
				var i = CopyrightNotice.IndexOf('.');
				if(i<0)
					return CopyrightNotice;
				return CopyrightNotice.Substring(0, i);
			}
		}

		private string _path;
		private static Encoding _commandLineEncoding = Encoding.UTF8;

		public void Write()
		{
			Write(_path);
		}

		/// <summary>Returns if the format of the image file supports metadata</summary>
		public bool FileFormatSupportsMetadata(string path)
		{
			var file = TagLib.File.Create(path) as TagLib.Image.File;
			return file != null && !file.GetType().FullName.Contains("NoMetadata");
		}

		/// <summary>
		/// Set standard orientation on any metadata saved from the original image (typically because we hard-rotated the image)
		/// </summary>
		public void NormalizeOrientation()
		{
			if (_originalTaglibMetadata == null)
				return;
			var ifdTag = _originalTaglibMetadata.GetTag(TagTypes.TiffIFD) as IFDTag;
			if (ifdTag != null)
				ifdTag.Orientation = ImageOrientation.TopLeft;
		}

		/// <summary>
		/// Write the metadata to the specified image file. By default, if this MetaData was made from a file,
		/// any other original metadata from that file is copied to the new one. This can include all kinds
		/// of details like where and when a photo was taken, image size, and so forth, so it should not be
		/// done except for copies of the image; for example, not when applying the same ClearShare settings
		/// to a group of images. Thought should also be given to copying to a modified version of the image;
		/// for example, some of the metadata may be incorrect if the image being saved has a different
		/// size or format from the original file from which the metadata was loaded.
		/// </summary>
		/// <param name="path"></param>
		/// <param name="copyAllMetaDataFromOriginal"></param>
		public void Write(string path, bool copyAllMetaDataFromOriginal = true)
		{
			// do not attempt to add metadata to a file type that does not support it.
			if (!FileFormatSupportsMetadata(path))
				throw new NotSupportedException(String.Format("The image file {0} is in a format that does not support metadata.", Path.GetFileName(path)));

			var file = TagLib.File.Create(path) as TagLib.Image.File;

			file.GetTag(TagTypes.XMP, true); // The Xmp tag, at least, must exist so we can store properties into it.
			// This does nothing if the file is not allowed to have PNG tags, that is, if it's not a PNG file.
			// If it is, we want this tag to exist, since otherwise tools like exiftool (and hence old versions
			// of this library and its clients) won't see our copyright notice and creator, at least.
			file.GetTag(TagTypes.Png, true);
			// If we know where the image came from, copy as much metadata as we can to the new image.
			if (copyAllMetaDataFromOriginal && _originalTaglibMetadata != null)
			{
				file.CopyFrom(_originalTaglibMetadata);
			}
			SaveInImageTag(file.ImageTag);
			file.Save();
			//as of right now, we are clean with respect to what is on disk, no need to save.
			HasChanges = false;
		}

		/// <summary>
		/// Write just the ClearShare License information to the specified image. This is appropriate
		/// when e.g. duplicating license info to a collection of images (things like the place and time
		/// where a photo was taken should not be copied to all of them).
		/// </summary>
		/// <param name="path"></param>
		public void WriteIntellectualPropertyOnly(string path)
		{
			Write(path, false);
		}

		public void SetupReasonableLicenseDefaultBeforeEditing()
		{
			if(License == null || License is NullLicense)
			{
				License= new CreativeCommonsLicense(true,true,CreativeCommonsLicense.DerivativeRules.Derivatives);
			}
		}

		public Metadata DeepCopy()
		{
			return (Metadata) CloneObject(this);
		}

		/// <summary>
		/// This is actually a generic cloning method, copied into this class just because UI needs to clone this class.
		/// From http://stackoverflow.com/questions/78536/cloning-objects-in-c-sharp/78551#78551
		/// </summary>
		/// <param name="source"></param>
		/// <returns></returns>
		private static object CloneObject(object source)
		{
			//grab the type and create a new instance of that type
			Type sourceType = source.GetType();
			//  var target =  Activator.CreateInstance(source.GetType(), null);
			object target = Activator.CreateInstance(sourceType, true);

			//grab the properties
			PropertyInfo[] PropertyInfo =
				sourceType.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

			//iterate over the properties and if it has a 'set' method assign it from the source TO the target
			foreach (PropertyInfo item in PropertyInfo)
			{
				if (item.CanWrite)
				{
					//value types can simply be 'set'
					if (item.PropertyType.IsValueType || item.PropertyType.IsEnum ||
						item.PropertyType.Equals(typeof(System.String)))
					{
						item.SetValue(target, item.GetValue(source, null), null);
					}
						//object/complex types need to recursively call this method until the end of the tree is reached
					else
					{
						object propertyValue = item.GetValue(source, null);
						if (propertyValue == null)
						{
							item.SetValue(target, null, null);
						}
						else
						{
							Type z = item.PropertyType;
							item.SetValue(target, CloneObject(propertyValue), null);
							//item.SetValue(target, CloneObject<LicenseInfo>(propertyValue as LicenseInfo), null);
						}
					}
				}
			}
			//return the new item
			return target;
		}

		/// <summary>
		/// Update the value of the specified node, or create it.
		/// Seems SetTextNode should work whether or not is already exists, but in some cases it doesn't.
		/// </summary>
		/// <param name="tag"></param>
		/// <param name="ns"></param>
		/// <param name="name"></param>
		/// <param name="val"></param>
		void AddOrModify(XmpTag tag, string ns, string name, string val)
		{
			var node = tag.FindNode(ns, name);
			if (node != null)
				node.Value = val;
			else
				tag.SetTextNode(ns, name, val);
		}

		/// <summary>
		/// Save the properties of this in tagMain in a suitable form for writing to a file which we can read and LoadProperties from
		/// to recover the current state of this object.
		/// tagMain may be a CombinedImageTag (when working with a real image file) or an XmpTag (when working with an XMP file).
		/// Most of the data is stored simply in the XmpTag (which is the Xmp property of the combined tag, if it is not tagMain itself).
		/// But, we don't want to pass combinedTag.Xmp when working with a file, because setting CopyRightNotice and Creator directly
		/// on the combinedTag may save it in additional places that may be useful;
		/// and we need to handle the case where we only have an XmpTag, because there appears to be no way to create a
		/// combinedTag that just has an XmpTag inside it (or indeed any way to create any combinedTag except as part of
		/// reading a real file).
		/// </summary>
		void SaveInImageTag(ImageTag tagMain)
		{
			// Taglib doesn't care what namespace prefix is used for these namespaces (which it doesn't already know about).
			// It will happily assign them to be ns1 and ns2 and successfully read back the data.
			// However, exiftool and its clients, including older versions of this library, will only recognize the
			// cc data if the namespace has the 'standard' abbreviation.
			// I'm not sure whether the pdf one is necessary, but minimally it makes the xmp more readable and less unusual.
			// This is a bit of a kludge...I'm using a method that TagLib says is only meant for unit tests,
			// and modifying what is meant to be an internal data structure, bypassing the (internal) method normally used
			// to initialize it. But it gets the job done without requiring us to fork taglib.
			XmpTag.NamespacePrefixes["http://creativecommons.org/ns#"] = "cc";
			XmpTag.NamespacePrefixes["http://ns.adobe.com/pdf/1.3/"] = "pdf";

			XmpTag xmp = tagMain as XmpTag;
			if (xmp == null)
				xmp = ((CombinedImageTag) tagMain).Xmp;
			SetCopyright(tagMain, CopyrightNotice);
			tagMain.Creator = Creator;
			AddOrModify(xmp, kNsCollections, "CollectionURI", CollectionUri);
			AddOrModify(xmp, kNsCollections, "CollectionName", CollectionName);
			AddOrModify(xmp, kNsCc, "attributionURL", AttributionUrl);
			if (License != null && !string.IsNullOrWhiteSpace(License.Url))
				AddOrModify(xmp, kNsCc, "license", License.Url);
			SetRights(xmp, License == null ? null : License.RightsStatement);
		}

		/// <summary>
		/// Set the copyright. This is tricky because when we do tagMain.Copyright = value,
		/// this sets the rights:default langauge to that string (as we wish), as well as setting
		/// copyright in any other tag that may be present and support it. We don't want to bypass
		/// setting copyright on other tags, so we need to set it on tagMain, not just do something
		/// to the xmp.
		/// However, taglib clears all other alternatives of rights when it does this.
		/// We don't want that, because it might include our rights statement, which we store in the
		/// English language alternative.
		/// This is probably excessively cautious for right now, since the only client of this method
		/// sets rights AFTER setting copyright; but I wanted a method that would be safe for any
		/// future use.
		/// (Though...it will need enhancing if we store yet more information in yet other alternatives.)
		/// </summary>
		/// <param name="tagMain"></param>
		/// <param name="copyright"></param>
		void SetCopyright(ImageTag tagMain, string copyright)
		{
			XmpTag xmp = tagMain as XmpTag;
			if (xmp == null)
				xmp = ((CombinedImageTag)tagMain).Xmp;
			var oldRights = GetRights(xmp);
			tagMain.Copyright = copyright;
			if (oldRights != null)
				SetRights(xmp, oldRights);
		}

		void SetRights(XmpTag xmp, string rights)
		{
			var rightsNode = xmp.FindNode("http://purl.org/dc/elements/1.1/", "rights");
			if (rightsNode == null)
			{
				if (string.IsNullOrEmpty(rights))
					return; // leave it missing.
				// No existing rights node, and we have some. We use (default lang) rights for copyright too, and there seems to be no way to
				// make the base node without setting that. So set it to something meaningless.
				// This will typically never happen, since our dialog requires a non-empty copyright.
				// I'm not entirely happy with it, but as far as I can discover the current version of taglib cannot
				// set the 'en' alternative of dc:rights without setting the  default alternative. In fact, I'm not sure the
				// result of doing so would technically be valid xmp; the standard calls for every langauge alternation
				// to have a default.
				xmp.SetLangAltNode("http://purl.org/dc/elements/1.1/", "rights", "Unknown");
				rightsNode = xmp.FindNode("http://purl.org/dc/elements/1.1/", "rights");
			}
			foreach (var child in rightsNode.Children)
			{
				if (child.Namespace == "http://www.w3.org/1999/02/22-rdf-syntax-ns#" && child.Name == "li" &&
					HasLangQualifier(child, "en"))
				{
					if (string.IsNullOrEmpty(rights))
					{
						rightsNode.RemoveChild(child);
						// enhance: possibly we should remove rightsNode, if it now has no children, and if taglib can.
						// However, we always require a copyright, so this will typically not happen.
					}
					else
						child.Value = rights;
					return;
				}
			}
			// Didn't find an existing rights:en node.
			if (string.IsNullOrEmpty(rights))
				return; // leave it missing.
			var childNode = new XmpNode(XmpTag.RDF_NS, "li", rights);
			childNode.AddQualifier(new XmpNode(XmpTag.XML_NS, "lang", "en"));

			rightsNode.AddChild(childNode);
		}

		static bool HasLangQualifier(XmpNode node, string lang)
		{
			var qualifier = node.GetQualifier(XmpTag.XML_NS, "lang");
			return qualifier != null && qualifier.Value == lang;
		}

		static string GetRights(XmpTag xmp)
		{
			var rightsNode = xmp.FindNode("http://purl.org/dc/elements/1.1/", "rights");
			if (rightsNode == null)
				return null;
			foreach (var child in rightsNode.Children)
			{
				if (child.Namespace == "http://www.w3.org/1999/02/22-rdf-syntax-ns#" && child.Name == "li" &&
					HasLangQualifier(child, "en"))
				{
					return child.Value;
				}
			}
			return null;
		}

		/// <summary>
		/// Saves all the metadata that fits in XMP to a file.
		/// </summary>
		/// <example>SaveXmplFile("c:\dir\metadata.xmp")</example>
		public void SaveXmpFile(string path)
		{
			var tag = new XmpTag();
			SaveInImageTag(tag);
			File.WriteAllText(path, tag.Render(), Encoding.UTF8);
		}

		/// <summary>
		/// Loads all metadata found in the XMP file.
		/// </summary>
		/// <example>LoadXmplFile("c:\dir\metadata.xmp")</example>
		public void LoadXmpFile(string path)
		{
			if(!File.Exists(path))
				throw new FileNotFoundException(path);

			var xmp = new XmpTag(File.ReadAllText(path, Encoding.UTF8), null);
			LoadProperties(xmp, this);
		}

		/// <summary>
		/// Save the current metadata in the user settings, so that in the future, a call to LoadFromStoredExamplar() will retrieve them.
		/// This is used to quickly populate metadata with the values used in the past (e.g. many images will have the same illustruator, license, etc.)
		/// </summary>
		/// <param name="category">e.g. "image", "document"</param>
		public void StoreAsExemplar(FileCategory category)
		{
			SaveXmpFile(GetExemplarPath(category));
		}

		/// <summary>
		/// Get previously saved values from a file in the user setting.
		/// This is used to quickly populate metadata with the values used in the past (e.g. many images will have the same illustruator, license, etc.)
		/// </summary>
		/// <param name="category">e.g. "image", "document"</param>
		public void LoadFromStoredExemplar(FileCategory category)
		{
			LoadXmpFile(GetExemplarPath(category));
			HasChanges = true;
		}

		/// <summary>
		/// Tell if there is previously saved values from a file in the user setting.
		/// </summary>
		/// <param name="category">e.g. "image", "document"</param>
		static public bool HaveStoredExemplar(FileCategory category)
		{
			return File.Exists(GetExemplarPath(category));
		}

		/// <summary>
		/// Deletes the stored exemplar (if it exists).  This can be useful if a program
		/// wants to establish "CC BY" as the default license for a new product.
		/// </summary>
		static public void DeleteStoredExemplar(FileCategory category)
		{
			var path = GetExemplarPath(category);
			if (File.Exists(path))
				File.Delete(path);
		}

		/// <summary>
		/// Gets a text combining the creator, copyright, and license
		/// </summary>
		/// <param name="idealIso639LanguageCode">e.g. "en" or "fr"</param>
		/// <returns></returns>
		[Obsolete("Instead, use the version with a prioritized list")]
		public string GetSummaryParagraph(string idealIso639LanguageCode)
		{
			string idOfLanguageUsed;
			return GetSummaryParagraph(new string[] { idealIso639LanguageCode }, out idOfLanguageUsed);
		}

		/// <summary>
		/// Gets a text combining the creator, copyright, and license
		/// </summary>
		/// <remarks>It's possible to get parts in multiple languages if some parts have been localized, and other parts haven't</remarks>
		/// <param name="languagePriorityIds">The summary will be in the first language available.</param>
		/// <param name="idOfLanguageUsed"></param>
		/// <returns></returns>
		public string GetSummaryParagraph(IEnumerable<string> languagePriorityIds, out string idOfLanguageUsed) 
		{
			var b = new StringBuilder();
			string creatorLabel = LocalizationManager.GetString("MetadataDisplay.CreatorLabel", "Creator");
			b.AppendLine(creatorLabel+": " + Creator);
			b.AppendLine(CopyrightNotice);
			if(!string.IsNullOrEmpty(CollectionName))
				b.AppendLine(CollectionName);
			if (!string.IsNullOrEmpty(CollectionUri))
				b.AppendLine(CollectionUri);
			var description = License.GetDescription(languagePriorityIds, out idOfLanguageUsed);
			// BL-4243, Description for CC usually contains the url already.
			if (!string.IsNullOrEmpty(License.Url) && !description.Contains(License.Url))
				b.AppendLine(License.Url);
			b.AppendLine(description);
			if (!String.IsNullOrWhiteSpace(License.RightsStatement))
				b.AppendLine(License.RightsStatement);
			return b.ToString();
		}

		/// <summary>
		/// For use on a hyperlink/button
		/// </summary>
		/// <returns></returns>
		static public string GetStoredExemplarSummaryString(FileCategory category)
		{
			try
			{
				var m = new Metadata();
				m.LoadFromStoredExemplar(category);
				return string.Format("{0}/{1}/{2}", m.Creator, m.CopyrightNotice, m.License.ToString());
			}
			catch (Exception)
			{
				return string.Empty;
			}
		}

		private static string GetExemplarPath(FileCategory category)
		{
			String appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
			var path = appData.CombineForPath("palaso");
			if(!Directory.Exists(path))
			{
				Directory.CreateDirectory(path);
			}
			path = path.CombineForPath("rememberedMetadata-" + Enum.GetName(typeof(FileCategory), category) + ".xmp");
			return path;
		}

		/// <summary>
		/// used when storing/retrieving exemplar metadata
		/// </summary>
		public enum FileCategory
		{
			Audio,
			Image,
			Document
		};

		public void SetCopyrightNotice(string year, string by)
		{
			if ((License is CreativeCommonsLicense) && !((CreativeCommonsLicense) License).AttributionRequired)
			{
				// Public Domain, no copyright as such.
				if (!string.IsNullOrEmpty(year))
					CopyrightNotice = by + ", " + year;
				else
					CopyrightNotice = by;
				return;
			}
			if(!string.IsNullOrEmpty(year))
				CopyrightNotice = "Copyright © " + year + ", " + by;
			else
				CopyrightNotice = "Copyright © " + by;
		}

		const string kCopyrightPattern = @"\D*(?<year>\d\d\d\d)?(,\s)?(?<by>(.|\r?\n)+)?";
		const string kNoYearPattern = @"([cC]opyright,?\s+)?(COPYRIGHT,?\s+)?\©?\s*(?<by>.+)";
		private const string kNsCollections = "http://www.metadataworkinggroup.com/schemas/collections/";
		private const string kNsCc = "http://creativecommons.org/ns#";


		public string GetCopyrightYear()
		{
			if (string.IsNullOrEmpty(CopyrightNotice))
				return string.Empty;
			Match m = Regex.Match(CopyrightNotice, kCopyrightPattern);
			return m.Groups["year"].Value;
		}

		public string GetCopyrightBy()
		{
			if(string.IsNullOrEmpty(CopyrightNotice))
				return string.Empty;
			Match m = Regex.Match(CopyrightNotice, kCopyrightPattern);

			//I was never able to get the cases without years to work with the standard pattern (just not good enough with regex's).
			if (!m.Groups["year"].Success && !m.Groups["by"].Success)
			{
				m = Regex.Match(CopyrightNotice, kNoYearPattern);
			}
			var trimChars = new char[] { ' ', '\t', ',', '.', ':', ';' };
			var by0 = m.Groups["by"].Value.Trim(trimChars);
			if (!String.IsNullOrEmpty(by0))
				return by0;

			// Okay, maybe we can get this by deleting the Copyright word or symbol and the year.
			m = Regex.Match(CopyrightNotice, kNoYearPattern);
			var by1 = m.Groups["by"].Value.Trim();
			if (String.IsNullOrEmpty(by1))
				by1 = CopyrightNotice;
			m = Regex.Match(by1, @"(?<year>\d\d\d\d)");
			if (m.Groups["year"].Success)
				by1 = by1.Replace(m.Groups["year"].Value, "");
			by1 = by1.Trim(trimChars);
			return by1;
		}

		/// <summary>
		/// A super compact form of credits that doesn't introduce any English.
		/// Jane Doe, © 2008 SIL International, CC BY-NC 3.0
		/// "International Illustrations: Art Of Reading", © 2009 SIL International, CC ND
		/// </summary>
		public string MinimalCredits(IEnumerable<string> languagePriorityIds, out string idOfLanguageUsedForLicense)
		{
			var notice = "";
			if (!string.IsNullOrWhiteSpace(Creator))
			{
				notice += string.Format("{0},", Creator);
			}
			if (!string.IsNullOrWhiteSpace(CollectionName))
			{
				notice += string.Format(" {0},", CollectionName);
			}
			if (!string.IsNullOrWhiteSpace(CopyrightNotice))
			{
				notice += string.Format(" © {0} {1}", GetCopyrightYear(), GetCopyrightBy());
			}
			if (License != null)
			{
				var minimalFormForCredits = License.GetMinimalFormForCredits(languagePriorityIds, out idOfLanguageUsedForLicense);
				if (!string.IsNullOrWhiteSpace(minimalFormForCredits))
				{
					notice += string.Format(". {0}", minimalFormForCredits);
				}
			}
			else
			{
				idOfLanguageUsedForLicense = "*";
			}

			return notice.Trim();
		}
	}
}
