namespace SIL.UiBindings
{
	public interface ICountGiver
	{
		int Count
		{ get;
		}
	}

	public class NullCountGiver : ICountGiver
	{
		public int Count
		{
			get { return 0; }
		}
	}
}