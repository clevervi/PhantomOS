namespace PhantomOS.Models
{
    public class HardwareInfo
    {
        // CPU
        public string CpuName { get; set; } = "Unknown CPU";
        public uint CpuCores { get; set; }
        public uint CpuThreads { get; set; }
        public string CpuArchitecture { get; set; } = string.Empty;

        // RAM
        public double TotalRamGb { get; set; }
        
        // GPU
        public string GpuName { get; set; } = "Unknown GPU";
        
        // Disk
        public string DiskModel { get; set; } = string.Empty;
        public bool IsSSD { get; set; }
        
        // System
        public bool IsLaptop { get; set; }
        public string OsVersion { get; set; } = string.Empty;
        
        // Summary for UI
        public string DetailedSummary => $"{CpuName} ({CpuCores}C/{CpuThreads}T) | {TotalRamGb:F1}GB RAM | {(IsSSD ? "SSD" : "HDD")} | {(IsLaptop ? "Portátil" : "Sobremesa")}";
    }
}
