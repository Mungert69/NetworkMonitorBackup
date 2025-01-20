# Network Monitor Backup

A lightweight and efficient tool for managing cloud snapshots, designed to simplify instance and snapshot management for cloud services. This tool provides a user-friendly CLI for listing, creating, and deleting snapshots, along with built-in logging for debugging and monitoring.

---

## Features

- **List All Instances**: Retrieve and display all instances in your account.
- **Snapshot Management**:
  - List snapshots for a specific instance.
  - Create new snapshots with a name and description.
  - Delete specific snapshots or all snapshots for an instance.
- **Refresh Snapshots**: Automate snapshot lifecycle by deleting the oldest snapshot and creating a new one.
- **Logging**:
  - File-based logging using Serilog for persistence.
  - Console-based logging with styled output for real-time monitoring.

---

## Installation

### Prerequisites

- .NET 6.0 or later installed on your machine
- Access credentials for your cloud provider's API

### Steps

1. Clone the repository:
   ```bash
   git clone https://github.com/mahadeva/network-monitor-backup.git
   cd network-monitor-backup
   ```

2. Restore dependencies:
   ```bash
   dotnet restore
   ```

3. Build the project:
   ```bash
   dotnet build
   ```

4. Run the project:
   ```bash
   dotnet run
   ```

---

## Configuration

Update the `appsettings.json` file with your cloud provider's API credentials:

```json
{
  "Contabo": {
    "ClientId": "your-client-id",
    "ClientSecret": "your-client-secret",
    "ApiUser": "your-api-user",
    "ApiPassword": "your-api-password"
  }
}
```

---

## Usage

1. Run the application:
   ```bash
   dotnet run
   ```

2. Follow the menu prompts to manage your instances and snapshots.

---

## Logging

### File Logging

Logs are saved in the `logs` directory, with a new file created daily:
```
logs/network_monitor_backup.log
```

### Console Logging

Real-time logs are displayed in the console with the following color-coded levels:
- **INFO**: White
- **SUCCESS**: Green
- **WARNING**: Yellow
- **ERROR**: Red
- **DEBUG**: Gray

---

## Contributing

Contributions are welcome! To contribute:

1. Fork the repository.
2. Create a feature branch:
   ```bash
   git checkout -b feature-name
   ```
3. Commit your changes:
   ```bash
   git commit -m "Description of your changes"
   ```
4. Push your branch:
   ```bash
   git push origin feature-name
   ```
5. Open a pull request.

---

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.

---

## Acknowledgments

- [Newtonsoft.Json](https://www.newtonsoft.com/json) for JSON parsing.
- [Serilog](https://serilog.net/) for advanced logging features.
- [Microsoft.Extensions.DependencyInjection](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection) for DI.

---

## Used By

This software is used by [Free Network Monitor](https://freenetworkmonitor.click) to enhance snapshot and instance management capabilities.

---

## Issues

If you encounter any issues, please report them [here](https://github.com/mahadeva/network-monitor-backup/issues).

---

## Author

Developed and maintained by **Mahadeva**  
Email: [contact@mahadeva.co](mailto:contact@mahadeva.co)

