using System.Collections.Generic;
using System.Drawing;

namespace SIL.Windows.Forms.ImageGallery
{
	public delegate string CaptionMethodDelegate(string path);

	public interface IImageCollection
	{
		IEnumerable<string> GetMatchingPictures(string keywords, out bool foundExactMatches);

		/// <summary>
		/// The imageTOken here could be a path or whatever, the client doesn't need to know or care
		/// </summary>
		/// <param name="imageToken"></param>
		/// <returns></returns>
		Image GetThumbNail(object imageToken);

		CaptionMethodDelegate CaptionMethod{ get;}

		IEnumerable<string> GetPathsFromResults(IEnumerable<string> results, bool limitToThoseActuallyAvailable);

		IEnumerable<string> IndexLanguageIds { get; }

		void ReloadImageIndex(string languageId);
	}
}