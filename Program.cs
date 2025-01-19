using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NetworkMonitorBackup.Services;
using System;
using System.Threading.Tasks;

namespace NetworkMonitorBackup
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            // Configure services
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            var services = new ServiceCollection()
                .AddSingleton<IConfiguration>(configuration)
                .AddHttpClient()
                .AddLogging(loggingBuilder =>
                {
                    loggingBuilder.ClearProviders();
                    loggingBuilder.AddConsole();
                    loggingBuilder.SetMinimumLevel(LogLevel.Information);
                })
                .AddTransient<IContaboService, ContaboService>()
                .AddTransient<SnapshotService>()
                .BuildServiceProvider();

            var logger = services.GetRequiredService<ILogger<Program>>();
            var snapshotService = services.GetRequiredService<SnapshotService>();

            while (true)
            {
                // Display menu
                Console.WriteLine("\nWelcome to Network Monitor Backup");
                Console.WriteLine("Select an option:");
                Console.WriteLine("1. List Instances");
                Console.WriteLine("2. List Snapshots");
                Console.WriteLine("3. Create Snapshot");
                Console.WriteLine("4. Delete Snapshot");
                Console.WriteLine("5. Refresh Snapshots");
                Console.WriteLine("6. Delete All Snapshots");
                Console.WriteLine("7. Display Instances with Snapshots");
                Console.WriteLine("8. Exit");
                Console.Write("Enter your choice: ");

                var choice = Console.ReadLine();

                try
                {
                    switch (choice)
                    {
                        case "1": // List Instances
                            var listInstancesReport = await snapshotService.ListInstancesAsync();
                            Console.WriteLine(listInstancesReport.Message);
                            break;

                        case "2": // List Snapshots
                            Console.Write("Enter Instance ID: ");
                            if (long.TryParse(Console.ReadLine(), out var instanceIdForSnapshots))
                            {
                                var listSnapshotsReport = await snapshotService.ListSnapshotsAsync(instanceIdForSnapshots);
                                Console.WriteLine(listSnapshotsReport.Message);
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

                                var createSnapshotReport = await snapshotService.CreateSnapshotAsync(instanceIdForSnapshotCreation, name, description);
                                Console.WriteLine(createSnapshotReport.Message);
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
                                    var deleteSnapshotReport = await snapshotService.DeleteSnapshotAsync(instanceIdForSnapshotDeletion, snapshotId);
                                    Console.WriteLine(deleteSnapshotReport.Message);
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
                            var refreshSnapshotsReport = await snapshotService.RefreshSnapshotsAsync();
                            Console.WriteLine(refreshSnapshotsReport.Message);
                            break;

                        case "6": // Delete All Snapshots
                            Console.Write("Enter Instance ID: ");
                            if (long.TryParse(Console.ReadLine(), out var instanceIdForDeleteAll))
                            {
                                var deleteAllSnapshotsReport = await snapshotService.DeleteAllSnapshotsAsync(instanceIdForDeleteAll);
                                Console.WriteLine(deleteAllSnapshotsReport.Message);
                            }
                            else
                            {
                                Console.WriteLine("Invalid Instance ID.");
                            }
                            break;

                        case "7": // Display Instances with Snapshots
                            var displayInstancesReport = await snapshotService.DisplayInstancesWithSnapshotsAsync();
                            Console.WriteLine(displayInstancesReport.Message);
                            break;

                        case "8": // Exit
                            Console.WriteLine("Exiting Network Monitor Backup. Goodbye!");
                            return;

                        default:
                            Console.WriteLine("Invalid option. Please select a number between 1 and 8.");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "An unexpected error occurred.");
                    Console.WriteLine($"An unexpected error occurred: {ex.Message}");
                }
            }
        }
    }
}
