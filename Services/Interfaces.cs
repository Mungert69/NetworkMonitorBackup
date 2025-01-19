using System.Threading.Tasks;
using NetworkMonitorBackup.Models;


namespace NetworkMonitorBackup.Services
{
    public interface IContaboService
    {
        /// <summary>
        /// Authenticates with the Contabo API.
        /// </summary>
        /// <returns>A ResultObj indicating success or failure of the authentication.</returns>
        Task<ResultObj> AuthenticateAsync();

        /// <summary>
        /// Retrieves the list of snapshots for a specific instance.
        /// </summary>
        /// <param name="instanceId">The ID of the instance.</param>
        /// <returns>A ResultObj containing the operation status, message, and data (list of SnapshotResponse objects).</returns>
        Task<ResultObj> ListSnapshotsAsync(long instanceId);

        /// <summary>
        /// Retrieves the list of instances.
        /// </summary>
        /// <returns>A ResultObj containing the operation status, message, and data (InstanceResponse object).</returns>
        Task<ResultObj> ListInstancesAsync();

        /// <summary>
        /// Creates a snapshot for a specific instance.
        /// </summary>
        /// <param name="instanceId">The ID of the instance.</param>
        /// <param name="request">The snapshot creation request containing the name and description.</param>
        /// <returns>A ResultObj containing the operation status, message, and data (SnapshotResponse object).</returns>
        Task<ResultObj> CreateSnapshotAsync(long instanceId, SnapshotRequest request);

        Task<ResultObj> DeleteSnapshotAsync(long instanceId, string snapshotId);
    }
}
