#pragma once

#include <grpcpp/grpcpp.h>
#include <memory>
#include <mutex>
#include <unordered_map>
#include <vector>
#include <atomic>
#include <thread>
#include <queue>
#include <condition_variable>

#include "todo.grpc.pb.h"

using grpc::Server;
using grpc::ServerBuilder;
using grpc::ServerContext;
using grpc::Status;
using grpc::ServerWriter;

using todo::TodoService;
using todo::AddItemRequest;
using todo::AddItemResponse;
using todo::UpdateStatusRequest;
using todo::UpdateStatusResponse;
using todo::GetListRequest;
using todo::GetListResponse;
using todo::StreamUpdatesRequest;
using todo::TodoUpdate;
using todo::TodoItem;
using todo::TodoStatus;
using todo::UpdateType;

class TodoServiceImpl final : public TodoService::Service {
public:
    TodoServiceImpl();
    ~TodoServiceImpl();

    // gRPC service methods
    Status AddItem(ServerContext* context, const AddItemRequest* request, AddItemResponse* response) override;
    Status UpdateStatus(ServerContext* context, const UpdateStatusRequest* request, UpdateStatusResponse* response) override;
    Status GetList(ServerContext* context, const GetListRequest* request, GetListResponse* response) override;
    Status StreamUpdates(ServerContext* context, const StreamUpdatesRequest* request, ServerWriter<TodoUpdate>* writer) override;

    // Server management
    void RunServer(const std::string& server_address);
    void Shutdown();

private:
    // Data storage
    std::unordered_map<int32_t, TodoItem> todo_items_;
    std::atomic<int32_t> next_id_{1};
    std::mutex items_mutex_;

    // Real-time update system
    struct UpdateEvent {
        UpdateType type;
        TodoItem item;
    };
    
    std::queue<UpdateEvent> update_queue_;
    std::mutex queue_mutex_;
    std::condition_variable queue_cv_;
    std::atomic<bool> shutdown_flag_{false};
    std::thread update_thread_;

    // Helper methods
    void ProcessUpdates();
    void NotifyClients(const UpdateEvent& event);
    std::vector<TodoItem> GetAllItems();
};
