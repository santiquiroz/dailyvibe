namespace DailyVibe.Application.Common;

public sealed record PagedResult<T>(IReadOnlyList<T> Items, int Page, int Size, int TotalCount)
{
    public PagedResult<TResult> Map<TResult>(Func<T, TResult> selector) =>
        new(Items.Select(selector).ToList(), Page, Size, TotalCount);
}
