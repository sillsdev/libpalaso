// Copyright (c) 2010-2025 SIL Global
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.Text;

namespace SIL.Progress
{
	public class StringBuilderProgress : GenericProgress
	{
		private StringBuilder _builder = new StringBuilder();

		public override void WriteMessage(string message, params object[] args)
		{
			try
			{
				_builder.Append("                          ".Substring(0, indent * 2));
				_builder.AppendFormat(message + Environment.NewLine, args);
			}
			catch //in case someone sneaks a { } into a user string, and cause that format to fail
			{
				_builder.Append(message + Environment.NewLine); //better than nothing
			}
		}

		public override void WriteMessageWithColor(string colorName, string message, params object[] args)
		{
			WriteMessage(message,args);
		}

		public string Text => _builder.ToString();

		public override string ToString() => Text;

		public void Clear()
		{
			_builder = new StringBuilder();
		}
	}
}