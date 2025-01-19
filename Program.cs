using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NetworkMonitorBackup.Services;
using NetworkMonitorBackup.Models;
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
                // Level 1: Manage instances
                Console.Clear();
                Console.WriteLine("\n=== Network Monitor Backup (Level 1) ===");
                Console.WriteLine("1. List All Instances");
                Console.WriteLine("2. Display Instances with Snapshots");
                Console.WriteLine("3. Delete All Snapshots (All Instances)");
                Console.WriteLine("4. Refresh Snapshots (All Instances)");
                Console.WriteLine("5. Select an Instance to Manage");
                Console.WriteLine("6. Exit");
                Console.Write("Enter your choice: ");

                var choice = Console.ReadLine();

                try
                {
                    switch (choice)
                    {
                        case "1": // List All Instances
                            var listInstancesReport = await snapshotService.ListInstancesAsync();
                            DisplayResult(listInstancesReport);
                            break;

                        case "2": // Display Instances with Snapshots
                            var displayInstancesReport = await snapshotService.DisplayInstancesWithSnapshotsAsync();
                            DisplayResult(displayInstancesReport);
                            break;

                        case "3": // Delete All Snapshots
                            Console.WriteLine("Deleting all snapshots for all instances...");
                            var deleteAllReport = await snapshotService.DeleteAllSnapshotsAsync();
                            DisplayResult(deleteAllReport);
                            break;

                        case "4": // Refresh Snapshots
                            Console.WriteLine("Refreshing snapshots for all instances...");
                            var refreshAllReport = await snapshotService.RefreshSnapshotsAsync();
                            DisplayResult(refreshAllReport);
                            break;

                        case "5": // Select an Instance to Manage
                            Console.Write("Enter Instance ID: ");
                            if (long.TryParse(Console.ReadLine(), out var instanceId))
                            {
                                await ManageInstance(snapshotService, instanceId);
                            }
                            else
                            {
                                Console.WriteLine("Invalid Instance ID.");
                            }
                            break;

                        case "6": // Exit
                            Console.WriteLine("Exiting Network Monitor Backup. Goodbye!");
                            return;

                        default:
                            Console.WriteLine("Invalid option. Please select a number between 1 and 6.");
                            break;
                    }

                    Console.WriteLine("\nPress Enter to return to the menu...");
                    Console.ReadLine();
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "An unexpected error occurred.");
                    Console.WriteLine($"An unexpected error occurred: {ex.Message}");
                    Console.WriteLine("\nPress Enter to return to the menu...");
                    Console.ReadLine();
                }
            }
        }

        private static async Task ManageInstance(SnapshotService snapshotService, long instanceId)
        {
            while (true)
            {
                // Level 2: Manage snapshots for a selected instance
                Console.Clear();
                Console.WriteLine($"\n=== Managing Instance ID: {instanceId} (Level 2) ===");
                Console.WriteLine("1. List Snapshots");
                Console.WriteLine("2. Create Snapshot");
                Console.WriteLine("3. Delete Snapshot");
                Console.WriteLine("4. Return to Main Menu");
                Console.Write("Enter your choice: ");

                var choice = Console.ReadLine();

                try
                {
                    switch (choice)
                    {
                        case "1": // List Snapshots
                            var listSnapshotsReport = await snapshotService.ListSnapshotsAsync(instanceId);
                            DisplayResult(listSnapshotsReport);
                            break;

                        case "2": // Create Snapshot
                            Console.Write("Enter Snapshot Name: ");
                            var name = Console.ReadLine();

                            Console.Write("Enter Snapshot Description: ");
                            var description = Console.ReadLine();

                            var createSnapshotReport = await snapshotService.CreateSnapshotAsync(instanceId, name, description);
                            DisplayResult(createSnapshotReport);
                            break;

                        case "3": // Delete Snapshot
                            Console.Write("Enter Snapshot ID: ");
                            var snapshotId = Console.ReadLine();

                            if (!string.IsNullOrEmpty(snapshotId))
                            {
                                var deleteSnapshotReport = await snapshotService.DeleteSnapshotAsync(instanceId, snapshotId);
                                DisplayResult(deleteSnapshotReport);
                            }
                            else
                            {
                                Console.WriteLine("Invalid Snapshot ID.");
                            }
                            break;

                        case "4": // Return to Main Menu
                            return;

                        default:
                            Console.WriteLine("Invalid option. Please select a number between 1 and 4.");
                            break;
                    }

                    Console.WriteLine("\nPress Enter to return to the instance menu...");
                    Console.ReadLine();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred: {ex.Message}");
                }
            }
        }

        private static void DisplayResult(ResultObj result)
        {
            if (result.Success)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("\n" + result.Message);
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\n" + result.Message);
            }
            Console.ResetColor();
        }
    }
}
