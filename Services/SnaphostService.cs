using Microsoft.Extensions.Logging;
using NetworkMonitorBackup.Models;
using NetworkMonitorBackup.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text;

namespace NetworkMonitorBackup.Services
{
    public class SnapshotService
    {
        private readonly IContaboService _contaboService;
        private readonly ILogger<SnapshotService> _logger;

        public SnapshotService(IContaboService contaboService, ILogger<SnapshotService> logger)
        {
            _contaboService = contaboService;
            _logger = logger;
        }

        public async Task<ResultObj> ListInstancesAsync()
        {
            _logger.LogInformation("Listing instances...");
            var instancesResult = await _contaboService.ListInstancesAsync();

            if (!instancesResult.Success)
            {
                var errorMessage = $"Failed to list instances: {instancesResult.Message}";
                _logger.LogError(errorMessage);
                return new ResultObj(errorMessage, false);
            }

            var instanceResponse = (InstanceResponse)instancesResult.Data!;
            if (instanceResponse.Data.Count == 0)
            {
                var noInstancesMessage = "No instances found.";
                _logger.LogInformation(noInstancesMessage);
                return new ResultObj(noInstancesMessage, true);
            }

            var report = new StringBuilder();
            report.AppendLine("Instances:");
            foreach (var instance in instanceResponse.Data)
            {
                report.AppendLine($"- Instance ID: {instance.InstanceId}, Name: {instance.Name}, Status: {instance.Status}");
            }

            _logger.LogInformation("Instances listed successfully.");
            return new ResultObj(report.ToString(), true);
        }

        public async Task<ResultObj> ListSnapshotsAsync(long instanceId)
        {
            _logger.LogInformation("Listing snapshots for Instance ID: {InstanceId}", instanceId);
            var snapshotsResult = await _contaboService.ListSnapshotsAsync(instanceId);

            if (!snapshotsResult.Success)
            {
                var errorMessage = $"Failed to list snapshots for Instance ID: {instanceId}: {snapshotsResult.Message}";
                _logger.LogError(errorMessage);
                return new ResultObj(errorMessage, false);
            }

            var snapshotListResponse = (SnapshotListResponse)snapshotsResult.Data!;
            var snapshots = snapshotListResponse?.Snapshots;

            if (snapshots == null || snapshots.Count == 0)
            {
                var noSnapshotsMessage = $"No snapshots found for Instance ID: {instanceId}.";
                _logger.LogInformation(noSnapshotsMessage);
                return new ResultObj(noSnapshotsMessage, true);
            }

            var report = new StringBuilder();
            report.AppendLine($"Snapshots for Instance ID: {instanceId}:");
            foreach (var snapshot in snapshots)
            {
                report.AppendLine($"  - Snapshot ID: {snapshot.SnapshotId}");
                report.AppendLine($"    Name       : {snapshot.Name}");
                report.AppendLine($"    Description: {snapshot.Description}");
                report.AppendLine($"    CreatedDate: {snapshot.CreatedDate:yyyy-MM-dd HH:mm:ss}");
                report.AppendLine($"    AutoDelete : {(snapshot.AutoDeleteDate.HasValue ? snapshot.AutoDeleteDate.Value.ToString("yyyy-MM-dd HH:mm:ss") : "N/A")}");
            }

            _logger.LogInformation("Snapshots listed successfully for Instance ID: {InstanceId}", instanceId);
            return new ResultObj(report.ToString(), true);
        }

        public async Task<ResultObj> CreateSnapshotAsync(long instanceId, string name, string description)
        {
            _logger.LogInformation("Creating snapshot for Instance ID: {InstanceId}", instanceId);

            var createSnapshotResult = await _contaboService.CreateSnapshotAsync(instanceId, new SnapshotRequest
            {
                Name = name,
                Description = description
            });

            if (!createSnapshotResult.Success)
            {
                var errorMessage = $"Failed to create snapshot for Instance ID: {instanceId}: {createSnapshotResult.Message}";
                _logger.LogError(errorMessage);
                return new ResultObj(errorMessage, false);
            }

            var snapshot = (SnapshotResponse)createSnapshotResult.Data!;
            var report = $"Snapshot created successfully:\n  Snapshot ID: {snapshot.SnapshotId}\n  Name       : {snapshot.Name}\n  Description: {snapshot.Description}";
            _logger.LogInformation(report);
            return new ResultObj(report, true);
        }
        public async Task<ResultObj> DeleteSnapshotAsync(long instanceId, string snapshotId)
        {
            _logger.LogInformation("Deleting snapshot with ID: {SnapshotId} for Instance ID: {InstanceId}", snapshotId, instanceId);

            var deleteSnapshotResult = await _contaboService.DeleteSnapshotAsync(instanceId, snapshotId);

            if (!deleteSnapshotResult.Success)
            {
                var errorMessage = $"Failed to delete Snapshot ID: {snapshotId} for Instance ID: {instanceId}: {deleteSnapshotResult.Message}";
                _logger.LogError(errorMessage);
                return new ResultObj(errorMessage, false);
            }

            var report = $"Snapshot deleted successfully:\n  Snapshot ID: {snapshotId}\n  Instance ID: {instanceId}";
            _logger.LogInformation(report);
            return new ResultObj(report, true);
        }
        public async Task<ResultObj> RefreshSnapshotsAsync()
        {
            _logger.LogInformation("Starting snapshot refresh process...");

            // Initialize the report
            var report = new StringBuilder();
            report.AppendLine("Snapshot Refresh Report:");

            // Step 1: List all instances
            var instancesResult = await _contaboService.ListInstancesAsync();

            if (!instancesResult.Success)
            {
                var errorMessage = $"Failed to retrieve instances: {instancesResult.Message}";
                _logger.LogError(errorMessage);
                report.AppendLine(errorMessage);
                return new ResultObj(report.ToString(), false);
            }

            var instanceResponse = (InstanceResponse)instancesResult.Data!;
            if (instanceResponse.Data.Count == 0)
            {
                var noInstancesMessage = "No instances found.";
                _logger.LogInformation(noInstancesMessage);
                report.AppendLine(noInstancesMessage);
                return new ResultObj(report.ToString(), true);
            }

            foreach (var instance in instanceResponse.Data)
            {
                report.AppendLine($"\nProcessing Instance ID: {instance.InstanceId}, Name: {instance.Name}");
                _logger.LogInformation("Processing Instance ID: {InstanceId}, Name: {InstanceName}", instance.InstanceId, instance.Name);

                // Step 2: List snapshots for this instance
                var snapshotsResult = await _contaboService.ListSnapshotsAsync(instance.InstanceId);

                if (!snapshotsResult.Success)
                {
                    var warningMessage = $"Failed to retrieve snapshots for Instance ID: {instance.InstanceId}. Skipping to the next instance.";
                    _logger.LogWarning(warningMessage);
                    report.AppendLine($"  {warningMessage}");
                    continue;
                }

                var snapshotListResponse = (SnapshotListResponse)snapshotsResult.Data!;
                var snapshots = snapshotListResponse?.Snapshots;

                // Step 3: Handle instances with no snapshots
                if (snapshots == null || snapshots.Count == 0)
                {
                    var initializingMessage = $"No snapshots found for Instance ID: {instance.InstanceId}. Initializing a new snapshot.";
                    _logger.LogInformation(initializingMessage);
                    report.AppendLine($"  {initializingMessage}");
                    var initializeResult = await InitializeSnapshotAsync(instance);
                    report.AppendLine($"  {initializeResult.Message}");
                    continue;
                }

                // Step 4: Find and delete the oldest snapshot
                var oldestSnapshot = snapshots.OrderBy(s => s.CreatedDate).First();
                var oldestSnapshotInfo = $"Oldest Snapshot ID: {oldestSnapshot.SnapshotId}, Name: {oldestSnapshot.Name}, Description: {oldestSnapshot.Description}, CreatedDate: {oldestSnapshot.CreatedDate:yyyy-MM-dd HH:mm:ss}";
                _logger.LogInformation(oldestSnapshotInfo);
                report.AppendLine($"  {oldestSnapshotInfo}");

                var deleteResult = await _contaboService.DeleteSnapshotAsync(instance.InstanceId, oldestSnapshot.SnapshotId);

                if (!deleteResult.Success)
                {
                    var deleteWarning = $"Failed to delete Snapshot ID: {oldestSnapshot.SnapshotId} for Instance ID: {instance.InstanceId}. Skipping creation of a new snapshot.";
                    _logger.LogWarning(deleteWarning);
                    report.AppendLine($"  {deleteWarning}");
                    continue;
                }

                var deleteSuccess = $"Deleted Snapshot ID: {oldestSnapshot.SnapshotId} for Instance ID: {instance.InstanceId}. Proceeding to create a new snapshot.";
                _logger.LogInformation(deleteSuccess);
                report.AppendLine($"  {deleteSuccess}");

                // Step 5: Create a new snapshot with the same name and description
                var createResult = await _contaboService.CreateSnapshotAsync(instance.InstanceId, new SnapshotRequest
                {
                    Name = oldestSnapshot.Name,
                    Description = oldestSnapshot.Description
                });

                if (createResult.Success)
                {
                    var newSnapshot = (SnapshotResponse)createResult.Data!;
                    var creationSuccess = $"Created new Snapshot ID: {newSnapshot.SnapshotId} for Instance ID: {instance.InstanceId}.";
                    _logger.LogInformation(creationSuccess);
                    report.AppendLine($"  {creationSuccess}");
                }
                else
                {
                    var creationError = $"Failed to create a new snapshot for Instance ID: {instance.InstanceId}.";
                    _logger.LogError(creationError);
                    report.AppendLine($"  {creationError}");
                }
            }

            var completionMessage = "Snapshot refresh process completed.";
            _logger.LogInformation(completionMessage);
            report.AppendLine($"\n{completionMessage}");

            return new ResultObj(report.ToString(), true);
        }
        public async Task<ResultObj> DeleteAllSnapshotsAsync(long instanceId)
        {
            _logger.LogInformation("Deleting all snapshots for Instance ID: {InstanceId}", instanceId);

            var report = new StringBuilder();
            report.AppendLine($"Delete All Snapshots Report for Instance ID: {instanceId}");

            // Step 1: List all snapshots for the instance
            var snapshotsResult = await _contaboService.ListSnapshotsAsync(instanceId);

            if (!snapshotsResult.Success)
            {
                var errorMessage = $"Failed to retrieve snapshots for Instance ID: {instanceId}.";
                _logger.LogError(errorMessage);
                report.AppendLine(errorMessage);
                return new ResultObj(report.ToString(), false);
            }

            var snapshotListResponse = (SnapshotListResponse)snapshotsResult.Data!;
            var snapshots = snapshotListResponse?.Snapshots;

            if (snapshots == null || snapshots.Count == 0)
            {
                var noSnapshotsMessage = $"No snapshots found for Instance ID: {instanceId}. Nothing to delete.";
                _logger.LogInformation(noSnapshotsMessage);
                report.AppendLine(noSnapshotsMessage);
                return new ResultObj(report.ToString(), true);
            }

            // Step 2: Iterate through snapshots and delete each one
            foreach (var snapshot in snapshots)
            {
                var deletingMessage = $"Deleting Snapshot ID: {snapshot.SnapshotId}, Name: {snapshot.Name} for Instance ID: {instanceId}";
                _logger.LogInformation(deletingMessage);
                report.AppendLine($"  {deletingMessage}");

                var deleteResult = await _contaboService.DeleteSnapshotAsync(instanceId, snapshot.SnapshotId);

                if (deleteResult.Success)
                {
                    var successMessage = $"Successfully deleted Snapshot ID: {snapshot.SnapshotId} for Instance ID: {instanceId}.";
                    _logger.LogInformation(successMessage);
                    report.AppendLine($"  {successMessage}");
                }
                else
                {
                    var errorMessage = $"Failed to delete Snapshot ID: {snapshot.SnapshotId} for Instance ID: {instanceId}. Error: {deleteResult.Message}";
                    _logger.LogError(errorMessage);
                    report.AppendLine($"  {errorMessage}");
                }
            }

            var completionMessage = $"Completed deleting all snapshots for Instance ID: {instanceId}.";
            _logger.LogInformation(completionMessage);
            report.AppendLine($"\n{completionMessage}");

            return new ResultObj(report.ToString(), true);
        }

        public async Task<ResultObj> DisplayInstancesWithSnapshotsAsync()
        {
            _logger.LogInformation("Retrieving all instances and their snapshots...");

            var report = new StringBuilder();
            report.AppendLine("Instances and Snapshots Report:");

            // Step 1: List all instances
            var instancesResult = await _contaboService.ListInstancesAsync();

            if (!instancesResult.Success)
            {
                var errorMessage = $"Failed to retrieve instances: {instancesResult.Message}";
                _logger.LogError(errorMessage);
                report.AppendLine(errorMessage);
                return new ResultObj(report.ToString(), false);
            }

            var instanceResponse = (InstanceResponse)instancesResult.Data!;

            // Step 2: Iterate through instances and retrieve snapshots
            foreach (var instance in instanceResponse.Data)
            {
                report.AppendLine($"\nInstance ID: {instance.InstanceId}, Name: {instance.Name}, Status: {instance.Status}");
                _logger.LogInformation("Processing Instance ID: {InstanceId}, Name: {InstanceName}", instance.InstanceId, instance.Name);

                var snapshotsResult = await _contaboService.ListSnapshotsAsync(instance.InstanceId);

                if (snapshotsResult.Success)
                {
                    var snapshotListResponse = (SnapshotListResponse)snapshotsResult.Data!;
                    var snapshots = snapshotListResponse?.Snapshots;

                    if (snapshots != null && snapshots.Count > 0)
                    {
                        foreach (var snapshot in snapshots)
                        {
                            var snapshotInfo = $"    - Snapshot ID: {snapshot.SnapshotId}, Name: {snapshot.Name}, Description: {snapshot.Description}, CreatedDate: {snapshot.CreatedDate:yyyy-MM-dd HH:mm:ss}";
                            _logger.LogInformation(snapshotInfo);
                            report.AppendLine(snapshotInfo);
                        }
                    }
                    else
                    {
                        var noSnapshotsMessage = "    (No snapshots found)";
                        _logger.LogInformation(noSnapshotsMessage);
                        report.AppendLine(noSnapshotsMessage);
                    }
                }
                else
                {
                    var errorMessage = "    (Failed to retrieve snapshots for this instance)";
                    _logger.LogWarning(errorMessage);
                    report.AppendLine(errorMessage);
                }
            }

            var completionMessage = "Completed retrieving instances and their snapshots.";
            _logger.LogInformation(completionMessage);
            report.AppendLine($"\n{completionMessage}");

            return new ResultObj(report.ToString(), true);
        }
        private async Task<ResultObj> InitializeSnapshotAsync(InstanceData instance)
        {
            _logger.LogInformation("Initializing snapshot for Instance ID: {InstanceId}, Name: {InstanceName}", instance.InstanceId, instance.Name);

            var name = $"{instance.Name}-Initial-Snapshot".Replace("_", "-");
            var description = $"Auto created by NetworkMonitorBackup for instance {instance.Name} (ID: {instance.InstanceId}).";

            var createResult = await _contaboService.CreateSnapshotAsync(instance.InstanceId, new SnapshotRequest
            {
                Name = name,
                Description = description
            });

            if (createResult.Success)
            {
                var newSnapshot = (SnapshotResponse)createResult.Data!;
                _logger.LogInformation("Initialized Snapshot ID: {SnapshotId} for Instance ID: {InstanceId}.", newSnapshot.SnapshotId, instance.InstanceId);
                return new ResultObj($"Initialized Snapshot ID: {newSnapshot.SnapshotId} for Instance ID: {instance.InstanceId}.", true);
            }
            else
            {
                _logger.LogError("Failed to initialize snapshot for Instance ID: {InstanceId}.", instance.InstanceId);
                return new ResultObj($"Failed to initialize snapshot for Instance ID: {instance.InstanceId}.", false);
            }
        }

    }
}
