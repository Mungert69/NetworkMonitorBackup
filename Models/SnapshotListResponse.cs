using Newtonsoft.Json;
namespace NetworkMonitorBackup.Models
{
    public class SnapshotListResponse
    {
        [JsonProperty("pagination")]
        public Pagination Pagination { get; set; }

        [JsonProperty("data")]
        public List<SnapshotResponse> Snapshots { get; set; }
    }

    public class Pagination
    {
        [JsonProperty("page")]
        public int Page { get; set; }

        [JsonProperty("size")]
        public int Size { get; set; }

        [JsonProperty("total")]
        public int Total { get; set; }
    }
}
