namespace LibraryManager;

/// Small reusable helpers
public static class Helpers
{
    /// (Bonus) Generic helper: returns the largest value in a sequence.
    /// Works for any type that can compare itself to another of the same type (int, double, string, ...).
    /// Throws InvalidOperationException when the sequence is empty, so callers check Any() first.
    public static T Largest<T>(IEnumerable<T> values) where T : IComparable<T>
    {
        using IEnumerator<T> e = values.GetEnumerator();

        if (!e.MoveNext())
        {
            throw new InvalidOperationException("Cannot find the largest value of an empty sequence.");
        }

        T largest = e.Current;

        while (e.MoveNext())
        {
            if (e.Current.CompareTo(largest) > 0)
            {
                largest = e.Current;
            }
        }

        return largest;
    }

    /// Shortens text that is too long for its column so the report stays aligned.
    public static string Fit(string text, int width) =>
        text.Length <= width ? text : text[..(width - 3)] + "...";
}