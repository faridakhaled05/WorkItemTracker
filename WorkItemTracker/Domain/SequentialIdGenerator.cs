namespace WorkItemTracker.Domain;
public class SequentialIdGenerator : IIdGenerator
{
    private int _currentId = 1;

    public int GetNextId()
    {
        
        return _currentId++;
    }
}