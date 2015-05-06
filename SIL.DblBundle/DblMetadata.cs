using System;
using System.Xml.Serialization;
using SIL.DblBundle.Text;
using SIL.Xml;

namespace SIL.DblBundle
{
	[XmlRoot("DBLMetadata")]
	public class DblMetadata
	{
		[XmlAttribute("id")]
		public string Id { get; set; }

		[XmlAttribute("type")]
		public string Type { get; set; }

		[XmlAttribute("typeVersion")]
		public string TypeVersion { get; set; }

		[XmlAttribute("revision")]
		public int Revision { get; set; }

		public bool IsTextReleaseBundle { get { return Type == "text"; } }
	}

	/// <summary>
	/// This is abstract to enforce Bundle taking a subclass (not allowing DblMetadata). DblMetadata
	/// itself cannot be abstract because in the event of a loading error, we need to deserialize it
	/// to properly report the situation to the user.
	/// </summary>
	public abstract class DblMetadataBase<TL> : DblMetadata where TL: DblMetadataLanguage, new()
	{
		public static T Load<T>(string projectFilePath, out Exception exception) where T : DblMetadataBase<TL>
		{
			var metadata = XmlSerializationHelper.DeserializeFromFile<T>(projectFilePath, out exception);
			metadata.InitializeMetadata();
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
	}
}
