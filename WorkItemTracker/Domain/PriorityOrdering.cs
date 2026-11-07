namespace WorkItemTracker.Domain;
public sealed class PriorityOrdering : IComparer<WorkItem>        
{
    public int Compare(WorkItem? x, WorkItem? y)
    {
        //handle null
        if (x == null && y == null) return 0;
        if (x == null) return -1;
        if (y == null) return 1;

        int priorityCompare = x.Priority.CompareTo(y.Priority);

        if (priorityCompare != 0)
        {
            return priorityCompare;
        }
        return x.Id.CompareTo(y.Id);
    }
}