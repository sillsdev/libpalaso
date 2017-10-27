using System;
using System.Xml.Serialization;
using SIL.DblBundle.Text;
using SIL.Xml;

namespace SIL.DblBundle
{
	/// <summary>
	/// Information about a Digital Bible Library bundle
	/// </summary>
	[XmlRoot("DBLMetadata")]
	public class DblMetadata
	{
		private DblMetadataType _type;

		[XmlAttribute("id")]
		public string Id { get; set; }

		/// <summary>
		/// Type of DBL bundle.
		/// Only text bundles are currently supported.
		/// Deprecated as of metadata version 2.
		/// </summary>
		[XmlAttribute("type")]
		public string Type_DeprecatedXml
		{
			get { return null; }
			set {
				if (_type == null)
					_type = new DblMetadataType();
				_type.Medium = value;
			}
		}

		/// <summary>
		/// This is required to prevent a breaking change to the API after deprecating the type attribute above.
		/// </summary>
		[XmlIgnore]
		public string Type
		{
			get { return _type?.Medium; }
			set
			{
				if (_type == null)
					_type = new DblMetadataType();
				_type.Medium = value;
			}
		}

		/// <summary>
		/// Type of DBL bundle.
		/// Only text bundles are currently supported.
		/// Replaced the type attribute as of metadata version 2.
		/// </summary>
		[XmlElement("type")]
		public DblMetadataType TypeElement
		{
			get { return _type; }
			set
			{
				// This is a hack to prevent the deserializer from setting
				// a blank DblMetadataType when we already have one with content
				// (set from Type_DeprecatedXml).
				// Surprisingly, the deserializer will call set with a newly
				// constructed DblMetadataType even if no type element exists.
				// This seems to be only because the potential attribute and
				// the potential element have the same name ("type").
				if (value?.Medium != null && _type?.Medium == null)
					_type = value;
			}
		}
		[XmlAttribute("typeVersion")]
		public string TypeVersion { get; set; }

		[XmlAttribute("revision")]
		public int Revision { get; set; }

		public bool IsTextReleaseBundle { get { return Type == "text"; } }
	}

	/// <summary>
	/// Contains the type information for DBL Metadata
	/// </summary>
	public class DblMetadataType
	{
		/// <summary>
		/// Type medium, e.g. text or audio
		/// </summary>
		[XmlElement("medium")]
		public string Medium { get; set; }
	}

	/// <summary>
	/// This is abstract to enforce Bundle taking a subclass (not allowing DblMetadata). DblMetadata
	/// itself cannot be abstract because in the event of a loading error, we need to deserialize it
	/// to properly report the situation to the user.
	/// </summary>
	public abstract class DblMetadataBase<TL> : DblMetadata, IProjectInfo where TL: DblMetadataLanguage, new()
	{
		public static T Load<T>(string projectFilePath, out Exception exception) where T : DblMetadataBase<TL>
		{
			var metadata = XmlSerializationHelper.DeserializeFromFile<T>(projectFilePath, out exception);
			if (metadata == null)
			{
				if (exception == null)
					exception = new ApplicationException(string.Format("Loading metadata ({0}) was unsuccessful.", projectFilePath));
				return null;
			}
			try
			{
				metadata.InitializeMetadata();
			}
			catch (Exception e)
			{
				exception = e;
			}
			return metadata;
		}

		/// <summary>
		/// Can be overridden to provide sub-class-specific behavior on load
		/// </summary>
		protected virtual void InitializeMetadata()
		{
		}

		/// <summary>
		/// The language element of the metadata. Can be any class of type DblMetadataLangauge.
		/// </summary>
		[XmlElement("language")]
		public virtual TL Language { get; set; }

		#region IProjectInfo Members
		public abstract string Name { get; }

		DblMetadataLanguage IProjectInfo.Language
		{
			get { return Language; }
		}
		#endregion
	}
}
