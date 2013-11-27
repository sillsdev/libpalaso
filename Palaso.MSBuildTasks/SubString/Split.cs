using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Palaso.BuildTasks.SubString
{
	public class Split : Task
	{
		private int _outputSubString;
		private string _delimiter;
		private string _input;
		private string _returnProperty;
		private int _maxSplit;

		public Split()
		{
			_input = "";
			_delimiter = ":";
			_outputSubString = 0;
			_maxSplit = 999;

			_returnProperty = "";

		}

		[Required]
		public string Input
		{
			get { return _input; }
			set { _input = value; }
		}

		public string Delimiter
		{
			get { return _delimiter.ToString(); }
			set { _delimiter = value; }
		}

		public int OutputSubString
		{
			get { return _outputSubString; }
			set { _outputSubString = value; }
		}

		public int MaxSplit
		{
			get { return _maxSplit; }
			set { _maxSplit = value; }
		}

		[Output]
		public string ReturnValue
		{
			get { return _returnProperty; }
		}

		public override bool Execute()
		{
			bool retval = false;
			string[] result = _input.Split(_delimiter.ToCharArray(), _maxSplit);
			if (_outputSubString < result.Length)
			{
				_returnProperty = result[_outputSubString];
				retval = true;
			}
			return retval;
		}

	}
}