using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Net.Client;
using ToDoAppClient.Models;

namespace ToDoAppClient.Services
{
    public class TodoService : ITodoService
    {
        private GrpcChannel? _channel;
        private Todo.TodoService.TodoServiceClient? _client;

        public async Task ConnectAsync(string serverAddress)
        {
            _channel = GrpcChannel.ForAddress(serverAddress);
            _client = new Todo.TodoService.TodoServiceClient(_channel);
            
            // Test connection by getting the list
            await GetListAsync();
        }

        public async Task DisconnectAsync()
        {
            if (_channel != null)
            {
                await _channel.ShutdownAsync();
                _channel = null;
                _client = null;
            }
        }

        public async Task<TodoItem> AddItemAsync(string description)
        {
            if (_client == null)
                throw new InvalidOperationException("Not connected to server");

            var request = new Todo.AddItemRequest
            {
                Description = description
            };

            var response = await _client.AddItemAsync(request);
            
            return new TodoItem
            {
                Id = response.Id,
                Description = response.Description,
                IsCompleted = response.Status == Todo.TodoStatus.Completed
            };
        }

        public async Task UpdateStatusAsync(int itemId)
        {
            if (_client == null)
                throw new InvalidOperationException("Not connected to server");

            var request = new Todo.UpdateStatusRequest
            {
                Id = itemId
            };

            await _client.UpdateStatusAsync(request);
        }

        public async Task<List<TodoItem>> GetListAsync()
        {
            if (_client == null)
                throw new InvalidOperationException("Not connected to server");

            var request = new Todo.GetListRequest();
            var response = await _client.GetListAsync(request);

            var items = new List<TodoItem>();
            foreach (var item in response.Items)
            {
                items.Add(new TodoItem
                {
                    Id = item.Id,
                    Description = item.Description,
                    IsCompleted = item.Status == Todo.TodoStatus.Completed
                });
            }

            return items;
        }

        public async IAsyncEnumerable<TodoUpdate> StreamUpdatesAsync([System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            if (_client == null)
                throw new InvalidOperationException("Not connected to server");

            var request = new Todo.StreamUpdatesRequest();
            using var call = _client.StreamUpdates(request);

            while (await call.ResponseStream.MoveNext(cancellationToken))
            {
                var update = call.ResponseStream.Current;
                yield return new TodoUpdate
                {
                    Type = (UpdateType)update.Type,
                    Item = new TodoItem
                    {
                        Id = update.Item.Id,
                        Description = update.Item.Description,
                        IsCompleted = update.Item.Status == Todo.TodoStatus.Completed
                    }
                };
            }
        }
    }
}
