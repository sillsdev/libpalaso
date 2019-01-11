namespace SIL.WritingSystems
{
	/// <summary>
	/// The writing system factory interface.
	/// </summary>
	public interface IWritingSystemFactory
	{
		/// <summary>
		/// Creates a new writing system.  Set will need to be called once identifying information
		/// has been changed in order to save it in the store.
		/// </summary>
		WritingSystemDefinition Create();

		/// <summary>
		/// Creates a new writing system using the specified language tag. Set will need to be
		/// called once identifying information has been changed in order to save it in the store.
		/// </summary>
		bool Create(string ietfLanguageTag, out WritingSystemDefinition ws);

		/// <summary>
		/// Creates a duplicate writing system.  Set will need to be called once identifying information
		/// has been changed in order to save it in the store.
		/// </summary>
		WritingSystemDefinition Create(WritingSystemDefinition ws, bool cloneId = false);
	}

	/// <summary>
	/// The generic writing system factory interface.
	/// </summary>
	public interface IWritingSystemFactory<T> : IWritingSystemFactory where T : WritingSystemDefinition
	{
		new T Create();
		bool Create(string ietfLanguageTag, out T ws);
		T Create(T ws, bool cloneId = false);
	}
}
