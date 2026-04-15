namespace PhantomOS.Models
{
    public enum TweakCategory
    {
        Privacy,
        Gaming,
        Performance,
        Network,
        System
    }

    public enum RiskLevel
    {
        Safe,      // No noticeable side effects
        Moderate,  // Might disable minor functionality
        Advanced   // For power users only, potential side effects
    }
}
