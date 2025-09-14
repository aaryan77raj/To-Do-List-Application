using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ToDoAppClient.Models;

namespace ToDoAppClient.Services
{
    public interface ITodoService
    {
        Task ConnectAsync(string serverAddress);
        Task DisconnectAsync();
        Task<TodoItem> AddItemAsync(string description);
        Task UpdateStatusAsync(int itemId);
        Task<List<TodoItem>> GetListAsync();
        IAsyncEnumerable<TodoUpdate> StreamUpdatesAsync(CancellationToken cancellationToken = default);
    }

    public class TodoUpdate
    {
        public UpdateType Type { get; set; }
        public TodoItem Item { get; set; } = new();
    }

    public enum UpdateType
    {
        ItemAdded,
        ItemUpdated,
        ItemDeleted
    }
}
