namespace SIL.WritingSystems
{
	public class Iso15924Script
	{
		public Iso15924Script(string label, string code)
		{
			Label = label;
			Code = code;
		}

		public string Code { get; private set; }

		public string Label { get; private set; }

		public override string ToString()
		{
			return Label;
		}

		public static int CompareScriptOptions(Iso15924Script x, Iso15924Script y)
		{
			if (x == null)
			{
				if (y == null)
				{
					return 0;
				}
				return -1;
			}
			if (y == null)
			{
				return 1;
			}
			return x.Label.CompareTo(y.Label);
		}

		public string ShortLabel()
		{
			if (!Label.Contains(" ("))
			{
				return Label;
			}
			return Label.Substring(0, Label.IndexOf(" ("));
		}
	}
}
