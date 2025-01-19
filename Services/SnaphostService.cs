using Microsoft.Extensions.Logging;
using NetworkMonitorBackup.Models;
using NetworkMonitorBackup.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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

        public async Task ListInstancesAsync()
        {
            _logger.LogInformation("Listing instances...");
            var instancesResult = await _contaboService.ListInstancesAsync();

            if (instancesResult.Success)
            {
                var instanceResponse = (InstanceResponse)instancesResult.Data!;
                foreach (var instance in instanceResponse.Data)
                {
                    _logger.LogInformation($"ID: {instance.InstanceId}, Name: {instance.Name}, Status: {instance.Status}");
                }
            }
            else
            {
                _logger.LogError("Failed to list instances: {Message}", instancesResult.Message);
            }
        }

        public async Task ListSnapshotsAsync(long instanceId)
        {
            _logger.LogInformation("Listing snapshots for Instance ID: {InstanceId}", instanceId);
            var snapshotsResult = await _contaboService.ListSnapshotsAsync(instanceId);

            if (snapshotsResult.Success)
            {
                var snapshotListResponse = (SnapshotListResponse)snapshotsResult.Data!;
                if (snapshotListResponse?.Snapshots != null && snapshotListResponse.Snapshots.Count > 0)
                {
                    foreach (var snapshot in snapshotListResponse.Snapshots)
                    {
                        _logger.LogInformation($"Snapshot ID: {snapshot.SnapshotId}, Name: {snapshot.Name}, Description: {snapshot.Description}");
                    }
                }
                else
                {
                    _logger.LogWarning("No snapshots found for Instance ID: {InstanceId}", instanceId);
                }
            }
            else
            {
                _logger.LogError("Failed to list snapshots: {Message}", snapshotsResult.Message);
            }
        }

        public async Task CreateSnapshotAsync(long instanceId, string name, string description)
        {
            _logger.LogInformation("Creating snapshot for Instance ID: {InstanceId}", instanceId);

            var createSnapshotResult = await _contaboService.CreateSnapshotAsync(instanceId, new SnapshotRequest
            {
                Name = name,
                Description = description
            });

            if (createSnapshotResult.Success)
            {
                var snapshot = (SnapshotResponse)createSnapshotResult.Data!;
                _logger.LogInformation("Snapshot created successfully: {SnapshotId}", snapshot.SnapshotId);
            }
            else
            {
                _logger.LogError("Failed to create snapshot: {Message}", createSnapshotResult.Message);
            }
        }
        public async Task DeleteSnapshotAsync(long instanceId, string snapshotId)
        {
            _logger.LogInformation("Deleting snapshot with ID: {SnapshotId} for Instance ID: {InstanceId}", snapshotId, instanceId);

            var deleteSnapshotResult = await _contaboService.DeleteSnapshotAsync(instanceId, snapshotId);

            if (deleteSnapshotResult.Success)
            {
                _logger.LogInformation("Snapshot deleted successfully: {SnapshotId}", snapshotId);
            }
            else
            {
                _logger.LogError("Failed to delete snapshot: {Message}", deleteSnapshotResult.Message);
            }
        }
public async Task RefreshSnapshotsAsync()
{
    _logger.LogInformation("Starting snapshot refresh process...");

    // Step 1: List all instances
    var instancesResult = await _contaboService.ListInstancesAsync();

    if (!instancesResult.Success)
    {
        _logger.LogError("Failed to retrieve instances: {Message}", instancesResult.Message);
        return;
    }

    var instanceResponse = (InstanceResponse)instancesResult.Data!;
    foreach (var instance in instanceResponse.Data)
    {
        _logger.LogInformation("Processing Instance ID: {InstanceId}, Name: {InstanceName}", instance.InstanceId, instance.Name);

        // Step 2: List snapshots for this instance
        var snapshotsResult = await _contaboService.ListSnapshotsAsync(instance.InstanceId);

        if (!snapshotsResult.Success)
        {
            _logger.LogWarning("Failed to retrieve snapshots for Instance ID: {InstanceId}. Skipping to the next instance.", instance.InstanceId);
            continue;
        }

        var snapshotListResponse = (SnapshotListResponse)snapshotsResult.Data!;
        var snapshots = snapshotListResponse?.Snapshots;

        if (snapshots == null || snapshots.Count == 0)
        {
            _logger.LogInformation("No snapshots found for Instance ID: {InstanceId}. Skipping to the next instance.", instance.InstanceId);
            continue;
        }

        // Step 3: Find the oldest snapshot
        var oldestSnapshot = snapshots.OrderBy(s => s.CreatedAt).First();
        _logger.LogInformation("Oldest Snapshot ID: {SnapshotId}, Name: {SnapshotName}, Description: {SnapshotDescription}, CreatedAt: {CreatedAt}",
            oldestSnapshot.SnapshotId, oldestSnapshot.Name, oldestSnapshot.Description, oldestSnapshot.CreatedAt);

        // Step 4: Delete the oldest snapshot
        var deleteResult = await _contaboService.DeleteSnapshotAsync(instance.InstanceId, oldestSnapshot.SnapshotId);

        if (!deleteResult.Success)
        {
            _logger.LogWarning("Failed to delete Snapshot ID: {SnapshotId} for Instance ID: {InstanceId}. Skipping creation of a new snapshot.", 
                oldestSnapshot.SnapshotId, instance.InstanceId);
            continue;
        }

        _logger.LogInformation("Deleted Snapshot ID: {SnapshotId} for Instance ID: {InstanceId}. Proceeding to create a new snapshot.",
            oldestSnapshot.SnapshotId, instance.InstanceId);

        // Step 5: Create a new snapshot with the same name and description
        var createResult = await _contaboService.CreateSnapshotAsync(instance.InstanceId, new SnapshotRequest
        {
            Name = oldestSnapshot.Name,
            Description = oldestSnapshot.Description
        });

        if (createResult.Success)
        {
            var newSnapshot = (SnapshotResponse)createResult.Data!;
            _logger.LogInformation("Created new Snapshot ID: {SnapshotId} for Instance ID: {InstanceId}.", newSnapshot.SnapshotId, instance.InstanceId);
        }
        else
        {
            _logger.LogError("Failed to create a new snapshot for Instance ID: {InstanceId}.", instance.InstanceId);
        }
    }

    _logger.LogInformation("Snapshot refresh process completed.");
}


    }
}
