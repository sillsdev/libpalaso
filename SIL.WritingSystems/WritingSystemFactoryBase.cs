namespace SIL.WritingSystems
{
	public abstract class WritingSystemFactoryBase<T> : IWritingSystemFactory<T> where T : WritingSystemDefinition
	{
		public virtual T Create(string ietfLanguageTag)
		{
			return ConstructDefinition(ietfLanguageTag);
		}

		public virtual T Create(T ws)
		{
			return ConstructDefinition(ws);
		}

		public virtual T Create()
		{
			return ConstructDefinition();
		}

		/// <summary>
		/// Creates an empty writing system. This is implemented by subclasses to allow the use
		/// subclasses of WritingSystemDefinition.
		/// </summary>
		protected abstract T ConstructDefinition();

		/// <summary>
		/// Creates an empty writing system with the specified language tag. This is implemented
		/// by subclasses to allow the use subclasses of WritingSystemDefinition.
		/// </summary>
		protected abstract T ConstructDefinition(string ietfLanguageTag);

		/// <summary>
		/// Clones the specified writing system. This is implemented by subclasses to allow the
		/// use subclasses of WritingSystemDefinition.
		/// </summary>
		protected abstract T ConstructDefinition(T ws);

		WritingSystemDefinition IWritingSystemFactory.Create(string ietfLanguageTag)
		{
			return Create(ietfLanguageTag);
		}

		WritingSystemDefinition IWritingSystemFactory.Create()
		{
			return Create();
		}

		WritingSystemDefinition IWritingSystemFactory.Create(WritingSystemDefinition ws)
		{
			return Create((T) ws);
		}
	}
}
