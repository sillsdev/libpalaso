namespace Palaso.Lift
{
	public class CommonEnumerations
	{
		public enum VisibilitySetting
		{
			Visible,
			ReadOnly,
			NormallyHidden, /*legacy, so the xml parser doesn't choke*/
			Invisible
		} ;
	}
}