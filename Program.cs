using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NetworkMonitorBackup.Services;
using NetworkMonitorBackup.Models;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Serilog;
using Serilog.Events;

namespace NetworkMonitorBackup
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            // Initialize ScreenLogger
            var screenLogger = new ScreenLogger();

            // Configure Serilog
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .WriteTo.File("logs/network_monitor_backup.log", rollingInterval: RollingInterval.Day)
                .CreateLogger();

            // Configure services
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            var services = new ServiceCollection()
                .AddSingleton<IConfiguration>(configuration)
                .AddHttpClient()
                .AddSingleton(screenLogger)
                .AddLogging(loggingBuilder =>
                {
                    loggingBuilder.ClearProviders();
                    loggingBuilder.AddSerilog(); // Use Serilog for file logging
                })
                .AddSingleton(typeof(ILogger<>), typeof(CompositeLogger<>)) // Register CompositeLogger
                .AddTransient<IContaboService, ContaboService>()
                .AddTransient<SnapshotService>()
                .BuildServiceProvider();

            var logger = services.GetRequiredService<ILogger<Program>>();
            var snapshotService = services.GetRequiredService<SnapshotService>();

            while (true)
            {
                DisplayMainMenu();
                var choice = GetInput("Enter your choice");

                try
                {

                    switch (choice)
                    {
                        case "1":
                            DisplayResult(await snapshotService.ListInstancesAsync());
                            break;
                        case "2":
                            DisplayResult(await snapshotService.DisplayInstancesWithSnapshotsAsync());
                            break;
                        case "3":
                            Console.WriteLine("Refreshing snapshots for all instances...");
                            DisplayResult(await snapshotService.RefreshSnapshotsAsync());
                            break;
                        case "4":
                            await ManageInstanceSelection(snapshotService);
                            break;
                        case "5":
                            Console.WriteLine("Exiting Network Monitor Backup. Goodbye!");
                            return;
                        default:
                            Console.WriteLine("Invalid option. Please select a number between 1 and 5.");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "An unexpected error occurred.");
                    DisplayError($"An unexpected error occurred: {ex.Message}");
                }
                AwaitUserAction("Press Enter to return to the menu...");
            }
        }

        private static void DisplayMainMenu()
        {
            ClearScreen();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("=== Network Monitor Backup ===");
            Console.ResetColor();
            Console.WriteLine("1. List All Instances");
            Console.WriteLine("2. Display Instances with Snapshots");
            Console.WriteLine("3. Refresh Snapshots (All Instances)");
            Console.WriteLine("4. Select an Instance to Manage");
            Console.WriteLine("5. Exit");
        }

        private static async Task ManageInstanceSelection(SnapshotService snapshotService)
        {
            var instancesResult = await snapshotService.ListInstancesAsync();
            if (!instancesResult.Success || instancesResult.Data is not InstanceResponse instanceResponse || instanceResponse.Data.Count == 0)
            {
                DisplayResult(instancesResult);
                return;
            }
            ClearScreen();
            Console.WriteLine("\nAvailable Instances:");
            for (var i = 0; i < instanceResponse.Data.Count; i++)
            {
                var instance = instanceResponse.Data[i];
                Console.WriteLine($"{i + 1}. Instance ID: {instance.InstanceId}, Name: {instance.Name}, Status: {instance.Status}");
            }

            var selectedIndex = GetInputAsInt("Enter the number corresponding to the Instance ID to manage");
            if (selectedIndex <= 0 || selectedIndex > instanceResponse.Data.Count)
            {
                DisplayError("Invalid selection. Please choose a valid number.");
                return;
            }

            var selectedInstanceId = instanceResponse.Data[selectedIndex - 1].InstanceId;
            await ManageInstance(snapshotService, selectedInstanceId);
        }

        private static async Task ManageInstance(SnapshotService snapshotService, long instanceId)
        {
            while (true)
            {
                ClearScreen();
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"\n=== Managing Instance ID: {instanceId} ===");
                Console.ResetColor();

                Console.WriteLine("1. List Snapshots");
                Console.WriteLine("2. Create Snapshot");
                Console.WriteLine("3. Delete Snapshot");
                Console.WriteLine("4. Delete All Snapshots");
                Console.WriteLine("5. Return to Main Menu");

                var choice = GetInput("Enter your choice");

                try
                {

                    switch (choice)
                    {
                        case "1":
                            DisplayResult(await snapshotService.ListSnapshotsAsync(instanceId));
                            break;
                        case "2":
                            var name = GetInput("Enter Snapshot Name");
                            var description = GetInput("Enter Snapshot Description");
                            DisplayResult(await snapshotService.CreateSnapshotAsync(instanceId, name, description));
                            break;
                        case "3":
                            await DeleteSnapshot(snapshotService, instanceId);
                            break;
                        case "4":
                            if (ConfirmAction("WARNING: This will delete all snapshots for this instance. Type 'CONFIRM' to proceed"))
                            {
                                DisplayResult(await snapshotService.DeleteAllSnapshotsAsync(instanceId));
                            }
                            break;
                        case "5":
                            return;
                        default:
                            Console.WriteLine("Invalid option. Please select a number between 1 and 5.");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    DisplayError($"An error occurred: {ex.Message}");
                }
                AwaitUserAction("Press Enter to return to the instance menu...");
            }
        }

        private static async Task DeleteSnapshot(SnapshotService snapshotService, long instanceId)
        {
            var snapshotsResult = await snapshotService.ListSnapshotsAsync(instanceId);
            if (!snapshotsResult.Success || snapshotsResult.Data is not List<SnapshotResponse> snapshots || snapshots.Count == 0)
            {
                DisplayResult(snapshotsResult);
                return;
            }
            ClearScreen();
            Console.WriteLine("\nAvailable Snapshots:");
            for (var i = 0; i < snapshots.Count; i++)
            {
                var snapshot = snapshots[i];
                Console.WriteLine($"{i + 1}. Snapshot ID: {snapshot.SnapshotId}, Name: {snapshot.Name}");
            }

            var selectedIndex = GetInputAsInt("Enter the number of the snapshot to delete");
            if (selectedIndex <= 0 || selectedIndex > snapshots.Count)
            {
                DisplayError("Invalid selection. Please choose a valid snapshot number.");
                return;
            }

            var selectedSnapshotId = snapshots[selectedIndex - 1].SnapshotId;
            DisplayResult(await snapshotService.DeleteSnapshotAsync(instanceId, selectedSnapshotId));
        }

        private static void DisplayResult(ResultObj result)
        {
            ClearScreen();

            if (result.Success)
            {
                Console.ForegroundColor = ConsoleColor.Yellow; // Set pale yellow for success
                Console.WriteLine($"\n[Success] {result.Message}");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red; // Red for error
                Console.WriteLine($"\n[Error] {result.Message}");
            }

            Console.ResetColor(); // Reset to default color
        }


        private static void DisplayError(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\n[Error] {message}");
            Console.ResetColor();
        }

        private static string GetInput(string prompt)
        {
            Console.Write($"\n{prompt}: ");
            return Console.ReadLine() ?? string.Empty;
        }

        private static int GetInputAsInt(string prompt)
        {
            return int.TryParse(GetInput(prompt), out var result) ? result : -1;
        }

        private static bool ConfirmAction(string prompt)
        {
            return string.Equals(GetInput(prompt), "CONFIRM", StringComparison.OrdinalIgnoreCase);
        }

        private static void AwaitUserAction(string message)
        {
            Console.WriteLine($"\n{message}");
            Console.ReadLine();
        }

        private static void ClearScreen()
        {
            Console.Clear();
        }
    }
}
