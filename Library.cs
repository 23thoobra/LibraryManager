namespace LibraryManager;

/// Holds every item in a Dictionary keyed by Id and records each action in a history list.
public class Library
{
    // Case-insensitive keys, so "b101" and "B101" count as the same Id.
    private readonly Dictionary<string, LibraryItem> items = new(StringComparer.OrdinalIgnoreCase);

    // A log of what happened this session, e.g. "Borrowed B101".
    private readonly List<string> history = new();

    /// Read-only view of the items so other classes cannot bypass the duplicate check.
    public IReadOnlyCollection<LibraryItem> Items => items.Values;

    public IReadOnlyList<string> History => history;

    public int Count => items.Count;

    /// Adds an item. Throws DuplicateIdException if the Id is already used.
    /// Pass logAction: false when loading from the file so the history only shows this session's actions.
    public void Add(LibraryItem item, bool logAction = true)
    {
        // TryAdd returns false instead of throwing when the key already exists.
        if (!items.TryAdd(item.Id, item))
        {
            throw new DuplicateIdException(item.Id);
        }

        if (logAction)
        {
            history.Add($"Added {item.Id}");
        }
    }

    /// Borrows an item. Throws KeyNotFoundException or InvalidOperationException.
    public LibraryItem Borrow(string id)
    {
        LibraryItem item = Find(id);
        item.Borrow(); // throws InvalidOperationException if already borrowed
        history.Add($"Borrowed {item.Id}");
        return item;
    }

    /// Returns an item. Throws KeyNotFoundException or InvalidOperationException.
    public LibraryItem Return(string id)
    {
        LibraryItem item = Find(id);
        item.Return(); // throws InvalidOperationException if it was not borrowed
        history.Add($"Returned {item.Id}");
        return item;
    }

    /// Adds a free-form entry to the history, e.g. "Saved 5 items".
    public void Log(string action) => history.Add(action);

    private LibraryItem Find(string id)
    {
        string key = id.Trim();

        if (!items.TryGetValue(key, out LibraryItem? item))
        {
            throw new KeyNotFoundException($"No item with Id '{key}' was found.");
        }

        return item;
    }
}