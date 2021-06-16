

using System.Collections.Generic;
using System.Threading.Tasks;

namespace TodoApi.Database {

    /// <summary>
    /// Mock DB used to hold
    ///
    /// </summary>
    public interface IMockDB<T> {

        public Task<T> GetAsync(int id);
        public Task<IEnumerable<T>> ListAsync();
        public Task<bool> DeleteAsync(int id);
        public Task<bool> UpdateAsync(T item);
        public Task<bool> ExistsAsync(int id);
        public Task<int> AddAsync(T item);

    }
}
