using System;
using System.Collections.Generic;
using System.Text;

namespace Palaso.Email
{
	public interface IEmailProvider
	{
		IEmailMessage CreateMessage();

		bool SendMessage(IEmailMessage message);
	}
}
