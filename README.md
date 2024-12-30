# File Process Application

This application implements a basic file process designed to handle COVID-related files. It supports file uploading, processing, data aggregation, and storage into multiple databases, while also providing an endpoint for retrieving the aggregated summary.

The application uses **.NET 9** along with various resources like: 

- Azure Blob Storage (Emulator)
- Azure Service Bus (Emulator)
- MongoDB
- PostgreSQL
- Redis

## Prerequisites
- .NET 9 SDK
- .NET Aspire
- Docker

## .NET Aspire
.NET Aspire is designed to improve the experience of building .NET cloud-native apps. It provides a consistent, opinionated set of tools and patterns that help you build and run distributed apps.

.NET Aspire is designed to help you with:

- Orchestration: .NET Aspire provides features for running and connecting multi-project applications and their dependencies for local development environments.
- Integrations: .NET Aspire integrations are NuGet packages for commonly used services, such as Redis or Postgres, with standardized interfaces ensuring they connect consistently and seamlessly with your app.
- Tooling: .NET Aspire comes with project templates and tooling experiences for Visual Studio, Visual Studio Code, and the .NET CLI to help you create and interact with .NET Aspire projects.

## Features

### **File Collector Service (Background Service):**
- **Automatic Upload**: A background service monitors a directory for new files. When a new file is detected, it is uploaded to **Azure Blob Storage**, and a message is sent to the **Azure Service Bus Queue**.
- **Typed HttpClient**: Uses a typed HttpClient `FileUploaderClient` to upload files through the API.
- **Azure Blob Storage**: Used for storing uploaded files.
- **Azure Service Bus Queue**: Used to send messages upon file upload for `File Details Service` processing.

### **File Details Service (Background Service):**
- **Message Processing**: Reads messages from the Azure Service Bus queue for file processing.
- **File Processing**: Downloads files from Blob Storage and reads them using the **CsvHelper** package.
- **Data Insertion**: Inserts detailed data into **MongoDB**.
- **Message Forwarding**: Sends a message to the **File Summary Service** once processing is complete.

### **File Summary Service (Background Service):**
- **Message Processing**: Reads messages from the queue for aggregated data processing.
- **Data Aggregation**: Aggregates values from **MongoDB**.
- **Data Insertion**: Saves the aggregated data into **PostgreSQL**.
- **Caching**: Caches the aggregated summary data into **Redis** for fast access.

### **Observability**

With **.NET Aspire**, we can get the full observability of your application and services, including:

- **MongoExpress** Dashboard: A web-based interface for managing **MongoDB** and viewing your stored data.
- **Postgres-PgAdmin / PgWeb Dashboards**: Web-based interfaces to interact with your **PostgreSQL** database.
- **Redis Cache Insights**: Visualize and manage the **Redis** cache with built-in dashboards, providing deep insights into cache usage and performance.

### Local Development

1. **Clone the repository:**
   ```bash
   git clone https://github.com/dianper/microservice-aspire.git
   cd microservice-aspire
   ```

2. **Set up Docker containers for required services**:
   - Docker is required to run the application dependencies.

3. **Run the application**:
   ```bash
   dotnet run
   ```

### API Endpoints

#### **GET** `/api/summaries`

Retrieve the global summary. The service checks **Redis** for cached data first; if not found, it queries **PostgreSQL**.

#### **POST** `/api/files`

Manually upload a CSV file to be processed.

- **Request body**: Multipart form-data containing the file.
