// Copyright (c) 2014 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using System;

namespace Palaso.IO
{
	public static class Path
	{

		// On Unix there are more characters valid in file names, but we
		// want the result to be identical on both platforms, so we want
		// to use the larger invalid Windows list for both platforms
		public static char[] GetInvalidOSIndependentFileNameChars()
		{
			return new char[]
			{
				'\0',
				'\u0001',
				'\u0002',
				'\u0003',
				'\u0004',
				'\u0005',
				'\u0006',
				'\a',
				'\b',
				'\t',
				'\n',
				'\v',
				'\f',
				'\r',
				'\u000e',
				'\u000f',
				'\u0010',
				'\u0011',
				'\u0012',
				'\u0013',
				'\u0014',
				'\u0015',
				'\u0016',
				'\u0017',
				'\u0018',
				'\u0019',
				'\u001a',
				'\u001b',
				'\u001c',
				'\u001d',
				'\u001e',
				'\u001f',
				'"',
				'<',
				'>',
				'|',
				':',
				'*',
				'?',
				'\\',
				'/'
			};
		}
	}
}

