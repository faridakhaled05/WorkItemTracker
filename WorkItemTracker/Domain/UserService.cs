namespace WorkItemTracker.Domain;
public class UserService
{
    //the idgenerator uses the interface type polymorphism to allow for different implementations of id generation, such as sequential or random.
    //solid principle: the user service depends on the abstraction of the IIdGenerator interface, rather than a specific implementation. This allows for flexibility and easier testing, as different implementations of the IIdGenerator can be injected into the UserService.
    private readonly IIdGenerator _idGenerator;
    //why readonly list? which helps maintain encapsulation and data integrity. 
    private readonly List<User> _users;

    public UserService(IIdGenerator idGenerator)
    {
        _idGenerator = idGenerator;
        _users = new List<User>();
    }

    public User CreateUser(string name)
    {
        //use validateion here so that the id is not wasted if the name is invalid. 
        string validatedName = User.ValidateName(name);
        if (_users.Any(u => u.Name.Equals(validatedName, StringComparison.OrdinalIgnoreCase)))
        {
            throw new DomainException($"User with name '{validatedName}' already exists.");
        }
        int newId = _idGenerator.GetNextId();
        var user = new User(newId, validatedName);
        _users.Add(user);
        return user;
    }

    public IReadOnlyList<User> GetAllUsers()
    {
        return _users.AsReadOnly();
    }
}