namespace WorkItemTracker.Domain;
public sealed class NewestFirstOrdering : IComparer<WorkItem>
{
    public int Compare(WorkItem? x, WorkItem? y)
    {
        //handle null
        if (x == null && y == null) return 0;
        if (x == null) return -1;
        if (y == null) return 1;
        int dateCompare = y.CreatedAt.CompareTo(x.CreatedAt);
        if (dateCompare != 0)
        {
            return dateCompare;
        }
        return y.Id.CompareTo(x.Id);
    }
}