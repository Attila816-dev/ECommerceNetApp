using System.Diagnostics;

namespace ECommerceNetApp.Service.Implementation
{
    public static class ServiceTelemetry
    {
        public static ActivitySource ActivitySource { get; } = new ActivitySource("ECommerceNetApp.Service");
    }
}
