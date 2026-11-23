# LibraryManager
C# .NET 10 console app that manages a campus library's books and DVDs. Uses an abstract class, inheritance, an interface and an enum, with Dictionary/List collections, a LINQ report, TryParse and exception-based input handling, and async save/load to a text file. 

==========================================================================
CIS 483/583 - LibraryManager
Name: Braden Thooft
Github Repository - https://github.com/23thoobra/LibraryManager

A .NET 10 console app that tracks books and DVDs a campus library lends out.
Items are saved to library.txt on exit and loaded again on the next start.

Optional test data (in TestData/):
  - DemoInput.txt              every menu path, including invalid input.
                                Run it with:  Get-Content TestData\DemoInput.txt | dotnet run
                                (start without a library.txt for the same output)
  - LibraryWithMissingLines.txt  4 good lines + 9 bad ones. Copy it to library.txt
                                and run the app: the good items load, each bad
                                line is listed as skipped, nothing crashes.


FILES AND CLASSES
-----------------
Category.cs            enum Category { Fiction, Science, Technology, History }
IBorrowable.cs         interface with void Borrow() and void Return()
LibraryItem.cs         abstract base class, implements IBorrowable
                         - Id (init-only), Title, Category, IsBorrowed (private set)
                         - abstract string Describe(), abstract string ToFileLine()
                         - Borrow() throws InvalidOperationException if already borrowed
                         - Return() throws InvalidOperationException if not borrowed
                         - validation: empty Id/Title -> ArgumentException
Book.cs                subclass: adds Author and Pages; calls base(...);
                       overrides Describe(); Pages must be positive (ArgumentException)
Dvd.cs                 subclass: adds Minutes; calls base(...);
                       overrides Describe(); Minutes must be positive (ArgumentException)
DuplicateIdException.cs custom exception thrown when an Id is already in use
Library.cs             Dictionary<string, LibraryItem> keyed by Id (case-insensitive),
                       rejects duplicates with TryAdd; List<string> history of actions
                       ("Added B101", "Borrowed B101", "Returned B101", ...)
LibraryFile.cs         async SaveAsync (File.WriteAllLinesAsync) and LoadAsync
                       (File.ReadAllLinesAsync); TryParseLine skips bad lines
ReportRow.cs           (bonus) record type for one row of the report
Helpers.cs             (bonus) generic T Largest<T>(...) where T : IComparable<T>,
                       plus Fit() to shorten long titles so columns stay aligned
Program.cs             async Main, do-while menu with a switch statement,
                       input helpers using int.TryParse, try/catch blocks, the report


INPUT HANDLING
--------------
- All numbers (menu choice, pages, minutes, category) use int.TryParse, so text such as
  "abc" or "99999999999" is rejected with a message instead of crashing.
- Pages/Minutes/Category re-prompt until a whole number is entered; the constructors
  then reject zero or negative values with ArgumentException.
- try/catch around item creation catches DuplicateIdException, then ArgumentException,
  then Exception (safety net). Borrow/Return catch KeyNotFoundException, then
  InvalidOperationException, then Exception. Each shows a friendly message.
- If the input stream ends (Ctrl+Z / Ctrl+D), the program saves and exits normally.


LINQ QUERIES (Program.PrintReport)
----------------------------------
1. Sorted listing:  library.Items.OrderBy(i => i.Title).Select(i => new ReportRow(...))
                    printed in aligned columns with format specifiers like {row.Id,-6}
2. Borrowed count:  library.Items.Count(i => i.IsBorrowed)
3. Per category:    library.Items.GroupBy(i => i.Category)
                                 .Select(g => new { g.Key, Count = g.Count() })
4. Average pages:   library.Items.OfType<Book>().Average(b => b.Pages)
                    guarded by .Any() - prints "n/a (no books yet)" when there are no books
5. Bonus:           Helpers.Largest(books.Select(b => b.Pages)) and the same for DVD minutes
