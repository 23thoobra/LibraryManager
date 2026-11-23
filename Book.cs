namespace LibraryManager;

/// A printed book. Adds an author and a page count to LibraryItem.
public class Book : LibraryItem
{
    public string Author { get; }

    public int Pages { get; }

    public Book(string id, string title, Category category, string author, int pages)
        : base(id, title, category) // the base class validates Id, Title and Category
    {
        Author = ValidateText(author, "Author");
        Pages = ValidatePositive(pages, "Pages");
    }

    public override string Describe() => $"Book by {Author}, {Pages} pages";

    // Format: BOOK|Id|Title|Category|IsBorrowed|Author|Pages
    public override string ToFileLine() =>
        string.Join(FieldSeparator, "BOOK", Id, Title, Category, IsBorrowed, Author, Pages);
}