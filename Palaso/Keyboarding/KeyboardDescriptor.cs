using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Palaso.Keyboarding
{
	public enum Engines
		{
			None = 0,
			Windows = 1,
			Keyman6 = 2,
			Keyman7 = 4,
			Scim = 8,
			IBus = 16,
			All = 255
		} ;

		public class KeyboardDescriptor
		{
			private string _keyboardName;
			private Engines _keyboardingEngine;
			private string _id;

			public KeyboardDescriptor(string keyboardName, Engines keyboardEngine, string id)
			{
				_keyboardName = keyboardName;
				_keyboardingEngine = keyboardEngine;
				_id = id;
			}

			public string KeyboardName
			{
				get { return _keyboardName; }
			}

			public Engines KeyboardingEngine
			{
				get { return _keyboardingEngine; }
			}

			public string Id
			{
				get { return _id; }
			}
		}
}
