using PickMeUp.Model.Enums;
using PickMeUp.Services.Database;
using Microsoft.Extensions.Logging;

namespace PickMeUp.Services.Recommendations
{
    public class InteractionRecorder : IInteractionRecorder
    {
        private readonly PickMeUpDbContext _dbContext;
        private readonly ILogger<InteractionRecorder> _logger;

        public InteractionRecorder(PickMeUpDbContext dbContext, ILogger<InteractionRecorder> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task RecordAsync(
            int userId,
            int taxiCompanyId,
            InteractionSignalType signalType,
            decimal value,
            int? orderId = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _dbContext.InteractionHistories.Add(new InteractionHistory
                {
                    UserId = userId,
                    TaxiCompanyId = taxiCompanyId,
                    SignalTypeId = (int)signalType,
                    Value = value,
                    OrderId = orderId,
                    CreatedAt = DateTime.UtcNow
                });

                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception,
                    "Could not record the {SignalType} signal for user {UserId} and company {CompanyId}. "
                    + "The action itself already succeeded.",
                    signalType, userId, taxiCompanyId);
            }
        }
    }
}
