# PowerShell script to set up ToDo Server dependencies
Write-Host "Setting up ToDo Server Dependencies..." -ForegroundColor Green
Write-Host ""

# Check if vcpkg exists
if (-not (Test-Path "C:\vcpkg\vcpkg.exe")) {
    Write-Host "vcpkg not found. Installing vcpkg..." -ForegroundColor Yellow
    Write-Host ""
    
    # Clone vcpkg
    try {
        git clone https://github.com/Microsoft/vcpkg.git C:\vcpkg
        if ($LASTEXITCODE -ne 0) {
            throw "Failed to clone vcpkg"
        }
    }
    catch {
        Write-Host "Failed to clone vcpkg. Please ensure Git is installed." -ForegroundColor Red
        Read-Host "Press Enter to continue"
        exit 1
    }
    
    # Bootstrap vcpkg
    Set-Location C:\vcpkg
    try {
        & .\bootstrap-vcpkg.bat
        if ($LASTEXITCODE -ne 0) {
            throw "Failed to bootstrap vcpkg"
        }
    }
    catch {
        Write-Host "Failed to bootstrap vcpkg." -ForegroundColor Red
        Read-Host "Press Enter to continue"
        exit 1
    }
    
    Write-Host "vcpkg installed successfully!" -ForegroundColor Green
    Write-Host ""
}

# Install required packages
Write-Host "Installing gRPC and Protobuf..." -ForegroundColor Yellow
try {
    & C:\vcpkg\vcpkg.exe install grpc protobuf --triplet x64-windows
    if ($LASTEXITCODE -ne 0) {
        throw "Failed to install packages"
    }
}
catch {
    Write-Host "Failed to install packages." -ForegroundColor Red
    Read-Host "Press Enter to continue"
    exit 1
}

Write-Host ""
Write-Host "Dependencies installed successfully!" -ForegroundColor Green
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Cyan
Write-Host "1. Run: cmake -B build -S . -DCMAKE_TOOLCHAIN_FILE=C:\vcpkg\scripts\buildsystems\vcpkg.cmake"
Write-Host "2. Run: cmake --build build"
Write-Host "3. Run: .\build\Debug\todo_server.exe"
Write-Host ""
Read-Host "Press Enter to continue"
