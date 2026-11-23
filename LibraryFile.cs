namespace LibraryManager;

/// Saves and loads library.txt asynchronously.
/// One item per line, fields separated by '|':
///   BOOK|Id|Title|Category|IsBorrowed|Author|Pages
///   DVD|Id|Title|Category|IsBorrowed|Minutes
public static class LibraryFile
{
    /// Writes every item to the file with File.WriteAllLinesAsync.
    public static async Task SaveAsync(string path, IEnumerable<LibraryItem> items)
    {
        IEnumerable<string> lines = items.Select(item => item.ToFileLine());
        await File.WriteAllLinesAsync(path, lines);
    }

    /// Reads the file with File.ReadAllLinesAsync and rebuilds the objects.
    /// Lines that cannot be parsed are skipped and described in the Skipped list – never a crash.
    public static async Task<LoadResult> LoadAsync(string path)
    {
        string[] lines = await File.ReadAllLinesAsync(path);
        var result = new LoadResult();

        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i];
            int lineNumber = i + 1;

            if (string.IsNullOrWhiteSpace(line))
            {
                continue; // blank lines are harmless, ignore them quietly
            }

            if (TryParseLine(line, out LibraryItem? item, out string error))
            {
                result.Items.Add(item!);
            }
            else
            {
                result.Skipped.Add($"line {lineNumber}: {error}");
            }
        }

        return result;
    }

    /// Turns one line into a Book or Dvd. Uses TryParse for every number/enum/bool,
    /// and catches ArgumentException from the constructors' validation.
    public static bool TryParseLine(string line, out LibraryItem? item, out string error)
    {
        item = null;
        error = string.Empty;

        string[] parts = line.Split(LibraryItem.FieldSeparator);
        string type = parts[0].Trim().ToUpperInvariant();

        int expectedFields = type switch
        {
            "BOOK" => 7,
            "DVD" => 6,
            _ => -1
        };

        if (expectedFields == -1)
        {
            error = $"unknown item type '{parts[0]}'";
            return false;
        }

        if (parts.Length != expectedFields)
        {
            error = $"expected {expectedFields} fields for {type} but found {parts.Length}";
            return false;
        }

        // Enum.TryParse also accepts numbers like "42", so IsDefined makes sure it is a real category.
        if (!Enum.TryParse(parts[3].Trim(), ignoreCase: true, out Category category) || !Enum.IsDefined(category))
        {
            error = $"invalid category '{parts[3]}'";
            return false;
        }

        if (!bool.TryParse(parts[4].Trim(), out bool isBorrowed))
        {
            error = $"invalid borrowed flag '{parts[4]}'";
            return false;
        }

        // The last field is Pages for a book and Minutes for a DVD.
        if (!int.TryParse(parts[^1].Trim(), out int number))
        {
            error = $"'{parts[^1]}' is not a whole number";
            return false;
        }

        try
        {
            item = type == "BOOK"
                ? new Book(parts[1], parts[2], category, parts[5], number)
                : new Dvd(parts[1], parts[2], category, number);

            // IsBorrowed has a private setter, so restore the state through the public Borrow() method.
            if (isBorrowed)
            {
                item.Borrow();
            }

            return true;
        }
        catch (ArgumentException ex) // empty title, non-positive pages/minutes, etc.
        {
            error = ex.Message;
            item = null;
            return false;
        }
    }
}

/// What LoadAsync found: the good items and a description of each skipped line.
public class LoadResult
{
    public List<LibraryItem> Items { get; } = new();

    public List<string> Skipped { get; } = new();
}