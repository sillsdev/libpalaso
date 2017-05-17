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
		/// <summary>Identification for bundle</summary>
		[XmlAttribute("id")]
		public string Id { get; set; }

		/// <summary>Identification for bundle--only text bundles are currently supported</summary>
		[XmlAttribute("type")]
		public string Type { get; set; }

		/// <summary>Version of the release bundle type</summary>
		[XmlAttribute("typeVersion")]
		public string TypeVersion { get; set; }

		/// <summary>Revision of the data for the entry contained within this bundle</summary>
		[XmlAttribute("revision")]
		public int Revision { get; set; }

		/// <summary>Gets whether bundle is for text</summary>
		public bool IsTextReleaseBundle { get { return Type == "text"; } }
	}

	/// <summary>
	/// This is abstract to enforce Bundle taking a subclass (not allowing DblMetadata). DblMetadata
	/// itself cannot be abstract because in the event of a loading error, we need to deserialize it
	/// to properly report the situation to the user.
	/// </summary>
	public abstract class DblMetadataBase<TL> : DblMetadata, IProjectInfo where TL: DblMetadataLanguage, new()
	{
		/// <summary>
		/// Loads information about a Digital Bible Library bundle from the specified projectFilePath.
		/// </summary>
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
