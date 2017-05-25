/*
 * Original code from https://code.google.com/archive/p/snowcode/
 * License: MIT (http://www.opensource.org/licenses/mit-license.php)
 *
 * The code has been modified (mostly in the authentication area)
 * and greatly simplified (extracting only the bits we are using).
 */

using System;
using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace SIL.BuildTasks.AWS
{
	public abstract class AwsTaskBase : Task
	{
		/// <summary>
		/// The profile name in the credential profile store
		/// </summary>
		[Required]
		public string CredentialStoreProfileName { get; set; }

		/// <summary>
		/// Get AWS credentials from the credential profile store
		/// </summary>
		/// <returns></returns>
		protected virtual AWSCredentials GetAwsCredentials()
		{
			AWSCredentials awsCredentials;
			if (new CredentialProfileStoreChain().TryGetAWSCredentials(CredentialStoreProfileName, out awsCredentials))
			{
				Log.LogMessage(MessageImportance.Normal, "Connecting to AWS using AwsAccessKeyId: {0}", awsCredentials.GetCredentials().AccessKey);
				return awsCredentials;
			}

			throw new ApplicationException("Unable to get AWS credentials from the credential profile store");
		}

		protected virtual string Join(string[] values)
		{
			return string.Join(";", values);
		}
	}
}
