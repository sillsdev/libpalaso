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
			StartParse,
			StartGetNextParse,
			Parsing,
			EndGetNextPart
		}

		private List<string> _parts = new List<string>();
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
			_state = State.StartParse;
			while (!ReachedEndOfString)
			{
				_parts.Add(GetNextPart());
			}
		}

		private string GetNextPart()
		{
			StringBuilder sb = new StringBuilder();
			bool atBeginningOfString = _position == 0;
			_state = State.StartGetNextParse;
			while (_state != State.EndGetNextPart)
			{
				switch (_state)
				{
					case State.StartParse:
						sb.Append(_currentCharacter);
						_state = State.Parsing;
						break;
					case State.Parsing:
						sb.Append(_currentCharacter);
						break;
					case State.StartGetNextParse:
						if (CurrentCharacterIsSeperator)
						{
							if (atBeginningOfString) { throw new ArgumentException(); }
							sb.Append(_currentCharacter);
							_state = State.EndGetNextPart;
						}
						else
						{
							sb.Append(_currentCharacter);
							_state = State.Parsing;
						}
						break;
					default:
						throw new ApplicationException();
				}

				if (ReachedEndOfString)
				{
					_state = State.EndGetNextPart;
				}
				else {
					_position++;
					if (CurrentCharacterIsSeperator)
					{
						_state = State.EndGetNextPart;
					}
				}
			}
			return sb.ToString();
		}

		private bool ReachedEndOfString
		{
			get { return _position == _stringToparse.Length - 1; }
		}

		public List<string> GetParts()
		{
			return _parts;
		}

		private bool CurrentCharacterIsSeperator
		{
			get
			{
				bool characterIsSeperator = false;
				foreach (char c in seperators)
				{
					if(c == _currentCharacter)
					{
						return true;
					}
				}
				return false;
			}
		}
	}
}
