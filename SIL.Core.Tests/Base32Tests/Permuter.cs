using System;
using System.Collections;

namespace SIL.Tests.Base32Tests
{
	public static class Permuter
	{
		#region Delegates

		public delegate void Visitor(IEnumerator[] a);

		#endregion

		public static void WriteToConsole(IEnumerator[] a)
		{
			for (int j = 1;j < a.Length;j++)
			{
				Console.Write("{0}{1}",
							  j == 0 ? string.Empty : " ",
							  a[j].Current);
			}
			Console.WriteLine();
		}

		public static void VisitAll(Visitor visitor, params IEnumerable[] m)
		{
			// Initialize.
			int n = m.Length;
			int j;
			IEnumerator[] a = new IEnumerator[n + 1];

			for (j = 1;j <= n;j++)
			{
				a[j] = m[j - 1].GetEnumerator();
				a[j].MoveNext();
			}
			a[0] = m[0].GetEnumerator();
			a[0].MoveNext();

			for (;;)
			{
				foreach (Visitor visit in visitor.GetInvocationList())
				{
					visit(a);
				}

				// Prepare to add one.
				j = n;

				// Carry if necessary.
				while (!a[j].MoveNext())
				{
					a[j].Reset();
					a[j].MoveNext();
					j -= 1;
				}

				// Increase unless done.
				if (j == 0)
				{
					break; // Terminate the algorithm
				}
			}
		}
	}
}