namespace LibraryManager;

/// A DVD. Adds a running time in minutes to LibraryItem.
public class Dvd : LibraryItem
{
    public int Minutes { get; }

    public Dvd(string id, string title, Category category, int minutes)
        : base(id, title, category) // the base class validates Id, Title and Category
    {
        Minutes = ValidatePositive(minutes, "Minutes");
    }

    public override string Describe() => $"DVD, {Minutes} min";

    // Format: DVD|Id|Title|Category|IsBorrowed|Minutes
    public override string ToFileLine() =>
        string.Join(FieldSeparator, "DVD", Id, Title, Category, IsBorrowed, Minutes);
}