using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Palaso.WritingSystems
{
	/// <summary>
	/// A trivial implementation of the interface.
	/// </summary>
	public class DefaultKeyboardController : IKeyboardController
	{
		public void Activate(IKeyboardDefinition keyboard)
		{
		}

		public IEnumerable<IKeyboardDefinition> AllAvailableKeyboards
		{
			get {return new IKeyboardDefinition[0];}
		}
	}
}
