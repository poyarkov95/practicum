using EventApplication.Service.Interface;

namespace EventApplication.Service.Hosted;

public class BookingWorker(ILogger<BookingWorker> logger, IServiceScopeFactory scopeFactory) : BackgroundService
{
    private readonly SemaphoreSlim _processingSemaphore = new(1, 1);
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("BookingWorker запущен");
        
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await using var scope = scopeFactory.CreateAsyncScope();
                var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();
                
                var pendingBookings = await bookingService.GetPendingBookingsAsync();

                if (pendingBookings.Count == 0)
                {
                    logger.LogInformation("Все бронирования обработаны");
                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                    continue;
                }
                
                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
                    await _processingSemaphore.WaitAsync(stoppingToken);
            
                    var tasks = pendingBookings.Select(booking => bookingService.ProcessBookingAsync(booking, stoppingToken));
                    await Task.WhenAll(tasks); 
                }
                finally
                {
                    _processingSemaphore.Release();
                }
            }
            catch (System.Exception ex)
            {
                logger.LogError(ex, "Ошибка при обработке бронирования");
            }
        }

        logger.LogInformation("BookingWorker остановлен");
    }
}