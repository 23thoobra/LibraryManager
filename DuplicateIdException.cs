namespace LibraryManager;
 
/// Thrown when an item is added with an Id that is already in the library.
/// A custom exception lets the menu catch this case separately from other errors.
public class DuplicateIdException : Exception
{
    public string Id { get; }

    public DuplicateIdException(string id)
        : base($"An item with Id '{id}' already exists. Ids must be unique.")
    {
        Id = id;
    }
}