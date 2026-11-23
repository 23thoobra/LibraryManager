namespace LibraryManager;

/// (Bonus) An immutable record that holds one row of the report.
/// Records give value equality and a readable ToString() for free.
public record ReportRow(string Id, string Title, Category Category, string Status, string Details);