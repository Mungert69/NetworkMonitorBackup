using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace NetworkMonitorBackup.Models
{
    public class SnapshotApiResponse
    {
        [JsonProperty("data")]
        public List<SnapshotResponse> Data { get; set; }

        [JsonProperty("_links")]
        public Links Links { get; set; }
    }

}
