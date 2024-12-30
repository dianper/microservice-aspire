# .NET Aspire Application

## .NET Aspire Overview

.NET Aspire is designed to enhance the development of cloud-native .NET applications by providing a consistent set of tools and patterns for building and running distributed apps. It helps developers with:

- **Orchestration**: Simplifies the management of multi-project applications and their dependencies in local development environments.
- **Integrations**: Offers NuGet packages for common services (e.g., Redis, PostgreSQL) with standardized interfaces for seamless app integration.
- **Tooling**: Provides project templates and tooling support for Visual Studio, Visual Studio Code, and the .NET CLI, making it easier to create and work with .NET Aspire projects.

## Introduction
This application implements a basic file process designed to handle COVID-related files. It supports file uploading, processing, data aggregation, and storage into multiple databases, while also providing an endpoint for retrieving the aggregated summary.

The application uses various resources like:

- Azure Blob Storage (Emulator)
- Azure Service Bus (Emulator)
- MongoDB
- PostgreSQL
- Redis

## Prerequisites
- .NET 9 SDK
- .NET Aspire
- Docker

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

## Deployment

Let's use Aspir8 to generate the Docker Compose file and Helm chart for the application.

1. **Install Aspir8**:
   ```bash
   dotnet tool install -g aspirate
   ```

2. **Generating Docker Compose**:
   ```bash
   aspirate generate --output-format compose
   ```

3. **Generating Helm Chart**:
   ```bash
   aspirate generate --output-format helm
   ```

## References
- [.NET Aspire](https://learn.microsoft.com/en-us/dotnet/aspire/get-started/aspire-overview)
- [Aspir8](https://prom3theu5.github.io/aspirational-manifests/getting-started.html)
- [CsvHelper](https://joshclose.github.io/CsvHelper/)
