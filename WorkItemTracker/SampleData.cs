using WorkItemTracker.Domain;

namespace WorkItemTracker;

// Startup sample data (C5). Uses only the services' public methods.
public static class SampleData
{
    public static void Load(UserService userService, WorkItemService workItemService)
    {
        // Users (Omar ends with no work items).
        var sara = userService.CreateUser("Sara");
        var khaled = userService.CreateUser("Khaled");
        userService.CreateUser("Omar");

        // Work items. The assignee is set at creation, and every item starts as ToDo.
        var ciPipeline = workItemService.CreateWorkItem("Set up CI pipeline", Priority.High, sara.Id);
        var loginPage = workItemService.CreateWorkItem("Design login page", Priority.High, khaled.Id);
        var apiDocs = workItemService.CreateWorkItem("Write API docs", Priority.Medium, sara.Id);
        var paginationBug = workItemService.CreateWorkItem("Fix pagination bug", Priority.Medium, khaled.Id);
        workItemService.CreateWorkItem("Update README", Priority.Low, null);
        workItemService.CreateWorkItem("Review pull requests", Priority.Medium, sara.Id);

        // Status moves, one step at a time (Done takes two calls).
        workItemService.UpdateWorkItemStatus(ciPipeline.Id, Status.InProgress);
        workItemService.UpdateWorkItemStatus(ciPipeline.Id, Status.Done);

        workItemService.UpdateWorkItemStatus(loginPage.Id, Status.InProgress);

        workItemService.UpdateWorkItemStatus(apiDocs.Id, Status.InProgress);

        workItemService.UpdateWorkItemStatus(paginationBug.Id, Status.InProgress);
        workItemService.UpdateWorkItemStatus(paginationBug.Id, Status.Done);
    }
}
