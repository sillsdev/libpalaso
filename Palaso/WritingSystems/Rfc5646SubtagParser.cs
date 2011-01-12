using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Palaso.WritingSystems
{
	public class Rfc5646SubtagParser
	{
		private enum State
		{
			Start,
			End,
			BuildingExtension,
			FoundSeperator
		}

		private string _stringToparse;
		private State _state;
		private int _position;
		char[] seperators = new char[] { '-', '_' };

		private char _currentCharacter
		{
			get{ return _stringToparse[_position];}
		}

		public Rfc5646SubtagParser(string stringToParse)
		{
			_stringToparse = stringToParse;
			_state = State.Start;
		}

		private string GetNextPart()
		{
			StringBuilder sb = new StringBuilder();
			bool reachedEndOfString = (_position != _stringToparse.Length-1);
			switch(_state)
			{
				case State.Start:
					if(CurrentCharacterIsSeperator)
					{
						sb.Append(_currentCharacter);
					}
					_state = State.End;
					break;
				case State.End:
					return sb.ToString();

				default: throw new ApplicationException();
			}
			throw new ApplicationException("The parser should leave this method through the State.End case.");
		}

		public List<string> Getparts()
		{
			List<String> parts = new List<string>();
			while(_state != State.End)
			{
				parts.Add(GetNextPart());
			}
			return parts;
		}

		private bool CurrentCharacterIsSeperator
		{
			get
			{
				bool characterIsSeperator = false;
				foreach (char c in seperators)
				{
					characterIsSeperator = c == _currentCharacter ? false : true;
				}
				return characterIsSeperator;
			}
		}
	}
}
