@echo off
echo Setting up ToDo Server Dependencies...
echo.

REM Check if vcpkg exists
if not exist "C:\vcpkg\vcpkg.exe" (
    echo vcpkg not found. Installing vcpkg...
    echo.
    
    REM Clone vcpkg
    git clone https://github.com/Microsoft/vcpkg.git C:\vcpkg
    if errorlevel 1 (
        echo Failed to clone vcpkg. Please ensure Git is installed.
        pause
        exit /b 1
    )
    
    REM Bootstrap vcpkg
    cd C:\vcpkg
    call bootstrap-vcpkg.bat
    if errorlevel 1 (
        echo Failed to bootstrap vcpkg.
        pause
        exit /b 1
    )
    
    echo vcpkg installed successfully!
    echo.
)

REM Install required packages
echo Installing gRPC and Protobuf...
C:\vcpkg\vcpkg.exe install grpc protobuf --triplet x64-windows
if errorlevel 1 (
    echo Failed to install packages.
    pause
    exit /b 1
)

echo.
echo Dependencies installed successfully!
echo.
echo Next steps:
echo 1. Run: cmake -B build -S . -DCMAKE_TOOLCHAIN_FILE=C:\vcpkg\scripts\buildsystems\vcpkg.cmake
echo 2. Run: cmake --build build
echo 3. Run: .\build\Debug\todo_server.exe
echo.
pause
