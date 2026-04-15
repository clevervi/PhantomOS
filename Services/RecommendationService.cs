using System.Collections.Generic;
using System.Linq;
using PhantomOS.Models;

namespace PhantomOS.Services
{
    public class RecommendationService
    {
        public List<string> GetRecommendedTweakIds(HardwareInfo info, List<AtomicTweak> catalog)
        {
            var recommendedIds = new List<string>();

            foreach (var tweak in catalog)
            {
                bool isRecommended = false;

                // Logic 1: Privacy tweaks are always recommended for everyone
                if (tweak.Category == TweakCategory.Privacy && tweak.Risk == RiskLevel.Safe)
                {
                    isRecommended = true;
                }

                // Logic 2: Gaming tweaks are recommended if there's a dedicated GPU or high-end CPU
                if (tweak.Category == TweakCategory.Gaming)
                {
                    if (info.GpuName.ToLower().Contains("nvidia") || 
                        info.GpuName.ToLower().Contains("radeon") || 
                        info.CpuCores >= 6)
                    {
                        isRecommended = true;
                    }
                }

                // Logic 3: SSD-specific tweaks
                if (tweak.Id == "perf_sysmain")
                {
                    // Recommendation: Disable only if it's an SSD
                    if (info.IsSSD) isRecommended = true;
                }

                // Logic 4: Performance tweaks for high-ram systems
                if (tweak.Id == "perf_large_cache")
                {
                    if (info.TotalRamGb >= 16) isRecommended = true;
                }

                // Logic 5: Network tweaks for systems with good CPUs
                if (tweak.Category == TweakCategory.Network)
                {
                    if (info.CpuThreads >= 8) isRecommended = true;
                }

                // Exception: Laptop safety
                if (info.IsLaptop && tweak.Id == "game_power_throttling")
                {
                    // Avoid recommending battery-draining tweaks on laptops automatically
                    isRecommended = false; 
                }

                if (isRecommended)
                {
                    recommendedIds.Add(tweak.Id);
                }
            }

            return recommendedIds;
        }
    }
}
