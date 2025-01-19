using Newtonsoft.Json;

namespace NetworkMonitorBackup.Models
{
    public class SnapshotResponse
    {
        [JsonProperty("snapshotId")]
        public string SnapshotId { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("createdAt")]
        public DateTime CreatedAt { get; set; }
    }
}
