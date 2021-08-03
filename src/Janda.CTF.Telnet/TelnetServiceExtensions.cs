using Microsoft.Extensions.DependencyInjection;

namespace Janda.CTF
{
    public static class TelnetServiceExtensions
    {
        public static IServiceCollection AddTelnetService(this IServiceCollection services)
        {
            return services.AddTransient<ITelnetService, TelnetService>();
        }      
    }
}
