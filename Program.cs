namespace LibraryManager;

/// Console front end: loads library.txt on startup, runs the menu loop, saves on exit.
internal static class Program
{
    private const string DataFile = "library.txt";

    // Set to true if the input stream closes (e.g. Ctrl+Z / Ctrl+D or piped input runs out),
    // so the program can save and exit instead of looping forever or crashing.
    private static bool inputEnded;

    private static async Task Main()
    {
        var library = new Library();

        Console.WriteLine("=== Campus Library Manager ===");
        await LoadAsync(library);

        bool running = true;

        do
        {
            ShowMenu();
            string input = ReadLineSafe();

            if (inputEnded)
            {
                Console.WriteLine();
                Console.WriteLine("Input ended - saving and exiting.");
                input = "6";
            }

            // int.TryParse never throws: it returns false for text like "abc" or "".
            if (!int.TryParse(input, out int choice))
            {
                Console.WriteLine($"  '{input}' is not a menu number. Please enter 1-6.");
                continue; // jumps to the while condition; running is still true
            }

            switch (choice)
            {
                case 1:
                    AddBook(library);
                    break;
                case 2:
                    AddDvd(library);
                    break;
                case 3:
                    BorrowItem(library);
                    break;
                case 4:
                    ReturnItem(library);
                    break;
                case 5:
                    PrintReport(library);
                    break;
                case 6:
                    await SaveAsync(library);
                    running = false;
                    break;
                default:
                    Console.WriteLine($"  {choice} is not on the menu. Please enter 1-6.");
                    break;
            }
        }
        while (running);

        Console.WriteLine("Goodbye!");
    }

    private static void ShowMenu()
    {
        Console.WriteLine();
        Console.WriteLine("1) Add book   2) Add DVD   3) Borrow   4) Return   5) Report   6) Save & exit");
        Console.Write("Choose an option: ");
    }

    // menu actions //

    private static void AddBook(Library library)
    {
        string id = Prompt("  Id (e.g. B101): ");
        string title = Prompt("  Title: ");
        string author = Prompt("  Author: ");
        int pages = ReadInt("  Pages: ");
        Category category = ReadCategory();

        if (inputEnded)
        {
            Console.WriteLine("  Input ended - item not added.");
            return;
        }

        // Creation is inside try/catch: the constructor validates the data and Add() checks for duplicates.
        try
        {
            var book = new Book(id, title, category, author, pages);
            library.Add(book);
            Console.WriteLine($"  Added {book.Id}: {book.Title}.");
        }
        catch (DuplicateIdException ex) // most specific first
        {
            Console.WriteLine($"  Not added: {ex.Message}");
        }
        catch (ArgumentException ex) // empty title/author, pages <= 0, ...
        {
            Console.WriteLine($"  Not added: {ex.Message}");
        }
        catch (Exception ex) // safety net so the program never crashes
        {
            Console.WriteLine($"  Something unexpected went wrong: {ex.Message}");
        }
    }

    private static void AddDvd(Library library)
    {
        string id = Prompt("  Id (e.g. D201): ");
        string title = Prompt("  Title: ");
        int minutes = ReadInt("  Minutes: ");
        Category category = ReadCategory();

        if (inputEnded)
        {
            Console.WriteLine("  Input ended - item not added.");
            return;
        }

        try
        {
            var dvd = new Dvd(id, title, category, minutes);
            library.Add(dvd);
            Console.WriteLine($"  Added {dvd.Id}: {dvd.Title}.");
        }
        catch (DuplicateIdException ex)
        {
            Console.WriteLine($"  Not added: {ex.Message}");
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"  Not added: {ex.Message}");
        } 
        catch (Exception ex)
        {
            Console.WriteLine($"  Something unexpected went wrong: {ex.Message}");
        }
    }

    private static void BorrowItem(Library library)
    {
        string id = Prompt("  Id to borrow: ");

        try
        {
            LibraryItem item = library.Borrow(id);
            Console.WriteLine($"  You borrowed '{item.Title}'.");
        }
        catch (KeyNotFoundException ex) // no such Id
        {
            Console.WriteLine($"  Cannot borrow: {ex.Message}");
        }
        catch (InvalidOperationException ex) // already borrowed
        {
            Console.WriteLine($"  Cannot borrow: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  Something unexpected went wrong: {ex.Message}");
        }
    }

    private static void ReturnItem(Library library)
    {
        string id = Prompt("  Id to return: ");

        try
        {
            LibraryItem item = library.Return(id);
            Console.WriteLine($"  '{item.Title}' has been returned. Thank you!");
        }
        catch (KeyNotFoundException ex)
        {
            Console.WriteLine($"  Cannot return: {ex.Message}");
        }
        catch (InvalidOperationException ex) // not borrowed
        {
            Console.WriteLine($"  Cannot return: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  Something unexpected went wrong: {ex.Message}");
        }
    }

    //  LINQ report

    private static void PrintReport(Library library)
    {
        Console.WriteLine();
        Console.WriteLine("======================== LIBRARY REPORT ========================");

        if (library.Count == 0)
        {
            Console.WriteLine("The library is empty. Add a book or DVD first.");
            return;
        }

        // 1. All items sorted by title, projected into ReportRow records.
        List<ReportRow> rows = library.Items
            .OrderBy(item => item.Title, StringComparer.OrdinalIgnoreCase)
            .Select(item => new ReportRow(
                item.Id,
                item.Title,
                item.Category,
                item.IsBorrowed ? "Borrowed" : "Available",
                item.Describe()))
            .ToList();

        // Negative width = left-aligned in a column of that many characters.
        Console.WriteLine($"{"Id",-6} {"Title",-28} {"Category",-11} {"Status",-10} Details");
        Console.WriteLine(new string('-', 80));

        foreach (ReportRow row in rows)
        {
            Console.WriteLine($"{row.Id,-6} {Helpers.Fit(row.Title, 28),-28} {row.Category,-11} {row.Status,-10} {row.Details}");
        }

        Console.WriteLine(new string('-', 80));

        // 2. Number of borrowed items.
        int borrowedCount = library.Items.Count(item => item.IsBorrowed);
        Console.WriteLine($"{"Total items:",-22}{library.Count,7}");
        Console.WriteLine($"{"Borrowed items:",-22}{borrowedCount,7}");

        // 3. Count per category with GroupBy.
        Console.WriteLine();
        Console.WriteLine("Items per category:");

        var perCategory = library.Items
            .GroupBy(item => item.Category)
            .OrderBy(group => group.Key)
            .Select(group => new { Category = group.Key, Count = group.Count() });

        foreach (var entry in perCategory)
        {
            Console.WriteLine($"  {entry.Category,-20}{entry.Count,7}");
        }

        // 4. Average pages of all books – Average() throws on an empty sequence, so check Any() first.
        Console.WriteLine();
        IEnumerable<Book> books = library.Items.OfType<Book>();

        if (books.Any())
        {
            double averagePages = books.Average(book => book.Pages);
            Console.WriteLine($"{"Average book pages:",-22}{averagePages,7:F1}");

            // Bonus: generic Largest<T> helper.
            Console.WriteLine($"{"Most pages in a book:",-22}{Helpers.Largest(books.Select(book => book.Pages)),7}");
        }
        else
        {
            Console.WriteLine($"{"Average book pages:",-22}  n/a (no books yet)");
        }

        IEnumerable<Dvd> dvds = library.Items.OfType<Dvd>();

        if (dvds.Any())
        {
            Console.WriteLine($"{"Longest DVD (min):",-22}{Helpers.Largest(dvds.Select(dvd => dvd.Minutes)),7}");
        }

        // 5. History of actions this session.
        Console.WriteLine();
        Console.WriteLine("History this session:");

        foreach (string action in library.History)
        {
            Console.WriteLine($"  - {action}");
        }

        Console.WriteLine("================================================================");
    }

    // async file I/O //

    private static async Task LoadAsync(Library library)
    {
        if (!File.Exists(DataFile))
        {
            Console.WriteLine($"No {DataFile} found - starting with an empty library.");
            return;
        }

        try
        {
            LoadResult result = await LibraryFile.LoadAsync(DataFile);
            int loaded = 0;

            foreach (LibraryItem item in result.Items)
            {
                try
                {
                    library.Add(item, logAction: false);
                    loaded++;
                }
                catch (DuplicateIdException ex) // same Id twice in the file: keep the first one
                {
                    result.Skipped.Add($"{item.Id}: {ex.Message}");
                }
            }

            library.Log($"Loaded {loaded} items from {DataFile}");
            Console.WriteLine($"Loaded {loaded} item(s) from {DataFile}.");

            if (result.Skipped.Count > 0)
            {
                Console.WriteLine($"Skipped {result.Skipped.Count} line(s) that could not be read:");

                foreach (string reason in result.Skipped)
                {
                    Console.WriteLine($"  - {reason}");
                }
            }
        }
        catch (IOException ex)
        {
            Console.WriteLine($"Could not read {DataFile}: {ex.Message}. Starting empty.");
        }
        catch (UnauthorizedAccessException ex)
        {
            Console.WriteLine($"No permission to read {DataFile}: {ex.Message}. Starting empty.");
        }
    }

    private static async Task SaveAsync(Library library)
    {
        try
        {
            await LibraryFile.SaveAsync(DataFile, library.Items);
            library.Log($"Saved {library.Count} items to {DataFile}");
            Console.WriteLine($"Saved {library.Count} item(s) to {Path.GetFullPath(DataFile)}.");
        }
        catch (IOException ex)
        {
            Console.WriteLine($"Could not save {DataFile}: {ex.Message}");
        }
        catch (UnauthorizedAccessException ex)
        {
            Console.WriteLine($"No permission to write {DataFile}: {ex.Message}");
        }
    }

    // input helpers //

    /// Console.ReadLine() returns null when input ends; turn that into "" and remembers it.
    private static string ReadLineSafe()
    {
        string? line = Console.ReadLine();

        if (line is null)
        {
            inputEnded = true;
            return string.Empty;
        }

        return line.Trim();
    }

    private static string Prompt(string message)
    {
        Console.Write(message);
        return ReadLineSafe();
    }

    /// Keeps asking until the user types a whole number (int.TryParse never throws).
    private static int ReadInt(string message)
    {
        while (true)
        {
            string text = Prompt(message);

            if (int.TryParse(text, out int value))
            {
                return value; // range (positive) is checked by the Book/Dvd constructors
            }

            if (inputEnded)
            {
                return 0; // 0 fails validation, so nothing is added
            }

            Console.WriteLine($"  '{text}' is not a whole number. Try again.");
        }
    }

    /// Shows the categories as a numbered list and keeps asking until a valid number is entered.
    private static Category ReadCategory()
    {
        Category[] categories = Enum.GetValues<Category>();
        string options = string.Join(", ", categories.Select((c, index) => $"{index + 1} {c}"));

        while (true)
        {
            string text = Prompt($"  Category ({options}): ");

            if (int.TryParse(text, out int number) && number >= 1 && number <= categories.Length)
            {
                return categories[number - 1];
            }

            if (inputEnded)
            {
                return categories[0];
            }

            Console.WriteLine($"  '{text}' is not a valid category. Enter a number from 1 to {categories.Length}.");
        }
    }
}