namespace LibraryManager;

/// Abstract base class for everything the library lends out.
/// It cannot be created directly; use Book or Dvd instead.
public abstract class LibraryItem : IBorrowable
{
    /// Character that separates fields in library.txt, so it is not allowed inside text fields.
    public const char FieldSeparator = '|';

    /// Unique id such as "B101". Init-only: it can be set while the object is created, never changed later.
    public string Id { get; init; }

    public string Title { get; }

    public Category Category { get; }

    /// Only Borrow() and Return() can change this (private set) – outside code can only read it.
    public bool IsBorrowed { get; private set; }

    /// Shared constructor that subclasses call with base(...).
    /// Throws ArgumentException when the data is invalid.
    protected LibraryItem(string id, string title, Category category)
    {
        Id = ValidateText(id, "Id").ToUpperInvariant();
        Title = ValidateText(title, "Title");

        if (!Enum.IsDefined(category))
        {
            throw new ArgumentException($"'{category}' is not a valid category.");
        }

        Category = category;
    }

    /// Each subclass returns its own one-line description (polymorphism).
    public abstract string Describe();

    /// Each subclass knows how to write itself as one line of library.txt.
    public abstract string ToFileLine();

    public void Borrow()
    {
        if (IsBorrowed)
        {
            throw new InvalidOperationException($"'{Title}' ({Id}) is already borrowed.");
        }

        IsBorrowed = true;
    }

    public void Return()
    {
        if (!IsBorrowed)
        {
            throw new InvalidOperationException($"'{Title}' ({Id}) is not currently borrowed, so it cannot be returned.");
        }

        IsBorrowed = false;
    }

    /// Shared validation for text fields: not empty and no '|' (which would break the save file).
    /// Returns the trimmed text.
    protected static string ValidateText(string? value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException($"{fieldName} cannot be empty.");
        }

        if (value.Contains(FieldSeparator))
        {
            throw new ArgumentException($"{fieldName} cannot contain the '{FieldSeparator}' character.");
        }

        return value.Trim();
    }

    /// Shared validation for counts such as pages and minutes.
    protected static int ValidatePositive(int value, string fieldName)
    {
        if (value <= 0)
        {
            throw new ArgumentException($"{fieldName} must be a positive number (you entered {value}).");
        }

        return value;
    }
}