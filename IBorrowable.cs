namespace LibraryManager;

/// Anything that can be lent out and given back.
/// Implemented by the abstract LibraryItem class, so every Book and Dvd is borrowable.
public interface IBorrowable
{
    /// Marks the item as borrowed. Throws InvalidOperationException if it already is.
    void Borrow();

    /// Marks the item as returned. Throws InvalidOperationException if it is not borrowed.
    void Return();
}