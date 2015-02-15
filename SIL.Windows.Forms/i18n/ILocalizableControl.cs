namespace SIL.Windows.Forms.i18n
{
	/// <summary>
	/// A control can implement this to override some default localization behavior
	/// </summary>
	public interface ILocalizableControl
	{
		bool ShouldModifyFont { get;}
		void BeginWiring();
		void EndWiring();
	}
}
