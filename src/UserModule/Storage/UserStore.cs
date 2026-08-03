using System.Collections.Concurrent;
using UserModule.Models;

namespace UserModule.Storage;

internal sealed class UserStore
{
    #region Field Declarations

    private readonly ConcurrentDictionary<int, User> _users = new();
    private int _nextId;

    #endregion

    #region Public Method Declarations

    public IReadOnlyCollection<User> GetAll() => [.. _users.Values.OrderBy(user => user.Id)];

    public User? Get(int id) => _users.TryGetValue(id, out User? user) ? user : null;

    public User Add(User user)
    {
        int id = Interlocked.Increment(ref _nextId);
        User created = user with { Id = id };
        _users[id] = created;
        return created;
    }

    public User? Update(int id, User user)
    {
        if (!_users.ContainsKey(id))
        {
            return null;
        }

        User updated = user with { Id = id };
        _users[id] = updated;
        return updated;
    }

    public bool Remove(int id) => _users.TryRemove(id, out _);

    #endregion
}