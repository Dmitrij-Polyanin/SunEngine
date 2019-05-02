using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LinqToDB;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using SunEngine.Core.Cache.Services;
using SunEngine.Core.Configuration.Options;
using SunEngine.Core.DataBase;
using SunEngine.Core.Security;

namespace SunEngine.Core.Services
{
    public class CleanCacheJobsService : IHostedService
    {
        private readonly SpamProtectionCache spamProtectionCache;
        private readonly JwtBlackListService jwtBlackListService;
        private readonly IDataBaseFactory dbFactory;
        private readonly SchedulerOptions schedulerOptions;
        

        private Timer timerSpamProtectionCache;
        private Timer timerJwtBlackListService;
        private Timer timerLongSessionsClearer;


        public CleanCacheJobsService(
            IDataBaseFactory dbFactory,
            SpamProtectionCache spamProtectionCache,
            IOptions<SchedulerOptions> schedulerOptions,
            JwtBlackListService jwtBlackListService)
        {
            this.dbFactory = dbFactory;
            this.spamProtectionCache = spamProtectionCache;
            this.jwtBlackListService = jwtBlackListService;
            this.schedulerOptions = schedulerOptions.Value;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            timerSpamProtectionCache = new Timer(_ =>
            {
                Console.WriteLine("SpamProtectionCache.RemoveExpired");
                spamProtectionCache.RemoveExpired();
            }, null, TimeSpan.Zero, TimeSpan.FromMinutes(schedulerOptions.SpamProtectionCacheClearMinutes));

            timerJwtBlackListService = new Timer(_ =>
            {
                Console.WriteLine("JwtBlackListService.RemoveExpired");
                jwtBlackListService.RemoveExpired();
            }, null, TimeSpan.Zero, TimeSpan.FromMinutes(schedulerOptions.JwtBlackListServiceClearMinutes));

            timerLongSessionsClearer = new Timer(_ =>
            {
                Console.WriteLine("LongSessionsClearer.ClearExpiredLongSessions");
                using (var db = dbFactory.CreateDb())
                {
                    LongSessionsClearer.ClearExpiredLongSessions(db);
                }
            }, null, TimeSpan.Zero, TimeSpan.FromDays(schedulerOptions.LongSessionsClearDays));

            return Task.CompletedTask;
        }


        public Task StopAsync(CancellationToken cancellationToken)
        {
            timerSpamProtectionCache.Dispose();
            timerJwtBlackListService.Dispose();
            timerLongSessionsClearer.Dispose();

            return Task.CompletedTask;
        }
    }
    
    public static class LongSessionsClearer
    {
        public static void ClearExpiredLongSessions(DataBaseConnection db)
        {
            var now = DateTime.UtcNow;
            db.LongSessions.Where(x => x.ExpirationDate <= now).Delete();
        }
    }
}