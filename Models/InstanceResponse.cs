using System;
using System.Collections.Generic;

namespace NetworkMonitorBackup.Models
{
    public class InstanceResponse
    {
        public List<InstanceData> Data { get; set; }
        public Links Links { get; set; }
        public Pagination Pagination { get; set; }
    }

    public class InstanceData
    {
        public string TenantId { get; set; }
        public string CustomerId { get; set; }
        public List<string> AdditionalIps { get; set; }
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public long InstanceId { get; set; }
        public string DataCenter { get; set; }
        public string Region { get; set; }
        public string RegionName { get; set; }
        public string ProductId { get; set; }
        public string ImageId { get; set; }
        public IpConfig IpConfig { get; set; }
        public string MacAddress { get; set; }
        public int RamMb { get; set; }
        public int CpuCores { get; set; }
        public string OsType { get; set; }
        public int DiskMb { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CancelDate { get; set; }
        public string Status { get; set; }
        public int VHostId { get; set; }
        public int VHostNumber { get; set; }
        public string VHostName { get; set; }
        public List<AddOn> AddOns { get; set; }
        public string ProductType { get; set; }
        public string ProductName { get; set; }
        public string DefaultUser { get; set; }
    }

    public class IpConfig
    {
        public IpDetails V4 { get; set; }
        public IpDetails V6 { get; set; }
    }

    public class IpDetails
    {
        public string Ip { get; set; }
        public string Gateway { get; set; }
        public int NetmaskCidr { get; set; }
    }

    public class AddOn
    {
        public int Id { get; set; }
        public int Quantity { get; set; }
    }

    public class Links
    {
        public string First { get; set; }
        public string Previous { get; set; }
        public string Next { get; set; }
        public string Last { get; set; }
        public string Self { get; set; }
    }

   
}
