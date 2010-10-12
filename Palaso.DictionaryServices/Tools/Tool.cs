namespace Palaso.DictionaryServices.Tools
{
	/// <summary>
	/// Tools are things which process a dictionary.  They are all exposed via the "Lift Tools"
	/// application (http://projects.palaso.org/projects/show/lifttools), but live here
	/// because other clients may wish to offer them, too.
	/// </summary>
	public abstract class  Tool
	{
		public abstract void Run(string inputLiftPath, string outputLiftPath, Palaso.Progress.LogBox.IProgress progress);
	}
}