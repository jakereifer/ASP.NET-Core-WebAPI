

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TodoApi.Models;

namespace TodoApi.Database {

    /// <summary>
    /// Mock DB used to hold
    ///
    /// </summary>
    public class TodoMockDB : IMockDB<TodoItem>
    {
        private readonly Dictionary<int, TodoItem> _dict;
        public int MaxId { get; private set;}

        public TodoMockDB() {
            _dict = new Dictionary<int, TodoItem>();
        }

        public Task<int> AddAsync(TodoItem item)
        {
            MaxId++;
            item.Id = MaxId;
            item.Secret = new Random().Next().ToString();
            _dict[item.Id] = item;
            return Task.FromResult(item.Id);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            if (!await ExistsAsync(id)) {
                throw new ArgumentException($"No TodoItem with id {id}");
            }
            return _dict.Remove(id);
        }

        public Task<bool> ExistsAsync(int id)
        {
            return Task.FromResult(_dict.ContainsKey(id));
        }

        public async Task<TodoItem> GetAsync(int id)
        {
            if (!await ExistsAsync(id)) {
                return null;
            }
            return _dict[id];
        }

        public Task<IEnumerable<TodoItem>> ListAsync()
        {
            return Task.FromResult(_dict.Values.ToList() as IEnumerable<TodoItem>); // ?
        }

        public async Task<bool> UpdateAsync(TodoItem item)
        {
            var id = item.Id;
            if (!await ExistsAsync(id)) {
                throw new ArgumentException($"No TodoItem with id {id}");
            }
            _dict[id] = item;
            return true;
        }

    }
}
