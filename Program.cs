using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NetworkMonitorBackup.Services;
using NetworkMonitorBackup.Models;
using NetworkMonitor.Objects;

namespace NetworkMonitorBackup
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            var services = new ServiceCollection()
                .AddSingleton<IConfiguration>(configuration)
                .AddHttpClient()
                .AddLogging()
                .AddTransient<IContaboService, ContaboService>()
                .BuildServiceProvider();

            var logger = services.GetRequiredService<ILogger<Program>>();
            var contaboService = services.GetRequiredService<IContaboService>();

            Console.WriteLine("1. List Instances\n2. List Snapshots\n3. Create Snapshot");
            var choice = Console.ReadLine();

            switch (choice)
            {
                case "1": // List Instances
                    var instancesResult = await contaboService.ListInstancesAsync();

                    if (instancesResult.Success)
                    {
                        var instanceResponse = (InstanceResponse)instancesResult.Data!;
                        foreach (var instance in instanceResponse.Data)
                        {
                            Console.WriteLine($"ID: {instance.InstanceId}, Name: {instance.Name}, Status: {instance.Status}");
                        }
                    }
                    else
                    {
                        logger.LogError(instancesResult.Message);
                        Console.WriteLine(instancesResult.Message);
                    }
                    break;

                case "2": // List Snapshots
                    Console.Write("Enter Instance ID: ");
                    if (long.TryParse(Console.ReadLine(), out var instanceIdForSnapshots))
                    {
                        var snapshotsResult = await contaboService.ListSnapshotsAsync(instanceIdForSnapshots);

                        if (snapshotsResult.Success)
                        {
                            var snapshots = (List<SnapshotResponse>)snapshotsResult.Data!;
                            foreach (var snapshot in snapshots)
                            {
                                Console.WriteLine($"{snapshot.SnapshotId}: {snapshot.Name} - {snapshot.Description}");
                            }
                        }
                        else
                        {
                            logger.LogError(snapshotsResult.Message);
                            Console.WriteLine(snapshotsResult.Message);
                        }
                    }
                    else
                    {
                        Console.WriteLine("Invalid Instance ID.");
                    }
                    break;

                case "3": // Create Snapshot
                    Console.Write("Enter Instance ID: ");
                    if (long.TryParse(Console.ReadLine(), out var instanceIdForSnapshotCreation))
                    {
                        Console.Write("Enter Snapshot Name: ");
                        var name = Console.ReadLine();

                        Console.Write("Enter Snapshot Description: ");
                        var description = Console.ReadLine();

                        var createSnapshotResult = await contaboService.CreateSnapshotAsync(instanceIdForSnapshotCreation, new SnapshotRequest
                        {
                            Name = name,
                            Description = description
                        });

                        if (createSnapshotResult.Success)
                        {
                            var snapshot = (SnapshotResponse)createSnapshotResult.Data!;
                            Console.WriteLine($"Snapshot Created: {snapshot.SnapshotId}");
                        }
                        else
                        {
                            logger.LogError(createSnapshotResult.Message);
                            Console.WriteLine(createSnapshotResult.Message);
                        }
                    }
                    else
                    {
                        Console.WriteLine("Invalid Instance ID.");
                    }
                    break;

                default:
                    Console.WriteLine("Invalid option. Please select 1, 2, or 3.");
                    break;
            }
        }
    }
}
