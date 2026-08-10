using System.Collections.Concurrent;
using ModularMonolith.Template.SharedKernel.Messaging;
using ModularMonolith.Template.SharedKernel.Outbox;
using AccountsModule.Models;
using AccountsModule.Storage;

namespace ModularMonolith.Template.AccountsModule.Tests.Storage;

internal sealed class InMemoryAccountStore : IAccountStore, IOutboxStore
{
    #region Field Declarations

    private readonly ConcurrentDictionary<int, Account> _accounts = new();
    private readonly ConcurrentDictionary<Guid, OutboxMessage> _outbox = new();
    private int _nextId;

    #endregion

    #region IAccountStore Method Declarations

    public Task<IReadOnlyList<Account>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Account> accounts = [.. _accounts.Values.OrderBy(account => account.Id)];
        return Task.FromResult(accounts);
    }

    public Task<Account?> FindAsync(int id, CancellationToken cancellationToken = default)
    {
        _accounts.TryGetValue(id, out Account? account);
        return Task.FromResult(account);
    }

    public Task<Account> CreateAsync(Account account, Func<Account, IMessage> integrationEventFactory, CancellationToken cancellationToken = default)
    {
        int id = Interlocked.Increment(ref _nextId);
        Account created = account with { Id = id };
        _accounts[id] = created;

        OutboxMessage message = OutboxMessage.Create(integrationEventFactory(created));
        _outbox[message.Id] = message;

        return Task.FromResult(created);
    }

    public Task<bool> UpdateAsync(Account account, CancellationToken cancellationToken = default)
    {
        if (!_accounts.ContainsKey(account.Id))
        {
            return Task.FromResult(false);
        }

        _accounts[account.Id] = account;
        return Task.FromResult(true);
    }

    public Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_accounts.TryRemove(id, out _));
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
