using ModularMonolith.Template.SharedKernel.Messaging;
using ModularMonolith.Template.SharedKernel.Outbox;
using System.Collections.Concurrent;
using UserModule.Models;
using UserModule.Storage;

namespace ModularMonolith.Template.UserModule.Tests.Storage;

internal sealed class InMemoryUserStore : IUserStore, IOutboxStore
{
    #region Field Declarations

    private readonly ConcurrentDictionary<int, User> _users = new();
    private readonly ConcurrentDictionary<Guid, OutboxMessage> _outbox = new();
    private int _nextId;

    #endregion

    #region IUserStore Method Declarations

    public Task<IReadOnlyList<User>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<User> users = [.. _users.Values.OrderBy(user => user.Id)];
        return Task.FromResult(users);
    }

    public Task<IReadOnlyList<User>> GetPageAsync(int afterId, int limit, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<User> users =
        [
            .. _users.Values
                .Where(user => user.Id > afterId)
                .OrderBy(user => user.Id)
                .Take(limit)
        ];
        return Task.FromResult(users);
    }

    public Task<User?> FindAsync(int id, CancellationToken cancellationToken = default)
    {
        _users.TryGetValue(id, out User? user);
        return Task.FromResult(user);
    }

    public Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_users.Count);
    }

    public Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_users.Values.Any(user => string.Equals(user.Email, email, StringComparison.OrdinalIgnoreCase)));
    }

    public Task<bool> ExistsByEmailAsync(string email, int excludingUserId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_users.Values.Any(user =>
            user.Id != excludingUserId && string.Equals(user.Email, email, StringComparison.OrdinalIgnoreCase)));
    }

    public Task<User> CreateAsync(User user, Func<User, IMessage> integrationEventFactory, CancellationToken cancellationToken = default)
    {
        int id = Interlocked.Increment(ref _nextId);
        User created = user with { Id = id };
        _users[id] = created;

        OutboxMessage message = OutboxMessage.Create(integrationEventFactory(created));
        _outbox[message.Id] = message;

        return Task.FromResult(created);
    }

    public Task<bool> UpdateAsync(User user, Func<User, IMessage> integrationEventFactory, CancellationToken cancellationToken = default)
    {
        if (!_users.ContainsKey(user.Id))
        {
            return Task.FromResult(false);
        }

        _users[user.Id] = user;

        OutboxMessage message = OutboxMessage.Create(integrationEventFactory(user));
        _outbox[message.Id] = message;

        return Task.FromResult(true);
    }

    public Task<bool> DeleteAsync(int id, Func<int, IMessage> integrationEventFactory, CancellationToken cancellationToken = default)
    {
        if (!_users.TryRemove(id, out _))
        {
            return Task.FromResult(false);
        }

        OutboxMessage message = OutboxMessage.Create(integrationEventFactory(id));
        _outbox[message.Id] = message;

        return Task.FromResult(true);
    }

    #endregion

    #region IOutboxStore Method Declarations

    public Task EnqueueAsync(IMessage message, CancellationToken cancellationToken = default)
    {
        OutboxMessage outboxMessage = OutboxMessage.Create(message);
        _outbox[outboxMessage.Id] = outboxMessage;
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<OutboxMessage>> GetPendingAsync(int maxAttempts, int batchSize, TimeSpan visibilityTimeout, CancellationToken cancellationToken = default)
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;
        IReadOnlyList<OutboxMessage> pending =
        [
            .. _outbox.Values
                .Where(message => message.ProcessedOnUtc is null
                    && message.Attempts < maxAttempts
                    && (message.NextAttemptOnUtc is null || message.NextAttemptOnUtc <= now))
                .OrderBy(message => message.OccurredOnUtc)
                .Take(batchSize)
        ];

        return Task.FromResult(pending);
    }

    public Task<IReadOnlyList<OutboxMessage>> GetDeadLetteredAsync(int maxAttempts, int batchSize, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<OutboxMessage> deadLettered =
        [
            .. _outbox.Values
                .Where(message => message.ProcessedOnUtc is null && message.Attempts >= maxAttempts)
                .OrderBy(message => message.OccurredOnUtc)
                .Take(batchSize)
        ];

        return Task.FromResult(deadLettered);
    }

    public Task UpdateAsync(IReadOnlyList<OutboxMessage> messages, CancellationToken cancellationToken = default)
    {
        foreach (OutboxMessage message in messages)
        {
            _outbox[message.Id] = message;
        }

        return Task.CompletedTask;
    }

    #endregion
}
