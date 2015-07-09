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
		WritingSystemDefinition Create(string ietfLanguageTag);

		/// <summary>
		/// Creates a duplicate writing system.  Set will need to be called once identifying information
		/// has been changed in order to save it in the store.
		/// </summary>
		WritingSystemDefinition Create(WritingSystemDefinition ws);
	}

	/// <summary>
	/// The generic writing system factory interface.
	/// </summary>
	public interface IWritingSystemFactory<T> : IWritingSystemFactory where T : WritingSystemDefinition
	{
		new T Create();
		new T Create(string ietfLanguageTag);
		T Create(T ws);
	}
}
