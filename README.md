# To-Do List Application

A real-time synchronized To-Do List application built with a C++ gRPC server and C# WPF client. The application supports multiple concurrent clients with real-time updates across all connected clients.

## Overview

This application demonstrates a client-server architecture where:
- **Server**: C++ gRPC server that manages the todo list in memory
- **Client**: C# WPF application with MVVM pattern for the user interface
- **Communication**: gRPC for efficient, type-safe communication
- **Real-time Updates**: gRPC streaming for live synchronization across clients

### Features

- ✅ Add new todo items with descriptions
- ✅ Toggle item status between Pending and Completed
- ✅ View the complete list of todo items
- ✅ Real-time synchronization across multiple clients
- ✅ Thread-safe concurrent access
- ✅ Modern, responsive WPF UI
- ✅ MVVM architecture for clean separation of concerns

## Requirements

### Server Dependencies
- **CMake** 3.15 or higher
- **C++17** compatible compiler (MSVC, GCC, or Clang)
- **vcpkg** package manager
- **gRPC** and **Protobuf** libraries

### Client Dependencies
- **.NET 6.0** or higher
- **Visual Studio 2022** or **Visual Studio Code** with C# extension
- **Windows** (WPF requirement)

## Setup Instructions

### 1. Building the Server (C++ with CMake)

#### Prerequisites
1. Install **vcpkg** package manager:
   ```bash
   git clone https://github.com/Microsoft/vcpkg.git
   cd vcpkg
   ./bootstrap-vcpkg.bat  # On Windows
   # or
   ./bootstrap-vcpkg.sh   # On Linux/macOS
   ```

2. Install required packages:
   ```bash
   vcpkg install grpc protobuf
   ```

#### Build Steps
1. Navigate to the server directory:
   ```bash
   cd server
   ```

2. Configure the project with CMake:
   ```bash
   cmake -B build -S . -DCMAKE_TOOLCHAIN_FILE=[path to vcpkg]/scripts/buildsystems/vcpkg.cmake
   ```

3. Build the project:
   ```bash
   cmake --build build
   ```

4. The executable will be created at `build/todo_server.exe` (Windows) or `build/todo_server` (Linux/macOS)

### 2. Building the Client (C# WPF with Visual Studio)

#### Prerequisites
1. Install **.NET 6.0 SDK** or higher
2. Install **Visual Studio 2022** with WPF workload, or **Visual Studio Code** with C# extension

#### Build Steps
1. Navigate to the client directory:
   ```bash
   cd client
   ```

2. Restore NuGet packages:
   ```bash
   dotnet restore
   ```

3. Build the project:
   ```bash
   dotnet build
   ```

4. Or open `ToDoAppClient.sln` in Visual Studio and build from there

## Run Instructions

### 1. Start the Server

1. Navigate to the server build directory:
   ```bash
   cd server/build
   ```

2. Run the server:
   ```bash
   # Windows
   todo_server.exe
   
   # Linux/macOS
   ./todo_server
   ```

3. The server will start listening on `localhost:50051` by default
4. You should see: `Server listening on 0.0.0.0:50051`

### 2. Run the Client(s)

1. Navigate to the client directory:
   ```bash
   cd client/ToDoAppClient
   ```

2. Run the client:
   ```bash
   dotnet run
   ```

3. Or run from Visual Studio by pressing F5

4. The client will automatically connect to the server and display the current todo list

### 3. Demonstration of Real-time Sync

1. **Start the server** (as described above)
2. **Run multiple clients** by opening additional instances of the client application
3. **Add items** in one client - they will appear in all other clients instantly
4. **Toggle status** of items in any client - changes will be reflected across all clients
5. **Close and reopen** clients - they will automatically sync with the current server state

## Project Structure

```
ToDoApp/
├── server/                    # C++ gRPC Server
│   ├── CMakeLists.txt        # CMake build configuration
│   ├── todo.proto            # gRPC service definition
│   ├── server.cpp            # Main server entry point
│   ├── todo_service_impl.h   # Service implementation header
│   └── todo_service_impl.cpp # Service implementation
├── client/                    # C# WPF Client
│   ├── ToDoAppClient.sln     # Visual Studio solution
│   └── ToDoAppClient/        # WPF project
│       ├── ToDoAppClient.csproj
│       ├── MainWindow.xaml   # Main UI
│       ├── MainWindow.xaml.cs
│       ├── App.xaml
│       ├── App.xaml.cs
│       ├── Models/
│       │   └── TodoItem.cs   # Data model
│       ├── ViewModels/
│       │   └── MainViewModel.cs # MVVM ViewModel
│       └── Services/
│           ├── ITodoService.cs    # Service interface
│           └── TodoService.cs     # gRPC client implementation
└── README.md                 # This file
```

## Design Choices

### Why gRPC?
- **Performance**: Binary protocol with HTTP/2 multiplexing
- **Type Safety**: Strongly typed contracts via Protocol Buffers
- **Cross-platform**: Works seamlessly between C++ and C#
- **Streaming Support**: Built-in support for real-time updates
- **Code Generation**: Automatic client/server code generation

### Why WebSockets Alternative?
While gRPC was chosen for this implementation, WebSockets could be used as an alternative:
- **Pros**: Simpler setup, more universal browser support
- **Cons**: Less type safety, manual message serialization, no built-in load balancing

### Handling Multiple Clients
- **Thread-safe Storage**: Uses mutex-protected data structures
- **Real-time Updates**: gRPC streaming to push changes to all connected clients
- **Concurrent Access**: Server handles multiple simultaneous requests safely
- **Memory Storage**: Simple in-memory storage for demonstration (production would use a database)

### Real-time Notifications
- **gRPC Streaming**: Clients subscribe to update streams
- **Event-driven**: Changes trigger immediate notifications to all clients
- **Efficient**: Only changed data is transmitted
- **Reliable**: gRPC handles connection management and retries

## Troubleshooting

### Server Issues
- **Port already in use**: Change the port in `server.cpp` or kill existing processes
- **gRPC not found**: Ensure vcpkg packages are properly installed
- **Build errors**: Check CMake version and compiler compatibility

### Client Issues
- **Connection failed**: Ensure server is running and accessible
- **Build errors**: Check .NET SDK version and NuGet package restoration
- **UI not updating**: Check if MVVM bindings are correct

### Common Solutions
1. **Restart both server and client** if synchronization issues occur
2. **Check firewall settings** if clients can't connect to server
3. **Verify port availability** (default: 50051)
4. **Check console output** for detailed error messages

## Future Enhancements

- **Database Persistence**: Replace in-memory storage with SQLite/PostgreSQL
- **User Authentication**: Add login/logout functionality
- **Item Categories**: Support for categorizing todo items
- **Due Dates**: Add deadline functionality
- **Mobile Client**: Create Android/iOS clients
- **Web Client**: Add web-based client using gRPC-Web

## License

