// Copyright (c) 2010-2025 SIL Global
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.IO;
using JetBrains.Annotations;
using SIL.Extensions;

namespace SIL.Progress
{
	[PublicAPI]
	public class FileLogProgress : GenericProgress
	{
		private readonly string _path;

		public FileLogProgress(string path)
		{
			ShowVerbose = true;
			_path = path;
			if (File.Exists(path))
			{
				File.Delete(path);
			}
		}

		public override void WriteMessage(string message, params object[] args)
		{
			File.AppendAllText(_path, message.FormatWithErrorStringInsteadOfException(args) + Environment.NewLine);
		}
		public override void WriteMessageWithColor(string colorName, string message, params object[] args)
		{
			WriteMessage(message, args);
		}

	}
}

