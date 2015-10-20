using System;
using System.Runtime.InteropServices;
using System.Text;

namespace SIL.Linux.Logging
{
	/// <summary>
	/// Functions useful in calling C code that expects UTF-8 string parameters
	/// </summary>
	public static class MarshallingHelper
	{
		/// <summary>
		/// Marshal a string to UTF-8, with a null terminating byte (a '\0' character)
		/// </summary>
		/// <param name="s">C# string to marshal to a UTF-8 encoded</param>
		/// <returns>IntPtr handle to the null-terminated UTF-8 string</returns>
		public static IntPtr MarshalStringToUtf8WithNullTerminator(string s)
		{
			// Can't use Marshal.StringToHGlobalAnsi as it's hardcoded to use the current codepage (which will mangle UTF-8)
			byte[] encodedBytes = GetUtf8BytesWithNullTerminator(s);
			IntPtr handle = Marshal.AllocHGlobal(encodedBytes.Length);
			Marshal.Copy(encodedBytes, 0, handle, encodedBytes.Length);
			return handle;
		}

		/// <summary>
		/// Free a marshalled handle if (and only if) it hasn't been freed before.
		/// Enforces the "only free a handle once" by setting it to IntPtr.Zero after
		/// freeing it, and requiring callers to pass the handle as a ref parameter so
		/// that they know it's going to be set to IntPtr.Zero.
		/// </summary>
		/// <param name="marshalledHandle"></param>
		public static void SafelyDisposeOfMarshalledHandle(ref IntPtr marshalledHandle)
		{
			if (marshalledHandle != IntPtr.Zero)
				Marshal.FreeHGlobal(marshalledHandle);
			marshalledHandle = IntPtr.Zero;
		}

		/// <summary>
		/// Convert a string to UTF-8 with a null terminating byte (a '\0' character) and no BOM.
		/// </summary>
		/// <param name="s">The string to encode</param>
		/// <returns>Byte array containing the UTF-8 encoding of s, with a \0 terminator</returns>
		public static byte[] GetUtf8BytesWithNullTerminator(string s)
		{
			UTF8Encoding utf8 = new UTF8Encoding(false);
			int byteCount = utf8.GetByteCount(s) + 1; // Need 1 extra byte for the '\0' terminator
			byte[] encodedBytes = new byte[byteCount];
			encodedBytes[byteCount - 1] = 0; // Not actually needed since C# zero-fills allocated memory, but be safe anyway
			utf8.GetBytes(s, 0, s.Length, encodedBytes, 0);
			return encodedBytes;
		}
	}
}