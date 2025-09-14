# ToDo Server Setup Guide

## Quick Start (Recommended)

### Option 1: Using Visual Studio (Easiest)

1. **Install Visual Studio Community** (free):
   - Download from: https://visualstudio.microsoft.com/vs/community/
   - During installation, make sure to include:
     - "Desktop development with C++"
     - "CMake tools for C++"

2. **Install vcpkg**:
   ```powershell
   # Open PowerShell as Administrator
   git clone https://github.com/Microsoft/vcpkg.git C:\vcpkg
   cd C:\vcpkg
   .\bootstrap-vcpkg.bat
   ```

3. **Install dependencies**:
   ```powershell
   .\vcpkg.exe install grpc protobuf --triplet x64-windows
   ```

4. **Build the server**:
   ```powershell
   cd D:\SourceCode\ToDo\server
   cmake -B build -S . -DCMAKE_TOOLCHAIN_FILE=C:\vcpkg\scripts\buildsystems\vcpkg.cmake
   cmake --build build
   ```

5. **Run the server**:
   ```powershell
   .\build\Debug\todo_server.exe
   ```

### Option 2: Using Chocolatey (Alternative)

1. **Install Chocolatey** (if not already installed):
   ```powershell
   # Run PowerShell as Administrator
   Set-ExecutionPolicy Bypass -Scope Process -Force
   [System.Net.ServicePointManager]::SecurityProtocol = [System.Net.ServicePointManager]::SecurityProtocol -bor 3072
   iex ((New-Object System.Net.WebClient).DownloadString('https://community.chocolatey.org/install.ps1'))
   ```

2. **Install dependencies**:
   ```powershell
   choco install cmake protoc grpc
   ```

3. **Build the server**:
   ```powershell
   cd D:\SourceCode\ToDo\server
   cmake -B build -S .
   cmake --build build
   ```

### Option 3: Manual Installation

1. **Download and install**:
   - CMake: https://cmake.org/download/
   - Protocol Buffers: https://github.com/protocolbuffers/protobuf/releases
   - gRPC: https://github.com/grpc/grpc/releases

2. **Add to PATH**:
   - Add the bin directories of the above tools to your system PATH

3. **Build**:
   ```powershell
   cd D:\SourceCode\ToDo\server
   cmake -B build -S .
   cmake --build build
   ```

## Troubleshooting

### Common Issues:

1. **"protoc not found"**:
   - Make sure Protocol Buffers is installed and in PATH
   - Try: `protoc --version`

2. **"gRPC not found"**:
   - Install gRPC using vcpkg or manually
   - Check that gRPC libraries are in the correct location

3. **"CMake not found"**:
   - Install CMake and add to PATH
   - Try: `cmake --version`

4. **Compilation errors**:
   - Make sure you have a C++17 compatible compiler
   - Check that all dependencies are properly installed

### Quick Test:

After building, test the server:
```powershell
.\build\Debug\todo_server.exe
```

You should see:
```
Starting To-Do List Server...
Server address: 0.0.0.0:50051
Press Ctrl+C to stop the server
Server listening on 0.0.0.0:50051
```

## Server Features

- **Add items**: Add new todo items with descriptions
- **Toggle status**: Mark items as completed or pending
- **Get list**: Retrieve all todo items
- **Real-time updates**: Stream updates to connected clients
- **Thread-safe**: Concurrent access support
- **Graceful shutdown**: Clean shutdown with Ctrl+C

## Client Connection

The server listens on `0.0.0.0:50051` by default. Your WPF client should connect to this address.

## Next Steps

1. Get the server running using one of the methods above
2. Test the server by running it
3. Connect your WPF client to the server
4. Start using the todo application!
