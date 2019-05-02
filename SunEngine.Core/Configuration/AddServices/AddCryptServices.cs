using Microsoft.Extensions.DependencyInjection;
using SunEngine.Core.Services;

namespace SunEngine.Core.Configuration.AddServices
{
    public static class AddCryptServicesExtensions
    {
        public static void AddCryptServices(this IServiceCollection services)
        {
            CryptService cryptService = new CryptService();
            cryptService.AddCryptorKey(CaptchaService.CryptserviceName);
            services.AddSingleton(cryptService);
        }
    }
}