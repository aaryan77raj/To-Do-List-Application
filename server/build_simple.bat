@echo off
echo Building ToDo Server (Simple approach)...
echo.

REM Check if protoc is available
where protoc >nul 2>&1
if errorlevel 1 (
    echo Protocol Buffers compiler (protoc) not found.
    echo Please install Protocol Buffers from: https://github.com/protocolbuffers/protobuf/releases
    echo Or install via chocolatey: choco install protoc
    echo Or install via vcpkg: vcpkg install protobuf
    pause
    exit /b 1
)

REM Generate protobuf files
echo Generating protobuf files...
protoc --cpp_out=. --grpc_out=. --plugin=protoc-gen-grpc=grpc_cpp_plugin.exe todo.proto
if errorlevel 1 (
    echo Failed to generate protobuf files.
    echo Make sure you have gRPC plugin installed.
    pause
    exit /b 1
)

REM Try to compile with available compiler
echo Compiling server...
where cl >nul 2>&1
if not errorlevel 1 (
    echo Using Visual Studio compiler...
    cl /EHsc /std:c++17 /I. server.cpp todo_service_impl.cpp todo.pb.cc todo.grpc.pb.cc /link /SUBSYSTEM:CONSOLE
    if errorlevel 1 (
        echo Compilation failed with Visual Studio compiler.
        pause
        exit /b 1
    )
) else (
    where g++ >nul 2>&1
    if not errorlevel 1 (
        echo Using g++ compiler...
        g++ -std=c++17 -I. server.cpp todo_service_impl.cpp todo.pb.cc todo.grpc.pb.cc -o todo_server.exe
        if errorlevel 1 (
            echo Compilation failed with g++ compiler.
            pause
            exit /b 1
        )
    ) else (
        echo No suitable compiler found. Please install Visual Studio or MinGW.
        pause
        exit /b 1
    )
)

echo.
echo Build completed successfully!
echo Run the server with: todo_server.exe
echo.
pause
