using WorkItemTracker.ConsoleUI;
using WorkItemTracker.Domain;

namespace WorkItemTracker;

// Starter menu loop. Replace each "Not implemented yet" with your own code.
// Some options already read their input with the ConsoleInput helpers, to show how they're used.
public class Program
{
    
    
    private static readonly List<(string Label, IComparer<WorkItem> Comparer, bool IsDefault)> Orderings = new()
    {
        ("Priority", new PriorityOrdering(), true),
        ("Title", new TitleOrdering(), false),
        ("Newest first", new NewestFirstOrdering(), false)
    };

    public static void Main()
    {
        var userService = new UserService(new SequentialIdGenerator());
        var workItemService = new WorkItemService(new SequentialIdGenerator(), userService);
        SampleData.Load(userService, workItemService);

        while (true)
        {
            ShowMenu();

            // One try/catch for every option: a rejected operation prints its message and returns to the menu.
            try
            {
                var choice = ConsoleInput.ReadText("Choose an option");
                switch (choice)
                {
                    case "0":
                        return;

                    case "1":
                        AddUser(userService);
                        break;

                    case "2":
                        ListUsers(userService);
                        break;

                    case "3":
                        CreateWorkItem(workItemService);
                        break;

                    case "4":
                        ListAllWorkItems(workItemService);
                        break;

                    case "5":
                        ListWorkItemsByStatus(workItemService);
                        break;

                    case "6":
                        ListWorkItemsByAssignee(workItemService);
                        break;

                    case "7":
                        AssignWorkItem(workItemService);
                        break;

                    case "8":
                        ChangeStatus(workItemService);
                        break;

                    case "9":
                        ShowSummary(workItemService);
                        break;

                    default:
                        Console.WriteLine($"'{choice}' is not a menu option.");
                        break;
                }
            }
            catch (DomainException ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }

    private static void ShowMenu()
    {
        Console.WriteLine();
        Console.WriteLine("=== Work Item Tracker ===");
        Console.WriteLine("1. Add user");
        Console.WriteLine("2. List users");
        Console.WriteLine("3. Create work item");
        Console.WriteLine("4. List all work items");
        Console.WriteLine("5. List work items by status");
        Console.WriteLine("6. List work items by assignee");
        Console.WriteLine("7. Assign work item");
        Console.WriteLine("8. Change status");
        Console.WriteLine("9. Show summary");
        Console.WriteLine("0. Exit");
    }

    private static void AddUser(UserService userService)
    {
        var name = ConsoleInput.ReadText("Name");
        var user = userService.CreateUser(name);
        Console.WriteLine($"User {user.Id} '{user.Name}' added.");
    }

    private static void ListUsers(UserService userService)
    {
        var users = userService.GetAllUsers();
        if (users.Count == 0)
        {
            Console.WriteLine("No users found.");
        }
        else
        {
            foreach (var u in users)
            {
                Console.WriteLine($"{u.Id}  {u.Name}");
            }
        }
    }

    private static void CreateWorkItem(WorkItemService workItemService)
    {
        var title = ConsoleInput.ReadText("Title");
        var priority = ConsoleInput.ReadChoice<Priority>("Priority");
        // ReadOptionalInt: returns null when the user just presses Enter.
        int? assigneeId = ConsoleInput.ReadOptionalInt("Assignee user Id (Enter for none)");

        var workItem = workItemService.CreateWorkItem(title, priority, assigneeId);
        Console.WriteLine($"Work item {workItem.Id} created.");
    }

private static IComparer<WorkItem> ChooseOrdering()
    {
        Console.WriteLine("Choose an ordering:");
        for (int i = 0; i < Orderings.Count; i++)
        {
            Console.WriteLine($"{i + 1}. {Orderings[i].Label}");
        }
        var orderingChoice = ConsoleInput.ReadOptionalInt("Ordering option (Enter for default)");
        while (orderingChoice != null && (orderingChoice < 1 || orderingChoice > Orderings.Count))
        {
            Console.WriteLine($"Invalid choice. Please enter a number between 1 and {Orderings.Count}, or press Enter for default.");
            orderingChoice = ConsoleInput.ReadOptionalInt("Ordering option (Enter for default)");
        }
        var selectedOrdering = orderingChoice == null 
        ? Orderings.First(o => o.IsDefault) 
        : Orderings[orderingChoice.Value - 1];

        return selectedOrdering.Comparer;
    
}        

    
    private static void ListAllWorkItems(WorkItemService workItemService)
    {
        var selectedComparer = ChooseOrdering();
        PrintList(workItemService.GetAllWorkItems(selectedComparer));
    }
    

    private static void ListWorkItemsByStatus(WorkItemService workItemService)
    {
        var status = ConsoleInput.ReadChoice<Status>("Status");
        //choose ordering option
         var selectedComparer = ChooseOrdering();
        PrintList(workItemService.GetWorkItemsByStatus(status, selectedComparer));
        
    }

    private static void ListWorkItemsByAssignee(WorkItemService workItemService)
    {
        var userId = ConsoleInput.ReadInt("Assignee User Id");
        var selectedComparer = ChooseOrdering();
        PrintList(workItemService.GetWorkItemsByAssignee(userId, selectedComparer));
    }

    private static void AssignWorkItem(WorkItemService workItemService)
    {
        var workItemId = ConsoleInput.ReadInt("Work item Id");
        var userId = ConsoleInput.ReadInt("User Id");
        var workItem = workItemService.AssignUserToWorkItem(workItemId, userId);
        //ust set by AssignUserToWorkItem it either succeeded or threw.
        Console.WriteLine($"Work item {workItemId} assigned to {workItem.Assignee!.Name}.");
    }

    private static void ChangeStatus(WorkItemService workItemService)
    {
        var workItemId = ConsoleInput.ReadInt("Work item Id");
        var newStatus = ConsoleInput.ReadChoice<Status>("New status");
        workItemService.UpdateWorkItemStatus(workItemId, newStatus);
        Console.WriteLine($"Work item {workItemId} is now {newStatus}.");
    }

    private static void ShowSummary(WorkItemService workItemService)
    {
        var summary = workItemService.GetStatusSummary();
        Console.WriteLine(string.Join("   ", summary.Select(kvp => $"{kvp.Key}: {kvp.Value}")));
    }

    private static void PrintList(IReadOnlyList<WorkItem> workItems)
    {
        if (workItems.Count == 0)
        {
            Console.WriteLine("No matching work items.");
            return;
        }

        // Each column is as wide as its longest value (header included). Id is right-aligned.
        var rows = workItems.Select(w => new[]
        {
            w.Id.ToString(),
            w.Title,
            w.Priority.ToString(),
            w.Status.ToString(),
            w.Assignee?.Name ?? "-",
            w.CreatedAt.ToLocalTime().ToString("yyyy-MM-dd HH:mm")
        }).ToList();
        string[] headers = ["Id", "Title", "Priority", "Status", "Assignee", "Created" ];
        var widths = headers.Select((h, i) => Math.Max(h.Length, rows.Max(r => r[i].Length))).ToArray();

        Console.WriteLine(FormatRow(headers, widths));
        foreach (var row in rows)
        {
            Console.WriteLine(FormatRow(row, widths));
        }
    }

    private static string FormatRow(string[] cells, int[] widths)
    {
        var id = cells[0].PadLeft(widths[0]);
        var others = cells.Skip(1).Select((c, i) => c.PadRight(widths[i + 1]));
        return (id + "  " + string.Join("  ", others)).TrimEnd();
    }
}
