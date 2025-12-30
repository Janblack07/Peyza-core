using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Peyza.Core.NotificationManagement.Providers;
using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Threading;
using Volo.Abp.Timing;
using Volo.Abp.Uow;

namespace Peyza.Core.NotificationManagement.Workers;

public class NotificationDispatchWorker : AsyncPeriodicBackgroundWorkerBase
{
    private readonly IRepository<NotificationMessage, Guid> _messageRepo;
    private readonly INotificationProviderDispatcher _dispatcher;
    private readonly IClock _clock;
    private readonly IUnitOfWorkManager _uowManager;
    private readonly NotificationDispatcherOptions _options;
    private readonly ILogger<NotificationDispatchWorker> _logger;

    public NotificationDispatchWorker(
        AbpAsyncTimer timer,
        IServiceScopeFactory serviceScopeFactory,
        IRepository<NotificationMessage, Guid> messageRepo,
        INotificationProviderDispatcher dispatcher,
        IClock clock,
        IUnitOfWorkManager uowManager,
        IOptions<NotificationDispatcherOptions> options,
        ILogger<NotificationDispatchWorker> logger)
        : base(timer, serviceScopeFactory)
    {
        _messageRepo = messageRepo;
        _dispatcher = dispatcher;
        _clock = clock;
        _uowManager = uowManager;
        _options = options.Value;
        _logger = logger;

        Timer.Period = _options.PeriodSeconds * 1000;
    }

    protected override async Task DoWorkAsync(PeriodicBackgroundWorkerContext workerContext)
    {
        var now = _clock.Now;

        using var uow = _uowManager.Begin(requiresNew: true, isTransactional: true);

        var queryable = await _messageRepo.GetQueryableAsync();

        var batch = queryable
            .Where(x => x.Status == NotificationStatus.Scheduled && x.ScheduledAt <= now)
            .OrderBy(x => x.ScheduledAt)
            .Take(_options.BatchSize)
            .ToList();

        if (batch.Count == 0)
        {
            await uow.CompleteAsync();
            return;
        }

        foreach (var msg in batch)
        {
            try
            {
                msg.MarkAsSending();
                await _messageRepo.UpdateAsync(msg, autoSave: true);

                var result = await _dispatcher.SendAsync(msg, workerContext.CancellationToken);

                if (result.Success && !string.IsNullOrWhiteSpace(result.ProviderMessageId))
                    msg.MarkAsSent(result.ProviderMessageId);
                else
                    msg.MarkAsFailed(result.ErrorCode ?? "PROVIDER_FAILED");

                await _messageRepo.UpdateAsync(msg, autoSave: true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DispatchWorker error. MessageId={MessageId}", msg.Id);

                try
                {
                    msg.MarkAsFailed("WORKER_EXCEPTION");
                    await _messageRepo.UpdateAsync(msg, autoSave: true);
                }
                catch { }
            }
        }

        await uow.CompleteAsync();
    }
}
