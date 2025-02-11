using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace SIL.Text
{
	[Flags]
	public enum ApproximateMatcherOptions
	{
		/// <summary>
		/// Find closest forms only
		/// </summary>
		None = 0,
		IncludePrefixedForms = 1,
		IncludeNextClosestForms = 2,
		IncludePrefixedAndNextClosestForms = IncludePrefixedForms | IncludeNextClosestForms
	}

	public class ApproximateMatcher
	{
		#region Delegates

		public delegate string GetStringDelegate<T>(T t);

		#endregion

		public const int EditDistanceLargerThanMax = int.MaxValue;

		private static string Self(string s)
		{
			return s;
		}

		public static IList<string> FindClosestForms(IEnumerable forms,
													 string notNormalizedFormToMatch)
		{
			return FindClosestForms(forms, notNormalizedFormToMatch, ApproximateMatcherOptions.None);
		}

		public static IList<T> FindClosestForms<T>(IEnumerable itemsToSearch,
												   GetStringDelegate<T> itemFormExtractor,
												   string notNormalizedFormToMatch)
		{
			return FindClosestForms(itemsToSearch,
									itemFormExtractor,
									notNormalizedFormToMatch,
									ApproximateMatcherOptions.None);
		}

		public static IList<string> FindClosestForms(IEnumerable forms,
													 string notNormalizedFormToMatch,
													 ApproximateMatcherOptions options)
		{
			return FindClosestForms<string>(forms, Self, notNormalizedFormToMatch, options);
		}

		public static IList<T> FindClosestForms<T>(IEnumerable itemsToSearch,
												  GetStringDelegate<T> itemFormExtractor,
												  string notNormalizedFormToMatch,
												  ApproximateMatcherOptions options)
		{
			return FindClosestForms(itemsToSearch, itemFormExtractor, notNormalizedFormToMatch, options, 999);
		}


		// would like to have IEnumerable<T> but IBindingList isn't strong typed
		public static IList<T> FindClosestForms<T>(IEnumerable itemsToSearch,
												   GetStringDelegate<T> itemFormExtractor,
												   string notNormalizedFormToMatch,
												   ApproximateMatcherOptions options,
													int maxDistance)
		{
			string formToMatch = notNormalizedFormToMatch.Normalize(NormalizationForm.FormD);
			bool includeNextClosest = (options & ApproximateMatcherOptions.IncludeNextClosestForms) ==
									  ApproximateMatcherOptions.IncludeNextClosestForms;
			bool includeApproximatePrefixedForms = (options &
													ApproximateMatcherOptions.IncludePrefixedForms) ==
												   ApproximateMatcherOptions.IncludePrefixedForms;

			List<T> bestMatches = new List<T>();
			List<T> secondBestMatches = new List<T>();

			int bestEditDistance = int.MaxValue;
			int secondBestEditDistance = int.MaxValue;

			foreach (T item in itemsToSearch)
			{
				string originalForm = itemFormExtractor(item);
				if (!string.IsNullOrEmpty(originalForm))
				{
					string form = originalForm.Normalize(NormalizationForm.FormD);
					if (!string.IsNullOrEmpty(form))
					{
						int editDistance;
						editDistance = EditDistance(formToMatch,
													form,
													secondBestEditDistance,
													includeApproximatePrefixedForms);
						if(editDistance > maxDistance)
							continue;

						if (editDistance < bestEditDistance)
						{
							if (includeNextClosest && bestEditDistance != int.MaxValue)
							{
								// best becomes second best
								secondBestMatches.Clear();
								secondBestMatches.AddRange(bestMatches);
								secondBestEditDistance = bestEditDistance;
							}
							bestMatches.Clear();
							bestEditDistance = editDistance;
						}
						else if (includeNextClosest && editDistance > bestEditDistance &&
								 editDistance < secondBestEditDistance)
						{
							secondBestEditDistance = editDistance;
							secondBestMatches.Clear();
						}
						if (editDistance == bestEditDistance)
						{
							bestMatches.Add(item);
						}
						else if (includeNextClosest && editDistance == secondBestEditDistance)
						{
							secondBestMatches.Add(item);
						}
						Debug.Assert(bestEditDistance != secondBestEditDistance);
					}
				}
			}
			if (includeNextClosest)
			{
				bestMatches.AddRange(secondBestMatches);
			}
			return bestMatches;
		}

		// The Damerau-Levenshtein distance is equal to the minimal number of insertions, deletions, substitutions and transpositions needed to transform one string into another
		// http://en.wikipedia.org/wiki/Damerau-Levenshtein_distance
		// This algorithm is O(|x||y|) time and O(min(|x|,|y|)) space in worst and average case
		// Ukkonen 1985 Algorithms for approximate string matching. Information and Control 64, 100-118.
		// Eugene W. Myers 1986. An O (N D) difference algorithm and its variations. Algorithmica 1:2, 251-266.
		// are algorithm that can compute the edit distance in O(editdistance(x,y)^2) time
		// and O(k) space
		// using a diagonal transition algorithm

		// Ukkonen's cut-off heuristic is faster than the original Sellers 1980

		// returns int.MaxValue if distance is greater than cutoff.
		public static int EditDistance(string list1,
									   string list2,
									   int maxEditDistance,
									   bool treatSuffixAsZeroDistance)
		{
			const int deletionCost = 1;
			const int insertionCost = deletionCost; // should be symmetric
			const int substitutionCost = 1;
			const int transpositionCost = 1;
			if (maxEditDistance == int.MaxValue)
				// int.MaxValue has special meaning to us
			{
				--maxEditDistance;
			}
			int lastColumnThatNeedsToBeEvaluatedPrev = maxEditDistance;
			int lastColumnThatNeedsToBeEvaluatedCurr = maxEditDistance;
			int lastColumnThatNeedsToBeEvaluatedNext;

			int firstColumnThatNeedsToBeEvaluatedPrev = 0;
			int firstColumnThatNeedsToBeEvaluatedCurr = 0;
			int firstColumnThatNeedsToBeEvaluatedNext;

			// Validate parameters
			if (list1 == null)
			{
				throw new ArgumentNullException("list1");
			}
			if (list2 == null)
			{
				throw new ArgumentNullException("list2");
			}

			if (!treatSuffixAsZeroDistance)
				// this is not a reflexive operation so swap isn't allowed
			{
				// list2 is the one that we are actually using storage space for so we want it to be the smaller of the two
				if (list1.Length < list2.Length)
				{
					swap(ref list1, ref list2);
				}
			}

			int n1 = list1.Length, n2 = list2.Length;
			if (n1 == 0)
			{
				if (treatSuffixAsZeroDistance)
				{
					return 0;
				}
				return n2 * insertionCost;
			}

			if (n2 == 0)
			{
				if (treatSuffixAsZeroDistance)
				{
					return 0;
				}
				return n1 * deletionCost;
			}

			// Rather than maintain an entire matrix (which would require O(x*y) space),
			// just store the previous row, current row, and next row, each of which has a length min(x,y)+1,
			// so just O(min(x,y)) space.
			int prevRow = 0, curRow = 1, nextRow = 2;
			int[][] rows = new int[][] {new int[n2 + 1], new int[n2 + 1], new int[n2 + 1]};

			// For each virtual row (since we only have physical storage for two)
			for (int list1index = 0;list1index <= n1;++list1index)
			{
				for (int i = 0;i < n2 + 1;i++)
				{
					rows[nextRow][i] = EditDistanceLargerThanMax;
				}

				int maxIndex = Math.Min(lastColumnThatNeedsToBeEvaluatedCurr + 1, n2);
				// if we are on the last row and we don't need to evaluate to the end of
				// the column to determine if our edit distance is larger than the max
				// then the edit distance is larger than the max
				if (!treatSuffixAsZeroDistance && list1index == n1 && maxIndex < n2)
				{
					return EditDistanceLargerThanMax;
				}
				lastColumnThatNeedsToBeEvaluatedNext = int.MaxValue;
				firstColumnThatNeedsToBeEvaluatedNext = lastColumnThatNeedsToBeEvaluatedCurr + 1;

				int minDistance = int.MaxValue;

				for (int list2index = firstColumnThatNeedsToBeEvaluatedCurr;
					 list2index <= maxIndex;
					 ++list2index)
				{
					if (lastColumnThatNeedsToBeEvaluatedCurr == int.MaxValue)
					{
						break;
					}

					int distance = EditDistanceLargerThanMax;
					if (list1index == 0 || list2index == 0)
					{
						distance = list1index * insertionCost + list2index * deletionCost;
					}
					else
					{
						// Delete Distance
						if (list2index > firstColumnThatNeedsToBeEvaluatedCurr)
						{
							distance = rows[nextRow][list2index - 1] + deletionCost;
						}

						// Insert Distance
						if (list2index <= lastColumnThatNeedsToBeEvaluatedPrev + 1)
						{
							distance = Math.Min(distance, rows[curRow][list2index] + insertionCost);
						}

						// Replace Distance
						if (list2index > firstColumnThatNeedsToBeEvaluatedPrev)
						{
							int replaceDistance = rows[curRow][list2index - 1];

							if (!list1[list1index - 1].Equals(list2[list2index - 1]))
							{
								replaceDistance += substitutionCost;
							}
							distance = Math.Min(distance, replaceDistance);
						}

						if (list1index > 1 && list2index > 1 &&
							list1[list1index - 1].Equals(list2[list2index - 2]) &&
							list1[list1index - 2].Equals(list2[list2index - 1]))
						{
							distance = Math.Min(distance,
												rows[prevRow][list2index - 2] + transpositionCost);
						}
					}

					// only relevant if treatSuffixAsZeroDistance
					if (treatSuffixAsZeroDistance && distance < minDistance)
					{
						minDistance = distance;
					}
					rows[nextRow][list2index] = distance;
					if (distance <= maxEditDistance)
					{
						if (list2index < firstColumnThatNeedsToBeEvaluatedNext)
						{
							firstColumnThatNeedsToBeEvaluatedNext = list2index;
						}
						lastColumnThatNeedsToBeEvaluatedNext = list2index;
					}
					else if ((list1index == n1) && list2index > lastColumnThatNeedsToBeEvaluatedCurr)
					{
						break;
					}
				}

				// cycle the previous, current and next rows
				switch (prevRow)
				{
					case 0:
						prevRow = 1;
						curRow = 2;
						nextRow = 0;
						break;
					case 1:
						prevRow = 2;
						curRow = 0;
						nextRow = 1;
						break;
					case 2:
						prevRow = 0;
						curRow = 1;
						nextRow = 2;
						break;
				}

				lastColumnThatNeedsToBeEvaluatedPrev = lastColumnThatNeedsToBeEvaluatedCurr;
				lastColumnThatNeedsToBeEvaluatedCurr = lastColumnThatNeedsToBeEvaluatedNext;

				firstColumnThatNeedsToBeEvaluatedPrev = firstColumnThatNeedsToBeEvaluatedCurr;
				firstColumnThatNeedsToBeEvaluatedCurr = firstColumnThatNeedsToBeEvaluatedNext;

				if (treatSuffixAsZeroDistance && list1index == n1)
				{
					if (minDistance > maxEditDistance)
					{
						return EditDistanceLargerThanMax;
					}

					return minDistance;
				}
			}

			// Return the computed edit distance
			int editDistance = rows[curRow][n2];

			if (editDistance > maxEditDistance)
			{
				return EditDistanceLargerThanMax;
			}
			return editDistance;
		}

		private static void swap<A>(ref A x, ref A y)
		{
			A temp = x;
			x = y;
			y = temp;
		}
	}
}