using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using Palaso.Extensions;
using Palaso.Progress;

namespace Palaso.Xml
{
	///<summary>
	/// Responsible to read a file which has a sequence of similar elements
	/// and return byte arrays of each element for further processing.
	/// This version works entirely with byte arrays, except for the GetSecondLevelElementStrings
	/// methods, which should only be used for testing or small files.
	///
	/// Enhance JohnT: This version still uses twice as much memory as necessary, in that it
	/// loads the whole file into an array as well as making a byte array for each chunk.
	/// It could be enhanced so that we never load the whole file.
	/// One approach would be to replace every occurrence of _inputBytes[x] with a method call,
	/// InputByte(n). Then we write an implementation of that which reads a chunk of the file
	/// into a buffer. It can answer immediately if the current index is in the buffer, otherwise, reload it.
	/// If we kept two buffers we would scarcely ever read the same section of the file twice.
	///
	/// Enhance JohnT: this is now smart enough to match markers. It could easily be enhanced
	/// to make the recordMarker parameter optional, and return all second-level elements of any type.
	///</summary>
	public class FastXmlElementSplitter : IDisposable
	{
		private readonly static Encoding EncUtf8 = Encoding.UTF8;
		private readonly static byte _openingAngleBracket = EncUtf8.GetBytes("<")[0];
		private readonly static byte _closingAngleBracket = EncUtf8.GetBytes(">")[0];
		private readonly static byte _slash = EncUtf8.GetBytes("/")[0];
		private readonly static byte _hyphen = EncUtf8.GetBytes("-")[0];
		private static AsyncFileReader _input;
		private static byte[] cdataStart = EncUtf8.GetBytes("![CDATA[");
		private static byte[] cdataEnd = EncUtf8.GetBytes("]]>");
		private static byte[] startComment = EncUtf8.GetBytes("!--");
		private static byte[] endComment = EncUtf8.GetBytes("-->");

		private readonly string _pathname;

		/// <summary>
		/// Characters that may follow a marker and indicate the end of it (for a successful match).
		/// </summary>
		private static readonly HashSet<byte> Terminators = new HashSet<byte>
										{
											EncUtf8.GetBytes(" ")[0],
											EncUtf8.GetBytes("\t")[0],
											EncUtf8.GetBytes("\r")[0],
											EncUtf8.GetBytes("\n")[0],
											EncUtf8.GetBytes(">")[0],
											EncUtf8.GetBytes("/")[0]
									};

		private int _currentOffset; // Position in the file as we work through it.
		// index of the angle bracket of the final, closing top-level marker.
		// In the special case where the top-level marker has no children or close tag (<top/>),
		// it may be the start of the whole top-level tag.
		private int _endOfRecordsOffset;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="pathname">Pathname of file to process.</param>
		public FastXmlElementSplitter(string pathname)
		{
			if (string.IsNullOrEmpty(pathname)) throw new ArgumentException("Null or empty string", "pathname");

			if (!File.Exists(pathname)) throw new FileNotFoundException("File was not found.", "pathname");

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
		/// The input file can contain one child element of the main root element, before the elements
		/// marked with <paramref name="recordMarker"/>, but if the file has them, you must use another overload
		/// and specify the marker for that optional element.
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
			return GetSecondLevelElementBytes(null, recordMarker);
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
		/// The input file can contain one child elements of the main root element before this, but if so, you must
		/// call the appropriate overload and specify the first element marker.
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
			bool foundOptionalFirstElement;
			return GetSecondLevelElementStrings(null, recordMarker, out foundOptionalFirstElement);
		}

		/// <summary>
		/// Return the second level elements that are in the input file, including an optional first element.
		/// Provides a boolean to indicate whether the optional first element was found.
		/// Note that the overload without this out param is an iterator and can return the results
		/// without creating a collection of all of them.
		/// </summary>
		public List<byte[]> GetSecondLevelElementBytes(string firstElementMarker, string recordMarker,
			out bool foundOptionalFirstElement)
		{
			var results = new List<byte[]>(GetSecondLevelElementBytes(firstElementMarker, recordMarker));
			foundOptionalFirstElement = false;
			if (results.Count == 0 || string.IsNullOrEmpty(firstElementMarker))
				return results;
			var first = results[0];
			int i = 0;
			// advance to marker
			for (; i < first.Length; i++)
			{
				var c = Convert.ToChar(first[i]);
				if (!Char.IsWhiteSpace(c) && c != '<')
					break;
			}
			// Set foundOptionalFirstElement if the marker matches and is terminated.
			if (i + firstElementMarker.Length + 1 > first.Length)
				return results;
			for (int j = 0; j < firstElementMarker.Length; j++)
			{
				if (first[i + j] != firstElementMarker[j])
					return results; // no match, didn't get first element
			}
			foundOptionalFirstElement = Terminators.Contains(first[i + firstElementMarker.Length]);
			return results;
		}


		/// <summary>
		/// Return the second level elements that are in the input file, including an optional first element.
		/// </summary>
		public IEnumerable<byte[]> GetSecondLevelElementBytes(string firstElementMarker, string recordMarker)
		{
			if (string.IsNullOrEmpty(recordMarker)) throw new ArgumentException("Null or empty string", "recordMarker");
			_input = new AsyncFileReader(_pathname);
			InitializeOffsets();
			var recordMarkerAsBytes = FormatRecordMarker(recordMarker);
			if (firstElementMarker != null && _currentOffset < _endOfRecordsOffset)
			{
				var firstElementMarkerAsBytes = FormatRecordMarker(firstElementMarker);
				if (MatchMarker(firstElementMarkerAsBytes))
				{
					yield return MakeElement(firstElementMarkerAsBytes);
					AdvanceToOpenAngleBracket();
				}
			}

			// We've processed the special first element if any and should be at the first record marker (or end of file)
			while (_currentOffset < _endOfRecordsOffset)
			{
				if (!MatchMarker(recordMarkerAsBytes))
				{
					var msg = string.Format("Can't find{0} main record with element name '{1}'",
						firstElementMarker == null ? "" : string.Format(" optional element name '{0}' or", firstElementMarker),
						recordMarker);
					throw new ArgumentException(msg);
				}
				yield return MakeElement(recordMarkerAsBytes);
				AdvanceToOpenAngleBracket();
			}
			_input.Close();
			_input = null;
		}

		/// <summary>
		/// Given that _currentInput is at the first character of the marker of the opening XML tag
		/// of an element, return the byte array that is the complete element.
		/// </summary>
		/// <param name="firstElementMarkerAsBytes"></param>
		/// <returns></returns>
		private byte[] MakeElement(byte[] marker)
		{
			int start = _currentOffset - 1; // including the opening angle bracket
			int depth = 1; // How many unmatched opening marker elements have we seen?
			while (true)
			{
				bool gotOpenBracket = AdvanceToAngleBracket();
				if (_currentOffset > _endOfRecordsOffset)
					throw new ArgumentException("Unmatched opening tag " + marker);
				// We have to distinguish these cases:
				// 1. <x... : depth++ (start of opening marker)
				// 2: ...> : no change (end of opening or closing marker)
				// 3: </x... : depth-- (closing marker)
				// 4: .../> : depth-- (end of open marker that has no close marker)
				// 5: <![CDATA[ : advance to matching ]]>
				// 6: <!-- : depth++
				// 7: --> : depth--
				// If depth is zero we must stop and output. In case 3 we must also check for the correct closing marker.
				if (gotOpenBracket)
				{
					if (_input[_currentOffset] == _slash)
						depth--; // case 3
					else if (Match(cdataStart))
					{
						_currentOffset += cdataStart.Length;
						while (_currentOffset <= _endOfRecordsOffset - cdataEnd.Length && !Match(cdataEnd))
							_currentOffset++;
						continue;
					}
					else
					{
						depth++; // case 1 (or 6)
					}
				}
				else
				{
					if (_input[_currentOffset - 2] == _slash)
						depth--; // case 4
					else if (_input[_currentOffset - 2] == _hyphen && _input[_currentOffset - 3] == _hyphen)
						depth--; // case 6
					// otherwise case 2, do nothing.
				}
				if (depth == 0)
				{
					if (gotOpenBracket)
					{
						// case 3, advance past opening slash
						_currentOffset++;
						if (!MatchMarker(marker))
							throw new ArgumentException("Unmatched opening tag " + marker); // wrong close marker
						AdvanceToClosingAngleBracket();
					}
					// We matched the marker we started with, output the chunk.
					if (_input[_currentOffset - 1] != _closingAngleBracket)
						throw new ArgumentException("Unmatched opening tag " + marker);
					return _input.SubArray(start, _currentOffset - start);
				}
				if (_currentOffset == _endOfRecordsOffset)
					throw new ArgumentException("Unmatched opening tag " + marker);
			}
		}

		/// <summary>
		/// Return true if the bytes starting at _currentPosition match the specified marker.
		/// The following character must not be part of a marker name.
		///
		/// Enhance JohnT: technically I think there could be white space between the opening angle bracket and the
		/// marker. We can make that enhancement if we need it.
		/// </summary>
		/// <param name="marker"></param>
		/// <returns></returns>
		bool MatchMarker(byte[] marker)
		{
			if (!Match(marker)) return false;
			return Terminators.Contains(_input[_currentOffset + marker.Length]);
		}

		/// <summary>
		/// Return true if the bytes starting at _currentPosition match the specified marker. No termination is required.
		/// </summary>
		/// <param name="marker"></param>
		/// <returns></returns>
		private bool Match(byte[] marker)
		{
			if (_currentOffset + marker.Length >= _endOfRecordsOffset)
				return false;
			for (int i = 0; i < marker.Length; i++)
			{
				if (_input[_currentOffset + i] != marker[i])
					return false;
			}
			return true;
		}

		/// <summary>
		/// Return the second level elements that are in the input file, including an optional first element.
		/// </summary>
		public IEnumerable<string> GetSecondLevelElementStrings(string firstElementMarker, string recordMarker, out bool foundOptionalFirstElement)
		{
			return new List<string>(
				GetSecondLevelElementBytes(firstElementMarker, recordMarker, out foundOptionalFirstElement)
					.Select(byteResult => EncUtf8.GetString(byteResult)));
		}

		/// <summary>
		/// This method adjusts _currentPosition to the offset of the first record,
		/// and adjusts _endOfRecordsOffset to the end of the last record.
		/// If there are no records they will end up equal, at the index of the last open angle bracket
		/// in the file. Throws exception if there is no top-level element in the file.
		/// </summary>
		private void InitializeOffsets()
		{
			// Find offset for end of records.
			_endOfRecordsOffset = -1;
			for (var i = _input.Length - 1; i >= 0; --i)
			{
				if (_input[i] != _openingAngleBracket)
					continue;

				_endOfRecordsOffset = i;
				break;
			}
			if (_endOfRecordsOffset < 0)
				throw new ArgumentException("There was no main ending tag in the file.");

			// Find first angle bracket
			_currentOffset = 0;
			AdvanceToOpenAngleBracket();
			if (_currentOffset < _input.Length && (_input[_currentOffset] == EncUtf8.GetBytes("?")[0]))
			{
				// got an xml declaration. Find the NEXT open bracket. This will be the root element.
				AdvanceToOpenAngleBracket();
			}

			if (_currentOffset >= _endOfRecordsOffset)
			{
				// There is only one opening angle bracket in the file, except possibly the xml declaration.
				// This is valid in just one case, when the root element ends />
				bool gotClose = false;
				for (var i = _input.Length - 1; i > _endOfRecordsOffset; --i)
				{
					var c = Convert.ToChar(_input[i]);
					if (Char.IsWhiteSpace(c))
						continue;
					if (gotClose)
					{
						if (c == '/')
							return; // OK, empty file, except for the self-terminating root.
					}
					else
					{
						if (c == '>')
						{
							gotClose = true;
							continue; // now we need the slash
						}
					}
					break; // got some invalid sequence at end of file
				}
				throw new ArgumentException("There was no main starting tag in the file.");
			}
			// Find the NEXT open bracket after the root. This should be the first second-level element.
			AdvanceToOpenAngleBracket();
			// It's OK for this to be equal to _endOfRecordsOffset. Just means a basically empty file, with no second-level elements.
		}

		/// <summary>
		/// Move _currentOffset to the character following the next angle bracket of either type, or to _endOfRecordOffset if that comes first.
		/// Answer true if what we got was an opening bracket
		/// </summary>
		private bool AdvanceToAngleBracket()
		{
			for (; _currentOffset < _endOfRecordsOffset; _currentOffset++)
			{
				var inputByte = _input[_currentOffset];
				if (inputByte == _openingAngleBracket)
				{
					_currentOffset++;
					return true;
				}
				if (inputByte == _closingAngleBracket)
				{
					_currentOffset++;
					return false;
				}
			}
			return _input[_currentOffset] == _openingAngleBracket;
		}

		/// <summary>
		/// Move _currentOffset to the character following the next angle bracket, or to _endOfRecordOffset if that comes first.
		/// Skip comments.
		/// </summary>
		private void AdvanceToOpenAngleBracket()
		{
			for (; _currentOffset < _endOfRecordsOffset;_currentOffset++)
			{
				if (_input[_currentOffset] == _openingAngleBracket)
				{
					_currentOffset++;
					if (Match(startComment))
					{
						_currentOffset += startComment.Length;
						while (_currentOffset + endComment.Length < _endOfRecordsOffset && !Match(endComment))
							_currentOffset++;
						continue;
					}
					return;
				}
			}
		}

		/// <summary>
		/// Move _currentOffset to the character following the next closing angle bracket, or to _endOfRecordOffset if that comes first.
		/// </summary>
		private void AdvanceToClosingAngleBracket()
		{
			for (; _currentOffset < _endOfRecordsOffset; _currentOffset++)
			{
				if (_input[_currentOffset] == _closingAngleBracket)
				{
					_currentOffset++;
					return;
				}
			}
		}

		private static byte[] FormatRecordMarker(string recordMarker)
		{
			return EncUtf8.GetBytes(recordMarker.Replace("<", null).Replace(">", null).Trim());
		}

		~FastXmlElementSplitter()
		{
			Debug.WriteLine("**** FastXmlElementSplitter.Finalizer called ****");
			Dispose(false);
			// The base class finalizer is called automatically.
		}

		// Enhance JohnT: there appears to be no reason for this class to be Disposable.
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
				if (_input != null)
					_input.Close();
				_input = null;
			}

			// Dispose unmanaged resources here, whether disposing is true or false.

			// Main data members.

			IsDisposed = true;
		}
	}

	/// <summary>
	/// Class responsible to manage access to the content of a file. The interface is mostly a subset of byte array,
	/// and in general it replaces input = File.ReadAllBytes(pathname) with input = new AsyncFileReader(pathname),
	/// and input can be used much like the array. One difference is that the AsyncFileReader should be closed.
	/// The benefit of using this is that it is not necessary to allocate an array as big as the file.
	/// Also, if the file is mainly accessed sequentially, some of the data reading will be overlapped with
	/// processing it, at least for a large file.
	/// </summary>
	internal class AsyncFileReader
	{
		private string _pathname;
		// MS doc says this is the smallest buffer that will produce real async reads.
		// We want them relatively small so we can start overlapping quickly.
		// For testing this class, kbufLen is set very small.
		// Otherwise the unit tests will just load everything into the first 65K buffer, and much of the logic
		// will never be tried.
		internal static int kbufLen = 65536;
		private const int kBufferCount = 3;
		private int _currentBuffer; // index of a buffer (no longer Loading) where we last found a desired byte. -1 if none.
		// The typical use of this class is reading through a file, noting a start position, reading to an end position,
		// and then making a byte array out of what is in between. Thus, it is basically a sequential read, with an occasional,
		// typically short, jump backwards.
		// Using three buffers allows us to have
		//	- one we are reading through (_currentBuffer),
		//  - one we are in the process of loading (asynchronously, while processing the previous one...this is typically
		//		the 'next' buffer in the rotation after _currentBuffer)
		//  - the one we read through last, which we keep around so that stepping back to get the chunk we identified
		//		does not require us to re-read anything (unless a chunk is really huge).
		Buffer[] buffers = new Buffer[kBufferCount];
		private FileStream m_reader;

		public AsyncFileReader(string pathName)
		{
			_pathname = pathName;
			Length = (int)new FileInfo(_pathname).Length;
			// Setting a high offset ensures that no byte we want will be in any buffer's range.
			for (int i = 0; i < kBufferCount; i++)
				buffers[i] = new Buffer() {Data = new byte[kbufLen], Offset = Length};
			_currentBuffer = -1; // no buffer is loaded
			m_reader = new FileStream(_pathname, FileMode.Open, FileAccess.Read, FileShare.None, 4096, true);
			// Read a block at the end of the file, which we happen to know we will want first.
			StartReading(Math.Max(Length - kbufLen, 0));
		}

		public void Close()
		{
			// MS Doc says EndRead must be called exactly once for each BeginRead. Not sure whether anything
			// very bad will happen if we don't finish the final reads, but just in case...
			for (int i = 0; i < kBufferCount; i++)
			{
				if (buffers[i].Loading)
					m_reader.EndRead(buffers[i].Token);
			}
			m_reader.Close();
		}

		// Start reading a block at the specified (byte) index.
		// Return the index of the block we are loading it into.
		int StartReading(int index)
		{
			// Load the new data into the buffer after the current one, leaving the buffer before it holding
			// hopefully more recent data.
			int bufIndex = NextBuffer(_currentBuffer);
			var buffer = buffers[bufIndex];
			// Have to finish the current read, even though we will then overwrite the data.
			// It should be very unusual, if it can happen at all, that the next buffer needs loading.
			FinishRead(buffer);
			// Start an asynchronous read. When something actually needs the data from this buffer,
			// it must call EndRead(buffer.Token) and then set Loading to false.
			m_reader.Seek(index, SeekOrigin.Begin);
			buffer.Token = m_reader.BeginRead(buffer.Data, 0, buffer.Data.Length, null, null);
			buffer.Offset = index;
			buffer.Loading = true;
			return bufIndex;
		}

		private void FinishRead(Buffer buffer)
		{
			if (!buffer.Loading)
				return; // already loaded.
			m_reader.EndRead(buffer.Token);
			buffer.Loading = false;
		}

		/// <summary>
		/// This makes the class function like a byte array, allowing [n] to get the nth byte.
		/// </summary>
		/// <param name="n"></param>
		/// <returns></returns>
		public byte this[int n]
		{
			get
			{
				Buffer currentBuffer;
				// Try to get it from the current buffer.
				if (_currentBuffer >= 0)
				{
					currentBuffer = buffers[_currentBuffer];
					if (n >= currentBuffer.Offset && n < currentBuffer.Offset + kbufLen)
						return currentBuffer.Data[n - currentBuffer.Offset];
				}
				for (int i = 0; i < kBufferCount; i++)
				{
					if (i == _currentBuffer)
						continue;
					currentBuffer = buffers[i];
					if (n >= currentBuffer.Offset && n < currentBuffer.Offset + kbufLen)
					{
						// We have a buffer that covers the relevant part of the file.
						FinishRead(currentBuffer);
						int prevBuffer = _currentBuffer;
						_currentBuffer = i;
						// If we advanced from one block to the following one, go ahead and start loading the next, if any.
						// For typical sequential access, this should overlap the reading with processing the block
						// we just finished loading.
						// If we went back to a previous block, probably in order to get a SubArray that extends into
						// the current one, we want to leave the current block intact.
						if (_currentBuffer == NextBuffer(prevBuffer) && currentBuffer.Offset + kbufLen < Length)
							StartReading(currentBuffer.Offset + kbufLen);
						return currentBuffer.Data[n - currentBuffer.Offset];
					}
				}
				// no current buffer has it. This is usually because we are going back to get a SubArray
				// up to the present location, and had to back up more than one buffer.
				// We may want a few bytes before the start of the sub-array, but mainly we will want following bytes.
				_currentBuffer = StartReading(Math.Max(n - 3, 0));
				currentBuffer = buffers[_currentBuffer];
				FinishRead(currentBuffer);
				// Start reading the NEXT block; hopefully it will be ready by the time we need it.
				if (currentBuffer.Offset + kbufLen < Length)
					StartReading(currentBuffer.Offset + kbufLen);
				return currentBuffer.Data[n - currentBuffer.Offset];
			}
		}

		int NextBuffer(int index)
		{
			return index + 1 < kBufferCount ? index + 1 : 0;  // 0 initially, when _currentBuffer is -1.
		}

		/// <summary>
		/// Initialized to length of file in constructor.
		/// </summary>
		public int Length { get; private set; }

		public byte[] SubArray(int start, int count)
		{
			var result = new byte[count];
			// Enhance JohnT: this could be optimized to copy chunks of buffers.
			for (int i = 0; i < count; i++)
				result[i] = this[i + start];
			return result;
		}
	}

	class Buffer
	{
		public bool Loading; // true if we're still loading data for it.
		public byte[] Data; // bytes we read from the file
		public int Offset; // from start of whole file (or beyond end, if we haven't started loading buffer)
		public IAsyncResult Token; // from BeginRead; should be passed to EndRead when we need the data.
	}
}