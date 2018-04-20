// Copyright (c) 2018 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System.IO;
using System.Text.RegularExpressions;

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
	}
}
