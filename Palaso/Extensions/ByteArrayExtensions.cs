using System;

namespace Palaso.Extensions
{
	public static class ByteArrayExtensions
	{
		/// <summary>
		/// Like string.IndexOf, returns the place where the subsequence occurs (or -1).
		/// Throws if source or target is null or target is empty.
		/// </summary>
		public static int IndexOfSubArray(this byte[] source, byte[] target)
		{
			var first = target[0];
			var targetLength = target.Length;
			if (targetLength == 1)
				return Array.IndexOf(source, first); // probably more efficient, and code below won't work.
			var lastStartPosition = source.Length - targetLength;
			for (var i = 0; i <= lastStartPosition; i++)
			{
				if (source[i] != first)
					continue;
				for (var j = 1; j < targetLength; j++)
					if (source[i + j] != target[j])
						break;
					else if (j == targetLength - 1)
						return i;
			}
			return -1;
		}

		/// <summary>
		/// Return the subarray from start for count items.
		/// </summary>
		public static byte[] SubArray(this byte[] source, int start, int count)
		{
			var realCount = Math.Min(count, source.Length - start);
			var result = new byte[realCount];
			Array.Copy(source, start, result, 0, realCount);
			return result;
		}

		/// <summary>
		/// Return 'true', if the two byte arrays are the same exact array, or contain the same bytes.
		/// Otherwise, return 'false'.
		/// </summary>
		public static bool AreByteArraysEqual(this byte[] source, byte[] target)
		{
			if (source == null || target == null || source.Length != target.Length)
				return false; // Tested: VariationsOnANullAreNeverEqual & DifferentLengthArraysAreNotEqual
			if (source == target) // Reference identity/equality
				return true; // Tested: SameIdenticalByteArrayAreEqual
			for (int i = 0; i < target.Length; i++)
			{
				if (source[i] != target[i])
					return false; // Tested: SameLengthButDifferentContentArraysAreNotEqual
			}
			return true; // Tested: SameContentByteArrayAreEqual
		}
	}
}