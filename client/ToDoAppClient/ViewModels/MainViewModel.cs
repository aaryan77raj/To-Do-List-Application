using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;
using Grpc.Net.Client;
using ToDoAppClient.Models;
using ToDoAppClient.Services;

namespace ToDoAppClient.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly ITodoService _todoService;
        private readonly Dispatcher _dispatcher;
        private string _newItemDescription = string.Empty;
        private bool _isConnected;
        private string _connectionStatus = "Disconnected";

        public MainViewModel()
        {
            _dispatcher = Dispatcher.CurrentDispatcher;
            _todoService = new TodoService();
            
            TodoItems = new ObservableCollection<TodoItem>();
            
            AddItemCommand = new RelayCommand(AddItem, CanAddItem);
            ToggleStatusCommand = new RelayCommand<int>(ToggleStatus);
            
            // Initialize connection
            _ = Task.Run(InitializeConnection);
        }

        public ObservableCollection<TodoItem> TodoItems { get; }

        public string NewItemDescription
        {
            get => _newItemDescription;
            set
            {
                if (_newItemDescription != value)
                {
                    _newItemDescription = value;
                    OnPropertyChanged();
                    ((RelayCommand)AddItemCommand).RaiseCanExecuteChanged();
                }
            }
        }

        public bool IsConnected
        {
            get => _isConnected;
            set
            {
                if (_isConnected != value)
                {
                    _isConnected = value;
                    OnPropertyChanged();
                }
            }
        }

        public string ConnectionStatus
        {
            get => _connectionStatus;
            set
            {
                if (_connectionStatus != value)
                {
                    _connectionStatus = value;
                    OnPropertyChanged();
                }
            }
        }

        public ICommand AddItemCommand { get; }
        public ICommand ToggleStatusCommand { get; }

        private async Task InitializeConnection()
        {
            try
            {
                await _todoService.ConnectAsync("http://localhost:50051");
                
                _dispatcher.Invoke(() =>
                {
                    IsConnected = true;
                    ConnectionStatus = "Connected";
                });

                // Load existing items
                await LoadTodoItems();

                // Start listening for real-time updates
                _ = Task.Run(ListenForUpdates);
            }
            catch (Exception ex)
            {
                _dispatcher.Invoke(() =>
                {
                    IsConnected = false;
                    ConnectionStatus = $"Connection failed: {ex.Message}";
                });
            }
        }

        private async Task LoadTodoItems()
        {
            try
            {
                var items = await _todoService.GetListAsync();
                
                _dispatcher.Invoke(() =>
                {
                    TodoItems.Clear();
                    foreach (var item in items)
                    {
                        TodoItems.Add(item);
                    }
                });
            }
            catch (Exception ex)
            {
                _dispatcher.Invoke(() =>
                {
                    ConnectionStatus = $"Failed to load items: {ex.Message}";
                });
            }
        }

        private async void AddItem()
        {
            if (string.IsNullOrWhiteSpace(NewItemDescription))
                return;

            try
            {
                var newItem = await _todoService.AddItemAsync(NewItemDescription);
                
                _dispatcher.Invoke(() =>
                {
                    NewItemDescription = string.Empty;
                });
            }
            catch (Exception ex)
            {
                _dispatcher.Invoke(() =>
                {
                    ConnectionStatus = $"Failed to add item: {ex.Message}";
                });
            }
        }

        private bool CanAddItem()
        {
            return !string.IsNullOrWhiteSpace(NewItemDescription) && IsConnected;
        }

        private async void ToggleStatus(int itemId)
        {
            try
            {
                await _todoService.UpdateStatusAsync(itemId);
            }
            catch (Exception ex)
            {
                _dispatcher.Invoke(() =>
                {
                    ConnectionStatus = $"Failed to update item: {ex.Message}";
                });
            }
        }

        private async Task ListenForUpdates()
        {
            try
            {
                await foreach (var update in _todoService.StreamUpdatesAsync())
                {
                    _dispatcher.Invoke(() =>
                    {
                        switch (update.Type)
                        {
                            case UpdateType.ItemAdded:
                                TodoItems.Add(update.Item);
                                break;
                            case UpdateType.ItemUpdated:
                                var existingItem = FindItemById(update.Item.Id);
                                if (existingItem != null)
                                {
                                    existingItem.Description = update.Item.Description;
                                    existingItem.IsCompleted = update.Item.IsCompleted;
                                }
                                break;
                            case UpdateType.ItemDeleted:
                                var itemToRemove = FindItemById(update.Item.Id);
                                if (itemToRemove != null)
                                {
                                    TodoItems.Remove(itemToRemove);
                                }
                                break;
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                _dispatcher.Invoke(() =>
                {
                    ConnectionStatus = $"Update stream error: {ex.Message}";
                    IsConnected = false;
                });
            }
        }

        private TodoItem? FindItemById(int id)
        {
            foreach (var item in TodoItems)
            {
                if (item.Id == id)
                    return item;
            }
            return null;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool>? _canExecute;

        public RelayCommand(Action execute, Func<bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter) => _canExecute?.Invoke() ?? true;

        public void Execute(object? parameter) => _execute();

        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }

    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T> _execute;
        private readonly Func<T, bool>? _canExecute;

        public RelayCommand(Action<T> execute, Func<T, bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter) => _canExecute?.Invoke((T)parameter!) ?? true;

        public void Execute(object? parameter) => _execute((T)parameter!);

        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}
