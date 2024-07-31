using System;
using System.Threading;

namespace SIL.CommandLineProcessing.TestApp
{
	public static class Program
	{
		[STAThread]
		public static void Main(string[] args)
		{
			// Support SIL.Tests.CommandLineProcessing.CommandLineRunnerTests
			if (args.Length > 0)
			{
				for (var i = 0; i < 10; i++)
				{
					Console.WriteLine(i);
					Thread.Sleep(1000);
				}
			}
		}
	}
}