namespace WorkItemTracker.Domain;
public class User
{
    public int Id { get; }
    public string Name { get; }    

    public User(int id, string name)
    {
        Id = id;
        Name = ValidateName(name);
    }

    public static  string ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("User name cannot be empty.");
        }
        name = name.Trim();
        if (name.Length > 50)
        {
            throw new DomainException("User name must be at most 50 characters.");
        }
        return name;
    }
}   
