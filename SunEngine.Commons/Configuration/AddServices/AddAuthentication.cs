using Microsoft.Extensions.DependencyInjection;
using SunEngine.Commons.Security.Authentication;

namespace SunEngine.Commons.Configuration.AddServices
{
    public static class AddAuthenticationExtensions
    {
        public static void AddAuthentication(this IServiceCollection services)
        {
            services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = MyJwt.Scheme;
                    options.DefaultChallengeScheme = MyJwt.Scheme;
                })
                .AddScheme<MyJwtOptions, MyJwtHandler>(MyJwt.Scheme, MyJwt.Scheme, options => { });
        }
    }
}