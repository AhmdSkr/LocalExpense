namespace LocalExpense.Services;

/// <summary>
/// The one order categories are listed in, in the category filter and in the Reports table alike: by name ignoring case, then
/// exactly. Categories are still matched exactly, so "Rent" and "rent" stay two categories, but they sit next to each other.
/// </summary>
public static class CategoryOrder
{
    public static IOrderedEnumerable<T> OrderByCategory<T>(this IEnumerable<T> source, Func<T, string> category) =>
        source.OrderBy(category, StringComparer.OrdinalIgnoreCase).ThenBy(category, StringComparer.Ordinal);
}
