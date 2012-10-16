//#define USEBYTESASPRIMARY
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
#if USEBYTESASPRIMARY
using Palaso.Extensions;
#endif
using Palaso.Progress;

namespace Palaso.Xml
{
	///<summary>
	/// Responsible to read a file which has a sequence of similar elements
	/// and return byte arrays of each element for further processing.
	///</summary>
	public class FastXmlElementSplitter : IDisposable
	{
		private readonly static Encoding EncUtf8 = Encoding.UTF8;

		private readonly string _pathname;
#if USEBYTESASPRIMARY
		private static readonly Dictionary<byte, bool> EndingWhitespace = new Dictionary<byte, bool>
										{
											{EncUtf8.GetBytes(" ")[0], true},
											{EncUtf8.GetBytes("\t")[0], true},
											{EncUtf8.GetBytes("\r")[0], true},
											{EncUtf8.GetBytes("\n")[0], true}
										};

		private int _startOfRecordsOffset;
		private int _endOfRecordsOffset;
#endif

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="pathname">Pathname of file to process.</param>
		public FastXmlElementSplitter(string pathname)
		{
			if (string.IsNullOrEmpty(pathname))
				throw new ArgumentException("Null or empty string", "pathname");

			if (!File.Exists(pathname))
				throw new FileNotFoundException("File was not found.", "pathname");

			_pathname = pathname;
		}

		///<summary>
		/// Return the second level elements that are in the input file.
		///</summary>
		///<param name="recordMarker">The element name of elements that are children of the main root elment.</param>
		///<returns>A collection of byte arrays of the records.</returns>
		/// <remarks>
		/// <para>
		/// <paramref name="recordMarker"/> should not contain the angle brackets that start/end an xml element.
		/// </para>
		/// <para>
		/// The input file can contain child elements of the main root element, which are not the same elements
		/// as those marked with <paramref name="recordMarker"/>, but if the file has them, they must be before the
		/// other <paramref name="recordMarker"/> elments.
		/// </para>
		/// </remarks>
		/// <exception cref="ArgumentException">
		/// Thrown if <paramref name="recordMarker"/> is null, an empty string, or there are no such records in the file.
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// Thrown if the input file is not xml.
		/// </exception>
		public IEnumerable<byte[]> GetSecondLevelElementBytes(string recordMarker)
		{
#if USEBYTESASPRIMARY
			if (string.IsNullOrEmpty(recordMarker))
				throw new ArgumentException(LogBoxResources.kNullOrEmptyString, "recordMarker");

			var inputBytes = File.ReadAllBytes(_pathname);
			var recordMarkerAsBytes = FormatRecordMarker(recordMarker);
			var openingAngleBracket = EncUtf8.GetBytes("<")[0];

			InitializeOffsets(openingAngleBracket, inputBytes, recordMarkerAsBytes);

			// Find the records.
			var results = new List<byte[]>(inputBytes.Length / 400); // Reasonable guess on size to avoid resizing so much.
			for (var i = _startOfRecordsOffset; i < _endOfRecordsOffset; ++i)
			{
				var endOffset = FindStartOfMainRecordOffset(i + 1, openingAngleBracket, inputBytes, recordMarkerAsBytes);
				// We should have the complete <foo> element now.
				results.Add(inputBytes.SubArray(i, endOffset - i));
				i = endOffset - 1;
			}
			return results;
#else
			return new List<byte[]>(
				GetSecondLevelElementStrings(recordMarker)
					.Select(stringResult => EncUtf8.GetBytes(stringResult)));
#endif
		}

		///<summary>
		/// Return the second level elements that are in the input file.
		///</summary>
		///<param name="recordMarker">The element name of elements that are children of the main root elment.</param>
		///<returns>A collection of strings of the records.</returns>
		/// <remarks>
		/// <para>
		/// <paramref name="recordMarker"/> should not contain the angle brackets that start/end an xml element.
		/// </para>
		/// <para>
		/// The input file can contain child elements of the main root element, which are not the same elements
		/// as those marked with <paramref name="recordMarker"/>, but if the file has them, they must be before the
		/// other <paramref name="recordMarker"/> elments.
		/// </para>
		/// </remarks>
		/// <exception cref="ArgumentException">
		/// Thrown if <paramref name="recordMarker"/> is null, an empty string, or there are no such records in the file.
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// Thrown if the input file is not xml.
		/// </exception>
		public IEnumerable<string> GetSecondLevelElementStrings(string recordMarker)
		{
#if USEBYTESASPRIMARY
			return new List<string>(
				GetSecondLevelElementBytes(recordMarker)
					.Select(byteResult => EncUtf8.GetString(byteResult)));
#else
			bool foundOptionalFirstElement;
			return GetSecondLevelElementStrings(null, recordMarker, out foundOptionalFirstElement);
#endif
		}

		/// <summary>
		/// Return the second level elements that are in the input file, including an optional first element.
		/// </summary>
		public IEnumerable<byte[]> GetSecondLevelElementBytes(string firstElementMarker, string recordMarker, out bool foundOptionalFirstElement)
		{
#if USEBYTESASPRIMARY
			foundOptionalFirstElement = false;
			if (string.IsNullOrEmpty(recordMarker))
				throw new ArgumentException(LogBoxResources.kNullOrEmptyString, "recordMarker");

			// First call gets the regular records.
			var results = new List<byte[]>(GetSecondLevelElementBytes(recordMarker));
			var readerSettings = CanonicalXmlSettings.CreateXmlReaderSettings();
			var textReader = new XmlTextReader(_pathname)
			{
				WhitespaceHandling = WhitespaceHandling.Significant
			};
			using (var reader = XmlReader.Create(textReader, readerSettings))
			{
				// Try to get the optional first element. Prepend it to 'results', if found.
				// But, don't mess with the reglar records here.
				reader.MoveToContent();
				if (reader.Read() && reader.LocalName == firstElementMarker)
				{
					foundOptionalFirstElement = true;
					var optionalElement = reader.ReadOuterXml();
					results.Insert(0, EncUtf8.GetBytes(optionalElement));
				}
			}

			return results;
#else
			return new List<byte[]>(
				GetSecondLevelElementStrings(firstElementMarker, recordMarker, out foundOptionalFirstElement)
					.Select(stringResult => EncUtf8.GetBytes(stringResult)));
#endif
		}

		/// <summary>
		/// Return the second level elements that are in the input file, including an optional first element.
		/// </summary>
		public IEnumerable<string> GetSecondLevelElementStrings(string firstElementMarker, string recordMarker, out bool foundOptionalFirstElement)
		{
#if USEBYTESASPRIMARY
			return new List<string>(
				GetSecondLevelElementBytes(firstElementMarker, recordMarker, out foundOptionalFirstElement)
					.Select(byteResult => EncUtf8.GetString(byteResult)));
#else
			if (string.IsNullOrEmpty(recordMarker))
				throw new ArgumentException("Null or empty string", "recordMarker");

			foundOptionalFirstElement = false;
			var results = new List<string>(25000);
			if (!string.IsNullOrEmpty(firstElementMarker))
				firstElementMarker = firstElementMarker.Replace("<", null).Replace(">", null);
			recordMarker = recordMarker.Replace("<", null).Replace(">", null);
			var readerSettings = CanonicalXmlSettings.CreateXmlReaderSettings();
			var textReader = new XmlTextReader(_pathname)
			{
				WhitespaceHandling = WhitespaceHandling.Significant
			};
			using (var reader = XmlReader.Create(textReader, readerSettings))
			{
				// Try to get the optional first element. Prepend it to 'results', if found.
				reader.MoveToContent();
				var keepReading = reader.Read();
				while (keepReading)
				{
					if (!reader.IsStartElement())
					{
						keepReading = false;
						continue;
					}
					if (reader.LocalName == firstElementMarker)
					{
						foundOptionalFirstElement = true;
						results.Insert(0, reader.ReadOuterXml());
					}
					else if (reader.LocalName == recordMarker)
					{
						results.Add(reader.ReadOuterXml());
					}
					else
					{
						// Prevents an infinite loop.
						var msg = string.Format("Can't find{0} main record with element name '{1}'",
							firstElementMarker == null ? "" : string.Format(" optional element name '{0}' or", firstElementMarker),
							recordMarker);
						throw new ArgumentException(msg);
					}
				}
			}

			return results;
#endif
		}

#if USEBYTESASPRIMARY
		/// <summary>
		/// This method adjusts _startOfRecordsOffset to the offset to the start of the records,
		/// and adjusts _endOfRecordsOffset to the end of the last record.
		/// </summary>
		private void InitializeOffsets(byte openingAngleBracket, byte[] inputBytes, byte[] recordMarkerAsBytes)
		{
			// Find offset for end of records.
			_endOfRecordsOffset = 0;
			for (var i = inputBytes.Length - 1; i >= 0; --i)
			{
				if (inputBytes[i] != openingAngleBracket)
					continue;

				_endOfRecordsOffset = i;
				break;
			}
			if (_endOfRecordsOffset == 0)
				throw new InvalidOperationException("There was no main ending tag in the file.");

			// Find offset for first record.
			_startOfRecordsOffset = FindStartOfMainRecordOffset(0, openingAngleBracket, inputBytes, recordMarkerAsBytes);
			// No. At least in a test (SyncScenarioTests.CanCollaborateOnLift) the ancestor has no elements at all.
			//if (_startOfRecordsOffset == _endOfRecordsOffset)
			//	throw new InvalidOperationException("There was no main starting tag in the file.");
		}

		private int FindStartOfMainRecordOffset(int currentOffset, byte openingAngleBracket, byte[] inputBytes, byte[] recordMarkerAsBytes)
		{
			// NB: It is possible for records to be nested, so be sure to handle this.

			// Need to get the next starting marker, or the main closing tag
			// When the end point is found, call _outputHandler with the current array
			// from 'offset' to 'i' (more or less).
			for (var i = currentOffset; i < _endOfRecordsOffset; ++i)
			{
				var currentByte = inputBytes[i];
				// Need to get the next starting marker, or the main closing tag
				// When the end point is found, call _outputHandler with the current array
				// from 'offset' to 'i' (more or less).

				// Skip quickly over anything that doesn't match even one character.
				if (currentByte != openingAngleBracket)
					continue;

				// Try to match the rest of the marker.
				for (var j = 1; ; j++)
				{
					var current = inputBytes[i + j];
					if (EndingWhitespace.ContainsKey(current))
					{
						// Got it! [Yes, but it may be a nested on that was found.]
						return i;
					}
					if (recordMarkerAsBytes[j] != current)
						break; // no match, resume searching for opening character.
					//if (j != recordMarkerAsBytes.Length - 1)
					//	continue;
				}
			}

			return _endOfRecordsOffset; // Found the end.
		}

		private static byte[] FormatRecordMarker(string recordMarker)
		{
			return EncUtf8.GetBytes("<" + recordMarker.Replace("<", null).Replace(">", null).Trim());
		}
#endif

		~FastXmlElementSplitter()
		{
			Debug.WriteLine("**** FastXmlElementSplitter.Finalizer called ****");
			Dispose(false);
			// The base class finalizer is called automatically.
		}

		public void Dispose()
		{
			Dispose(true);
			// This object will be cleaned up by the Dispose method.
			// Therefore, you should call GC.SupressFinalize to
			// take this object off the finalization queue
			// and prevent finalization code for this object
			// from executing a second time.
			GC.SuppressFinalize(this);
		}

		private bool IsDisposed
		{ get; set; }

		private void Dispose(bool disposing)
		{
			if (IsDisposed)
				return; // Done already, so nothing left to do.

			if (disposing)
			{
			}

			// Dispose unmanaged resources here, whether disposing is true or false.

			// Main data members.

			IsDisposed = true;
		}
	}
}