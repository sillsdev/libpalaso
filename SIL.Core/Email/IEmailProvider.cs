namespace SIL.Email
{
	public interface IEmailProvider
	{
		IEmailMessage CreateMessage();

		bool SendMessage(IEmailMessage message);
	}
}
