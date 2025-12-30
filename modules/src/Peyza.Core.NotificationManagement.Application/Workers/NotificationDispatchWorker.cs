using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Peyza.Core.NotificationManagement.Providers;
using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Linq;
using Volo.Abp.Threading;
using Volo.Abp.Timing;
using Volo.Abp.Uow;

namespace Peyza.Core.NotificationManagement;

public class NotificationDispatchWorker : AsyncPeriodicBackgroundWorkerBase
{
    private const int BatchSize = 20;

    private readonly IUnitOfWorkManager _uowManager;
    private readonly IClock _clock;
    private readonly IAsyncQueryableExecuter _asyncExecuter;

    public NotificationDispatchWorker(
        AbpAsyncTimer timer,
        IServiceScopeFactory serviceScopeFactory,
        IUnitOfWorkManager uowManager,
        IClock clock,
        IAsyncQueryableExecuter asyncExecuter)
        : base(timer, serviceScopeFactory)
    {
        _uowManager = uowManager;
        _clock = clock;
        _asyncExecuter = asyncExecuter;

        Timer.Period = 10_000; // cada 10s
    }

    protected override async Task DoWorkAsync(PeriodicBackgroundWorkerContext workerContext)
    {
        var now = _clock.Now;

        using var scope = ServiceScopeFactory.CreateScope();

        var messageRepo = scope.ServiceProvider.GetRequiredService<IRepository<NotificationMessage, Guid>>();
        var emailProvider = scope.ServiceProvider.GetRequiredService<IEmailProvider>();
        var smsProvider = scope.ServiceProvider.GetRequiredService<ISmsProvider>();

        // 1) CLAIM (Scheduled y due) -> Sending (UoW corta)
        Guid[] claimedIds;

        using (var uow = _uowManager.Begin(requiresNew: true, isTransactional: true))
        {
            var q = await messageRepo.GetQueryableAsync();

            var dueQuery = q
                .Where(x =>
                    x.Status == NotificationStatus.Scheduled &&
                    x.ScheduledAt != null &&
                    x.ScheduledAt <= now)
                .OrderBy(x => x.ScheduledAt)
                .Take(BatchSize);

            var dueList = await _asyncExecuter.ToListAsync(dueQuery);

            foreach (var msg in dueList)
            {
                // Invariante: solo Pending/Scheduled -> Sending
                msg.MarkAsSending();
                await messageRepo.UpdateAsync(msg, autoSave: true);
            }

            claimedIds = dueList.Select(x => x.Id).ToArray();

            await uow.CompleteAsync();
        }

        if (claimedIds.Length == 0)
            return;

        // 2) SEND (cada mensaje con su UoW)
        foreach (var id in claimedIds)
        {
            using var uow = _uowManager.Begin(requiresNew: true, isTransactional: true);

            var msg = await messageRepo.GetAsync(id);

            // Idempotencia: si cambió estado, skip
            if (msg.Status != NotificationStatus.Sending)
            {
                await uow.CompleteAsync();
                continue;
            }

            try
            {
                var providerId = await SendViaProviderAsync(msg, emailProvider, smsProvider);

                msg.MarkAsSent(providerId, _clock.Now);
                await messageRepo.UpdateAsync(msg, autoSave: true);

                Logger.LogInformation("Notification sent. Id={Id} ProviderId={ProviderId}", msg.Id, providerId);
            }
            catch (Exception ex)
            {
                msg.MarkAsFailed("PROVIDER_SEND_FAILED");
                await messageRepo.UpdateAsync(msg, autoSave: true);

                Logger.LogWarning(ex, "Notification failed. Id={Id}", msg.Id);
            }

            await uow.CompleteAsync();
        }
    }

    private static Task<string> SendViaProviderAsync(
        NotificationMessage msg,
        IEmailProvider emailProvider,
        ISmsProvider smsProvider)
    {
        return msg.Channel switch
        {
            NotificationChannel.Email => emailProvider.SendAsync(msg),
            NotificationChannel.SMS => smsProvider.SendAsync(msg),
            _ => throw new BusinessException("CHANNEL_NOT_SUPPORTED")
        };
    }
}
