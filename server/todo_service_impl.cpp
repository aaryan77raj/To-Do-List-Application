#include "todo_service_impl.h"
#include <iostream>
#include <chrono>

TodoServiceImpl::TodoServiceImpl() : update_thread_(&TodoServiceImpl::ProcessUpdates, this) {
    std::cout << "TodoService initialized" << std::endl;
}

TodoServiceImpl::~TodoServiceImpl() {
    Shutdown();
}

Status TodoServiceImpl::AddItem(ServerContext* context, const AddItemRequest* request, AddItemResponse* response) {
    std::lock_guard<std::mutex> lock(items_mutex_);
    
    // Create new todo item
    TodoItem item;
    item.set_id(next_id_.fetch_add(1));
    item.set_description(request->description());
    item.set_status(TodoStatus::PENDING);
    
    // Store the item
    todo_items_[item.id()] = item;
    
    // Set response
    response->set_id(item.id());
    response->set_description(item.description());
    response->set_status(item.status());
    
    // Queue update for real-time notification
    UpdateEvent event;
    event.type = UpdateType::ITEM_ADDED;
    event.item = item;
    
    {
        std::lock_guard<std::mutex> queue_lock(queue_mutex_);
        update_queue_.push(event);
    }
    queue_cv_.notify_all();
    
    std::cout << "Added item: " << item.description() << " (ID: " << item.id() << ")" << std::endl;
    return Status::OK;
}

Status TodoServiceImpl::UpdateStatus(ServerContext* context, const UpdateStatusRequest* request, UpdateStatusResponse* response) {
    std::lock_guard<std::mutex> lock(items_mutex_);
    
    auto it = todo_items_.find(request->id());
    if (it == todo_items_.end()) {
        return Status(grpc::StatusCode::NOT_FOUND, "Todo item not found");
    }
    
    // Toggle status
    TodoStatus new_status = (it->second.status() == TodoStatus::PENDING) ? 
                           TodoStatus::COMPLETED : TodoStatus::PENDING;
    it->second.set_status(new_status);
    
    // Set response
    response->set_success(true);
    response->set_new_status(new_status);
    
    // Queue update for real-time notification
    UpdateEvent event;
    event.type = UpdateType::ITEM_UPDATED;
    event.item = it->second;
    
    {
        std::lock_guard<std::mutex> queue_lock(queue_mutex_);
        update_queue_.push(event);
    }
    queue_cv_.notify_all();
    
    std::cout << "Updated item " << request->id() << " to " 
              << (new_status == TodoStatus::COMPLETED ? "COMPLETED" : "PENDING") << std::endl;
    return Status::OK;
}

Status TodoServiceImpl::GetList(ServerContext* context, const GetListRequest* request, GetListResponse* response) {
    std::lock_guard<std::mutex> lock(items_mutex_);
    
    auto items = GetAllItems();
    for (const auto& item : items) {
        TodoItem* response_item = response->add_items();
        *response_item = item;
    }
    
    std::cout << "Retrieved " << items.size() << " todo items" << std::endl;
    return Status::OK;
}

Status TodoServiceImpl::StreamUpdates(ServerContext* context, const StreamUpdatesRequest* request, ServerWriter<TodoUpdate>* writer) {
    std::cout << "Client connected for real-time updates" << std::endl;
    
    // Send current state first
    {
        std::lock_guard<std::mutex> lock(items_mutex_);
        auto items = GetAllItems();
        for (const auto& item : items) {
            TodoUpdate update;
            update.set_type(UpdateType::ITEM_ADDED);
            TodoItem* update_item = update.mutable_item();
            *update_item = item;
            
            if (!writer->Write(update)) {
                std::cout << "Client disconnected during initial sync" << std::endl;
                return Status::OK;
            }
        }
    }
    
    // Wait for updates
    while (!context->IsCancelled() && !shutdown_flag_.load()) {
        std::unique_lock<std::mutex> lock(queue_mutex_);
        
        if (queue_cv_.wait_for(lock, std::chrono::milliseconds(100), [this] { return !update_queue_.empty() || shutdown_flag_.load(); })) {
            while (!update_queue_.empty()) {
                UpdateEvent event = update_queue_.front();
                update_queue_.pop();
                lock.unlock();
                
                TodoUpdate update;
                update.set_type(event.type);
                TodoItem* update_item = update.mutable_item();
                *update_item = event.item;
                
                if (!writer->Write(update)) {
                    std::cout << "Client disconnected" << std::endl;
                    return Status::OK;
                }
                
                lock.lock();
            }
        }
    }
    
    std::cout << "Client disconnected from updates stream" << std::endl;
    return Status::OK;
}

void TodoServiceImpl::ProcessUpdates() {
    // This method is called by the update thread
    // The actual processing is done in StreamUpdates method
    while (!shutdown_flag_.load()) {
        std::this_thread::sleep_for(std::chrono::milliseconds(100));
    }
}

void TodoServiceImpl::NotifyClients(const UpdateEvent& event) {
    // This method is called when we want to notify all clients
    // The actual notification is handled in StreamUpdates
}

std::vector<TodoItem> TodoServiceImpl::GetAllItems() {
    std::vector<TodoItem> items;
    for (const auto& pair : todo_items_) {
        items.push_back(pair.second);
    }
    return items;
}

void TodoServiceImpl::RunServer(const std::string& server_address) {
    ServerBuilder builder;
    builder.AddListeningPort(server_address, grpc::InsecureServerCredentials());
    builder.RegisterService(this);
    
    std::unique_ptr<Server> server(builder.BuildAndStart());
    std::cout << "Server listening on " << server_address << std::endl;
    
    server->Wait();
}

void TodoServiceImpl::Shutdown() {
    shutdown_flag_.store(true);
    queue_cv_.notify_all();
    
    if (update_thread_.joinable()) {
        update_thread_.join();
    }
}
