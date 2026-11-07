namespace WorkItemTracker.Domain;
public sealed class TitleOrdering : IComparer<WorkItem>
{
    public int Compare(WorkItem? x, WorkItem? y)
    {
        //handle null
        if (x == null && y == null) return 0;
        if (x == null) return -1;
        if (y == null) return 1;

        int titleCompare = string.Compare(x.Title, y.Title, StringComparison.OrdinalIgnoreCase);

        if (titleCompare != 0)
        {
            return titleCompare;
        }
        return x.Id.CompareTo(y.Id);
    }
}