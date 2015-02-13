namespace SIL.ObjectModel
{
	/// <summary>
	/// A generic, deep cloneable interface.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public interface ICloneable<out T>
	{
		/// <summary>
		/// Deepclone
		/// </summary>
		/// <returns></returns>
		T Clone();
	}
}
