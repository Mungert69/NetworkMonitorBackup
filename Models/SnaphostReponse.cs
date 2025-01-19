using Newtonsoft.Json;
using System;

namespace NetworkMonitorBackup.Models
{
    public class SnapshotResponse
    {
        [JsonProperty("tenantId")]
        public string TenantId { get; set; }

        [JsonProperty("customerId")]
        public string CustomerId { get; set; }

        [JsonProperty("snapshotId")]
        public string SnapshotId { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("instanceId")]
        public long InstanceId { get; set; }

        [JsonProperty("createdDate")]
        public DateTime CreatedDate { get; set; } // Field name corrected from `createdAt`

        [JsonProperty("autoDeleteDate")]
        public DateTime? AutoDeleteDate { get; set; }

        [JsonProperty("imageId")]
        public string ImageId { get; set; }

        [JsonProperty("imageName")]
        public string ImageName { get; set; }
    }
}
