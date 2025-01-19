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
                Console.WriteLine("3. Refresh Snapshots (All Instances)");
                Console.WriteLine("4. Select an Instance to Manage");
                Console.WriteLine("5. Exit");
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

                        case "3": // Refresh Snapshots
                            Console.WriteLine("Refreshing snapshots for all instances...");
                            var refreshAllReport = await snapshotService.RefreshSnapshotsAsync();
                            DisplayResult(refreshAllReport);
                            break;

                        case "4": // Select an Instance to Manage
                            var instancesReport = await snapshotService.ListInstancesAsync();

                            if (instancesReport.Success && instancesReport.Data is InstanceResponse instanceResponse)
                            {
                                DisplayInstancesWithIndexes(instanceResponse);

                                Console.WriteLine("\nEnter the number corresponding to the Instance ID to manage:");
                                if (int.TryParse(Console.ReadLine(), out var index) && index > 0 && index <= instanceResponse.Data.Count)
                                {
                                    var selectedInstanceId = instanceResponse.Data[index - 1].InstanceId;
                                    await ManageInstance(snapshotService, selectedInstanceId);
                                }
                                else
                                {
                                    Console.WriteLine("Invalid selection.");
                                }
                            }
                            else
                            {
                                DisplayResult(instancesReport);
                            }
                            break;

                        case "5": // Exit
                            Console.WriteLine("Exiting Network Monitor Backup. Goodbye!");
                            return;

                        default:
                            Console.WriteLine("Invalid option. Please select a number between 1 and 5.");
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

        private static void DisplayInstancesWithIndexes(InstanceResponse instanceResponse)
        {
            Console.WriteLine("\nInstances:");
            for (var i = 0; i < instanceResponse.Data.Count; i++)
            {
                var instance = instanceResponse.Data[i];
                Console.WriteLine($"{i + 1}. Instance ID: {instance.InstanceId}, Name: {instance.Name}, Status: {instance.Status}");
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
                Console.WriteLine("4. !! Delete All Snapshots !!");
                Console.WriteLine("5. Return to Main Menu");
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

                        case "4": // Delete All Snapshots
                            Console.WriteLine("!! WARNING: This will delete all snapshots for this instance !!");
                            Console.Write("Type 'CONFIRM' to proceed: ");
                            var confirmation = Console.ReadLine();

                            if (confirmation?.ToUpper() == "CONFIRM")
                            {
                                Console.WriteLine("Deleting all snapshots for this instance...");
                                var deleteAllReport = await snapshotService.DeleteAllSnapshotsAsync(instanceId);
                                DisplayResult(deleteAllReport);
                            }
                            else
                            {
                                Console.WriteLine("Operation cancelled. No snapshots were deleted.");
                            }
                            break;

                        case "5": // Return to Main Menu
                            return;

                        default:
                            Console.WriteLine("Invalid option. Please select a number between 1 and 5.");
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
