using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DUIWA_Tests.Models
{
public class MockSession : ISession
{
    private readonly Dictionary<string, byte[]> _store = new();

    public string Id => Guid.NewGuid().ToString();
    public bool IsAvailable => true;
    public IEnumerable<string> Keys => _store.Keys;

    public void Clear() => _store.Clear();
    public Task CommitAsync(CancellationToken token = default) => Task.CompletedTask;
    public Task LoadAsync(CancellationToken token = default) => Task.CompletedTask;
    public void Remove(string key) => _store.Remove(key);
    public void Set(string key, byte[] value) => _store[key] = value;
    public bool TryGetValue(string key, out byte[] value) => _store.TryGetValue(key, out value);
}
}
