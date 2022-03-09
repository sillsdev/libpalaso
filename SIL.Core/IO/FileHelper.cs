// Copyright (c) 2018 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml.Serialization;

namespace SIL.IO
{
	public static class FileHelper
	{
		public static bool IsLocked(string filePath)
		{
			if (filePath == null || !File.Exists(filePath))
				return false;

			try
			{
				File.OpenWrite(filePath).Close();
				return false;
			}
			catch
			{
				return true;
			}
		}

		public static bool Grep(string inputPath, string pattern)
		{
			Regex regex = new Regex(pattern, RegexOptions.Compiled);

			using (StreamReader reader = File.OpenText(inputPath))
			{
				while (!reader.EndOfStream)
				{
					if (regex.IsMatch(reader.ReadLine()))
					{
						return true;
					}
				}
			}
			return false;
		}

		/// <summary>
		/// Serialize an object directly to disk, avoiding the operating system cache.
		/// 
		/// This method is intended to prevent the problem with null files being generated
		/// as reported here: LT-20651, LT-20333, LTB-3915, LTB-3916, and LTB-3917
		/// The theory is that FieldWorks is closing and power is lost before the operating
		/// system cache is written to disk. This method is intended to prevent that problem
		/// by avoiding the cache and writing directly to disk.
		///
		/// The disadvantage of this method is that it is slow; so it should only be used to
		/// write relatively small files that are not frequently written.
		/// </summary>
		/// <param name="objToSerialize">Object to be serialized to the file.</param>
		/// <param name="path">The full path (containing the file name and extension).</param>
		public static void WriteXmlFileDirectlyToDisk(object objToSerialize, string path)
		{
			try
			{
				// Note: Using FileOptions.WriteThrough causes the data to still be written to the
				//       operating system cache but it is immediately flushed. If this doesn't address
				//       the problem then a more thorough solution that completely bypasses the cache is
				//       to use the c++ CreateFile() api and pass both FILE_FLAG_NO_BUFFERING and
				//       FILE_FLAG_WRITE_THROUGH.
				//       https://docs.microsoft.com/en-us/windows/win32/fileio/file-caching
				using (var writer = new FileStream(path, FileMode.Create,
					FileAccess.Write, FileShare.None,
					4096, FileOptions.WriteThrough))
				{
					XmlSerializer xmlSerializer = new XmlSerializer(objToSerialize.GetType());
					xmlSerializer.Serialize(writer, objToSerialize);
				}
			}
			catch (Exception err)
			{
				throw new ApplicationException("There was a problem saving your xml file.", err);
			}
		}
	}
}
