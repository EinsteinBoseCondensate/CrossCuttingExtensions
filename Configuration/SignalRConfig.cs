namespace CrossCuttingExtensions.Configuration
{
    public class SignalRConfig
    {
        public SignalRSection SignalRSection { get; set; }
    }
    public class SignalRSection
    {
        public bool? EnableDetailedErrors { get; set; }
    }
}