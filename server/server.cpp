#include <iostream>
#include <memory>
#include <string>
#include <signal.h>
#include "todo_service_impl.h"

using namespace std;

// Global server instance for signal handling
unique_ptr<TodoServiceImpl> g_server;

void SignalHandler(int signal) {
    cout << "\nReceived signal " << signal << ". Shutting down server..." << endl;
    if (g_server) {
        g_server->Shutdown();
    }
    exit(0);
}

int main(int argc, char** argv) {
    // Set up signal handlers for graceful shutdown
    signal(SIGINT, SignalHandler);
    signal(SIGTERM, SignalHandler);
    
    // Server configuration
    string server_address = "0.0.0.0:50051";
    
    if (argc > 1) {
        server_address = argv[1];
    }
    
    cout << "Starting To-Do List Server..." << endl;
    cout << "Server address: " << server_address << endl;
    cout << "Press Ctrl+C to stop the server" << endl;
    
    try {
        // Create and run the server
        g_server = make_unique<TodoServiceImpl>();
        g_server->RunServer(server_address);
    }
    catch (const exception& e) {
        cerr << "Server error: " << e.what() << endl;
        return 1;
    }
    
    return 0;
}

/*
Build Instructions:
==================

1. Install dependencies using vcpkg:
   vcpkg install grpc protobuf

2. Configure with CMake:
   cmake -B build -S . -DCMAKE_TOOLCHAIN_FILE=[path to vcpkg]/scripts/buildsystems/vcpkg.cmake

3. Build:
   cmake --build build

4. Run:
   ./build/todo_server
   
   Or with custom address:
   ./build/todo_server 0.0.0.0:8080

Run Instructions:
================

1. Start the server:
   ./build/todo_server

2. The server will listen on port 50051 by default

3. Connect clients to the server address

4. Use Ctrl+C to stop the server gracefully

Features:
=========

- Add new todo items with descriptions
- Toggle item status between Pending and Completed
- Get the full list of todo items
- Real-time updates via gRPC streaming
- Thread-safe concurrent access
- Graceful shutdown handling
*/