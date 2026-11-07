using System.Collections.Immutable;

namespace  WorkItemTracker.Domain; 
public class WorkItemService
{
    private readonly IIdGenerator _idGenerator;
    private readonly List<WorkItem> _workItems;
    private readonly UserService _userService;

      private WorkItem findWorkItemById(int id)
    {
        var workItem = _workItems.FirstOrDefault(w => w.Id == id);
        if (workItem == null)
        {
            throw new DomainException($"Work item with ID {id} not found.");
        }
        return workItem;
    }
    // Parameter: IEnumerable<WorkItem>
//   The method only iterates; it never adds, removes or indexes. Asking for the
//   narrowest type lets every caller pass a filtered query (Where) directly,
//   without first copying it into a List.
//
// Return: IReadOnlyList<WorkItem>
//   Callers need Count and an indexer to print results, but must not modify them.
//
// ToList(): OrderBy/ThenBy are lazy, so ToList() is what runs the sort now and
//   produces a snapshot. Without it, the caller would get a live query over _workItems
//   that is re-sorted on every enumeration and breaks if the list changes meanwhile.
//
// AsReadOnly(): wraps the fresh list so it cannot be cast back to List<WorkItem> and
//   mutated. No extra copy is made.
//
// Ordering: Priority (High = 0 .. Low = 2, so ascending), then Id (Q2). This is the
//  
   
   //default to priority if nothing passed in, so that the default ordering is by priority.
    private IReadOnlyList<WorkItem> OrderWorkItems(IEnumerable<WorkItem> workItems, IComparer<WorkItem>? comparer)
    {
        comparer ??= new PriorityOrdering();    
    
        //use the custom comparer to sort by priority in descending order (High -> Low) and then by Id in ascending order.
        return workItems.OrderBy(w => w, comparer).ToList().AsReadOnly();
    
    
    }
    public WorkItemService(IIdGenerator idGenerator, UserService userService)
    {
        _idGenerator = idGenerator;
        _workItems = new List<WorkItem>();
        _userService = userService;
    }

    public WorkItem CreateWorkItem(string title, Priority priority, int? assigneeId)
    {
        WorkItem.ValidateTitle(title);

        User? assignee = assigneeId.HasValue
            ? _userService.GetUserById(assigneeId.Value)
            : null;
        var workItem = new WorkItem(_idGenerator.GetNextId(), title, priority);
        if (assignee != null)
            {
                workItem.AssignUser(assignee);
            }
        _workItems.Add(workItem);

        return workItem;
    }
   

    public void UpdateWorkItemStatus(int id, Status newStatus)
    {
        var workItem = findWorkItemById(id);
        workItem.UpdateStatus(newStatus);
    }
   
   //return workitem so when printing u have access to user 
    public WorkItem AssignUserToWorkItem(int workItemId, int userId)
    {
        var workItem = findWorkItemById(workItemId);
        var user = _userService.GetUserById(userId);
        workItem.AssignUser(user);
        return workItem;
    }

//instant index/xount access withthout permission to modify the list, which helps maintain encapsulation and data integrity.
    public IReadOnlyList<WorkItem> GetAllWorkItems(IComparer<WorkItem>? comparer = null)
    {
      
        return OrderWorkItems(_workItems, comparer);
    }

    public IReadOnlyList<WorkItem> GetWorkItemsByStatus(Status status, IComparer<WorkItem>? comparer = null)
    {
        var filteredWorkItems = _workItems.Where(w => w.Status == status);
        return OrderWorkItems(filteredWorkItems, comparer);
    }

    public IReadOnlyList<WorkItem> GetWorkItemsByAssignee(int userId, IComparer<WorkItem>? comparer = null)
    {
        var assignee = _userService.GetUserById(userId); // Ensure the user exists and throw error if not found
        //var filteredWorkItems = _workItems.Where(w => w.Assignee == assignee); //this depends on both objects being the same reference which is true here but not robus
        var filteredWorkItems = _workItems.Where(w => w.Assignee?.Id == assignee.Id);
        return OrderWorkItems(filteredWorkItems, comparer );
    }

    public IReadOnlyDictionary<Status, int> GetStatusSummary()
    {
       return Enum.GetValues<Status>().ToDictionary(status => status, status => _workItems.Count(w => w.Status == status)).AsReadOnly();
    }
}
