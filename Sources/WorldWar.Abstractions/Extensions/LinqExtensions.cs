using WorldWar.Abstractions.Models.Items.Base;

namespace WorldWar.Abstractions.Extensions;

public static class LinqExtensions
{
	public static IEnumerable<IEnumerable<T>> Batch<T>(
		this IEnumerable<T> source, int batchSize)
		where T : Item
	{
		using var enumerator = source.OrderByDescending(x => x.Size).GetEnumerator();
		while (enumerator.MoveNext())
		{
			yield return YieldBatchElements(enumerator, batchSize);
		}
	}

	private static IEnumerable<T> YieldBatchElements<T>(
		IEnumerator<T> source, int batchSize)
		where T : Item
	{
		do
		{
			yield return source.Current;

			batchSize -= source.Current.Size;
		} while (batchSize > 0 && source.MoveNext());
	}
}