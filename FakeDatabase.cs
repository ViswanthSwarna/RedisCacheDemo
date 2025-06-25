using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedisCacheDemo
{
    public class FakeDatabase
    {
        private readonly Dictionary<string, string> _store = new();

        public Task<string> GetAsync(string id)
            => Task.FromResult(_store.ContainsKey(id) ? _store[id] : $"DB-MISS:{id}");

        public Task WriteAsync(string id, string value)
        {
            _store[id] = value;
            return Task.CompletedTask;
        }
    }

}
