namespace InterviewProblems.Developer1;

/// <summary>
/// Developer 1 — Basic SQL (SQLite).
///
/// Each method returns a SQL query <b>string</b> that the tests execute against
/// the seeded database (same schema as every other level — see
/// <c>src/InterviewProblems/Data/schema.sql</c>). These cover SELECT, WHERE,
/// ORDER BY, and COUNT — no joins yet.
/// </summary>
public class SqlBasicsProblems
{
    /// <summary>
    /// Return the <c>Name</c> of every customer, ordered alphabetically.
    /// Expected: Alice, Bob, Carol, Dave, Eve.
    /// </summary>
    public string AllCustomerNamesAlphabetical()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Return the <c>Name</c> of every customer in the city 'Portland',
    /// ordered alphabetically.
    /// Expected: Bob, Eve.
    /// </summary>
    public string CustomersInPortland()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Return the <c>Name</c> of every product priced below 20, ordered by
    /// price from lowest to highest.
    /// Expected: Gizmo, Widget, eBook.
    /// </summary>
    public string ProductsCheaperThan20()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Return a single value: the total number of orders in the Orders table.
    /// Expected: 5.
    /// </summary>
    public string TotalNumberOfOrders()
    {
        throw new NotImplementedException();
    }
}
