// Copyright (c) 2025 SIL Global
// Distributable under the terms of the MIT license (http://opensource.org/licenses/MIT).

using System;
using System.Windows.Forms;

namespace SIL.Windows.Forms.Keyboarding
{
	public class RegisterEventArgs : EventArgs
	{
		public RegisterEventArgs(Control control, object eventHandler)
		{
			Control = control;
			EventHandler = eventHandler;
		}

		public Control Control { get; private set; }
		public object EventHandler { get; private set; }
	}
}
