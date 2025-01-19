using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NetworkMonitorBackup.Services;

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
               .AddLogging(loggingBuilder =>
                {
                    loggingBuilder.ClearProviders(); // Remove default providers
                    loggingBuilder.AddConsole(); // Add console logging
                    loggingBuilder.SetMinimumLevel(LogLevel.Information); // Set minimum log level
                })
                .AddTransient<IContaboService, ContaboService>()
                .AddTransient<SnapshotService>()
                .BuildServiceProvider();

            var logger = services.GetRequiredService<ILogger<Program>>();
            var snapshotService = services.GetRequiredService<SnapshotService>();

            Console.WriteLine("1. List Instances\n2. List Snapshots\n3. Create Snapshot");
            var choice = Console.ReadLine();

            try
            {
                switch (choice)
                {
                    case "1": // List Instances
                        await snapshotService.ListInstancesAsync();
                        break;

                    case "2": // List Snapshots
                        Console.Write("Enter Instance ID: ");
                        if (long.TryParse(Console.ReadLine(), out var instanceIdForSnapshots))
                        {
                            await snapshotService.ListSnapshotsAsync(instanceIdForSnapshots);
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

                            await snapshotService.CreateSnapshotAsync(instanceIdForSnapshotCreation, name, description);
                        }
                        else
                        {
                            Console.WriteLine("Invalid Instance ID.");
                        }
                        break;
                    case "4": // Delete Snapshot
                        Console.Write("Enter Instance ID: ");
                        if (long.TryParse(Console.ReadLine(), out var instanceIdForSnapshotDeletion))
                        {
                            Console.Write("Enter Snapshot ID: ");
                            var snapshotId = Console.ReadLine();

                            if (!string.IsNullOrEmpty(snapshotId))
                            {
                                await snapshotService.DeleteSnapshotAsync(instanceIdForSnapshotDeletion, snapshotId);
                            }
                            else
                            {
                                Console.WriteLine("Invalid Snapshot ID.");
                            }
                        }
                        else
                        {
                            Console.WriteLine("Invalid Instance ID.");
                        }
                        break;
                      case "5": // Refresh Snapshots
            await snapshotService.RefreshSnapshotsAsync();
            break;

                    default:
                        Console.WriteLine("Invalid option. Please select 1, 2, 3, 4, or 5.");
                        break;
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An unexpected error occurred.");
                Console.WriteLine("An unexpected error occurred: " + ex.Message);
            }
        }
    }
}
