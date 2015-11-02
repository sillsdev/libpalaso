namespace SIL.WritingSystems
{
	/// <summary>
	/// This interface is used to read and write writing system properties to a
	/// custom data format (i.e. an application-specific settings file).
	/// </summary>
	public interface ICustomDataMapper<in T> where T : WritingSystemDefinition
	{
		void Read(T ws);

		void Write(T ws);

		void Remove(string wsId);
	}
}
