using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Palaso.WritingSystems
{
	public class Rfc5646SubtagParser
	{
		private enum State
		{
			StartParse,
			StartGetNextParse,
			Parsing,
			EndGetNextPart,
			EndParse,
			ExpectingPotentialDashAsPartOfExtension
		}

		private string _stringToparse;
		private State _state;
		private int _position;
		static private string[] _seperators = new string[] { "-" };

		private char _currentCharacter
		{
			get{ return _stringToparse[_position];}
		}

		public Rfc5646SubtagParser(string stringToParse)
		{
			_stringToparse = stringToParse;
			_state = State.StartParse;
		}

		private string GetNextPart()
		{
			StringBuilder sb = new StringBuilder();
			bool atBeginningOfString = _position == 0;
			_state = State.StartGetNextParse;
			while (_state != State.EndGetNextPart && _state != State.EndParse)
			{
				switch (_state)
				{
					case State.StartParse:
						sb.Append(_currentCharacter);
						_state = State.Parsing;
						break;
					case State.Parsing:
						if (CurrentCharacterIsSeperator && _state != State.ExpectingPotentialDashAsPartOfExtension)
						{
							_state = State.EndGetNextPart;
							break;
						}
						CheckIfCharacterIsAphaNumericorSeperator();

						sb.Append(_currentCharacter);
						break;
					//case State.ExpectingPotentialDashAsPartOfExtension:
					//    if(_currentCharacter == '_'){throw new ArgumentException("Extensions may only have \"-\" as their separator.");}
					//    sb.Append(_currentCharacter);
					//    _state = State.Parsing;
					//    break;
					case State.StartGetNextParse:
						if (CurrentCharacterIsSeperator)
						{
							if (atBeginningOfString) {break; }
							sb.Append(_currentCharacter);
							_state = State.EndGetNextPart;
						}
						//else if(CurrentCharacterIsExtensionMarker())
						//{
						//    sb.Append(_currentCharacter);
						//    _state = State.ExpectingPotentialDashAsPartOfExtension;
						//}
						else
						{
							CheckIfCharacterIsAphaNumericorSeperator();
							sb.Append(_currentCharacter);
							_state = State.Parsing;
						}
						break;
					default:
						throw new ApplicationException();
				}

				if (ReachedEndOfString)
				{
					_state = State.EndParse;
				}
				else {
					_position++;
				}
			}
			return sb.ToString();
		}

		private void CheckIfCharacterIsAphaNumericorSeperator()
		{
			if(!char.IsLetter(_currentCharacter) && !char.IsDigit(_currentCharacter) && !CurrentCharacterIsSeperator)
			{
				throw new ArgumentException(
					String.Format("Languagetags may only consist of alphanumeric characters and '-'. {0} is not a valid character.", _currentCharacter));
			}
		}

		private bool CurrentCharacterIsExtensionMarker()
		{
			return _currentCharacter == 'x' || _currentCharacter == 'X';
		}

		private bool ReachedEndOfString
		{
			get { return _position == _stringToparse.Length - 1; }
		}

		public List<string> GetParts()
		{
			List<string> parts = new List<string>();
			if (String.IsNullOrEmpty(_stringToparse)) { return parts; }
			if (StringIsSeperator(_stringToparse)) { return parts; }
			do
			{
				parts.Add(GetNextPart());
			} while (_state != State.EndParse);
			RemoveTrailingSeperators(parts);
			return parts;
		}

		private void RemoveTrailingSeperators(List<string> parts)
		{
			foreach (string seperator in _seperators)
			{
				if (parts.Last() == seperator)
				{
					parts.RemoveAt(parts.Count-1);
				}
			}
		}

		private bool CurrentCharacterIsSeperator
		{
			get
			{
				return StringIsSeperator(_currentCharacter.ToString());
			}
		}

		static public bool StringIsSeperator(string stringToCheck)
		{
			foreach (string c in _seperators)
			{
				if (c == stringToCheck)
				{
					return true;
				}
			}
			return false;
		}
	}
}
