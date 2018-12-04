namespace SIL.WritingSystems
{
	public abstract class WritingSystemFactoryBase<T> : IWritingSystemFactory<T> where T : WritingSystemDefinition
	{
		public virtual bool Create(string ietfLanguageTag, out T ws)
		{
			ws = ConstructDefinition(ietfLanguageTag);
			return false;
		}

		public virtual T Create(T ws, bool cloneId = false)
		{
			return ConstructDefinition(ws, cloneId);
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
		protected abstract T ConstructDefinition(T ws, bool cloneId = false);

		bool IWritingSystemFactory.Create(string ietfLanguageTag, out WritingSystemDefinition ws)
		{
			T tempWS;
			bool res = Create(ietfLanguageTag, out tempWS);
			ws = tempWS;
			return res;
		}

		WritingSystemDefinition IWritingSystemFactory.Create()
		{
			return Create();
		}

		WritingSystemDefinition IWritingSystemFactory.Create(WritingSystemDefinition ws, bool cloneId)
		{
			return Create((T) ws, cloneId);
		}
	}
}
