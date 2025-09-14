cmake_minimum_required(VERSION 3.15)
project(ToDoServer)

set(CMAKE_CXX_STANDARD 17)
set(CMAKE_CXX_STANDARD_REQUIRED ON)

# Find required packages
find_package(Protobuf REQUIRED)
find_package(gRPC REQUIRED)
find_package(Threads REQUIRED)

# Generate protobuf and gRPC files
set(PROTO_PATH "${CMAKE_CURRENT_SOURCE_DIR}/todo.proto")
get_filename_component(PROTO_NAME "${PROTO_PATH}" NAME_WE)
get_filename_component(PROTO_DIR "${PROTO_PATH}" DIRECTORY)

set(PROTO_SRCS "${CMAKE_CURRENT_BINARY_DIR}/${PROTO_NAME}.pb.cc")
set(PROTO_HDRS "${CMAKE_CURRENT_BINARY_DIR}/${PROTO_NAME}.pb.h")
set(GRPC_SRCS "${CMAKE_CURRENT_BINARY_DIR}/${PROTO_NAME}.grpc.pb.cc")
set(GRPC_HDRS "${CMAKE_CURRENT_BINARY_DIR}/${PROTO_NAME}.grpc.pb.h")

add_custom_command(
    OUTPUT "${PROTO_SRCS}" "${PROTO_HDRS}" "${GRPC_SRCS}" "${GRPC_HDRS}"
    COMMAND protobuf::protoc
    ARGS --grpc_out "${CMAKE_CURRENT_BINARY_DIR}"
         --cpp_out "${CMAKE_CURRENT_BINARY_DIR}"
         -I "${PROTO_DIR}"
         --plugin=protoc-gen-grpc=$<TARGET_FILE:gRPC::grpc_cpp_plugin>
         "${PROTO_PATH}"
    DEPENDS "${PROTO_PATH}")

# Create a library for the generated files
add_library(todo_proto
    ${PROTO_SRCS}
    ${PROTO_HDRS}
    ${GRPC_SRCS}
    ${GRPC_HDRS})

target_link_libraries(todo_proto
    gRPC::grpc++
    protobuf::libprotobuf)

# Include directories
target_include_directories(todo_proto PUBLIC ${CMAKE_CURRENT_BINARY_DIR})

# Main server executable
add_executable(todo_server
    server.cpp
    todo_service_impl.cpp
    todo_service_impl.h)

target_link_libraries(todo_server
    todo_proto
    gRPC::grpc++
    protobuf::libprotobuf
    Threads::Threads)

# Include directories
target_include_directories(todo_server PRIVATE ${CMAKE_CURRENT_BINARY_DIR})

# Build instructions:
# 1. Install dependencies using vcpkg:
#    vcpkg install grpc protobuf
# 2. Configure with CMake:
#    cmake -B build -S . -DCMAKE_TOOLCHAIN_FILE=[path to vcpkg]/scripts/buildsystems/vcpkg.cmake
# 3. Build:
#    cmake --build build
# 4. Run:
#    ./build/todo_server