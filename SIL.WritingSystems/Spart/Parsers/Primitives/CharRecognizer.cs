namespace Spart.Parsers.Primitives.Testers
{
	/// <summary>
	/// A Delegate for testing whether a character should be recognized
	/// </summary>
	/// <param name="c">The character to be tested</param>
	/// <returns>true when the character is recognized, false when it is not</returns>
	public delegate bool CharRecognizer(char c);
}
