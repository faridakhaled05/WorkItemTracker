namespace WorkItemTracker.Domain;
public class UserService
{
    //the idgenerator uses the interface type polymorphism to allow for different implementations of id generation, such as sequential or random.
    //solid principle: the user service depends on the abstraction of the IIdGenerator interface, rather than a specific implementation. This allows for flexibility and easier testing, as different implementations of the IIdGenerator can be injected into the UserService.
    private readonly IIdGenerator _idGenerator;
    private readonly Dictionary<int, User> _users;
    private readonly HashSet<string> _userNames;

    public UserService(IIdGenerator idGenerator)
    {
        _idGenerator = idGenerator;
        _users = new Dictionary<int, User>();
        _userNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    }

    public User CreateUser(string name)
    {
        
        string validatedName = User.ValidateName(name);
        if (_userNames.Contains(validatedName))
        {
            throw new DomainException($"User with name '{validatedName}' already exists.");
        }
        int newId = _idGenerator.GetNextId();
        var user = new User(newId, validatedName);
        _users.Add(user.Id, user);
        _userNames.Add(validatedName);
        return user;
    }
    public User GetUserById(int id)
    {
        if (!_users.TryGetValue(id, out var user))
        {
            throw new DomainException($"User with ID '{id}' not found.");
        }
        return user;
    }

    public IReadOnlyList<User> GetAllUsers()
    {
        return _users.Values.OrderBy(u => u.Id).ToList().AsReadOnly();
    }
}