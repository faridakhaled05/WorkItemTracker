namespace WorkItemTracker.Domain;

public class WorkItem
{

    public int Id { get; }
    public string Title { get; }
    public Status Status { get; private set; }
    public Priority Priority { get; private set; }
    public User? Assignee { get; private set; }
    public DateTime CreatedAt { get; }

    public WorkItem(int id, string title, Priority priority)
    {
        Id = id;
        Title = validateTitle(title);
        Status = Status.ToDo;
        Priority = priority;
        CreatedAt = DateTime.UtcNow;
    }
    public static string ValidateTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new DomainException("Title cannot be empty.");
        }
        title = title.Trim();
        if (title.Length > 100)
        {
            throw new DomainException("Title must be at most 100 characters.");
        }
        return title;
    }

    public void UpdateStatus(Status newStatus)
    {
        if((int)newStatus-(int)Status !=1)
        {
            throw new DomainException($"Cannot change status from {Status} to {newStatus}. Status can only be updated in the order: ToDo -> InProgress -> Done.");
        }
        if(newStatus == Status.InProgress && Assignee == null)
        {
            throw new DomainException("Cannot mark work item as InProgress without an assignee.");
        }
        Status = newStatus;
    }
    public void AssignUser(User user)
    {
        if (Status == Status.Done)
        {
            throw new DomainException($"Work item {Id} is already done and cannot be assigned to a user.");
        }
        Assignee = user;
    }

    public void UpdatePriority(Priority newPriority)
    {
        Priority = newPriority;
    }
}   