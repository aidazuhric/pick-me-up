using PickMeUp.Model.Constants;
using PickMeUp.Model.Enums;
using PickMeUp.Model.Exceptions;
using PickMeUp.Model.Requests;
using PickMeUp.Model.Responses;
using PickMeUp.Model.SearchObjects;
using PickMeUp.Services.Database;
using PickMeUp.Services.Recommendations;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace PickMeUp.Services
{
    public class ReviewService
        : BaseCRUDService<Review, ReviewResponse, ReviewSearch, ReviewInsertRequest, ReviewUpdateRequest>,
          IReviewService
    {
        private readonly IAuthenticatedUserAccessor _userAccessor;
        private readonly IInteractionRecorder _interactions;

        private ReviewScope? _scope;

        public ReviewService(
            PickMeUpDbContext dbContext,
            MapsterMapper.IMapper mapper,
            IValidator<ReviewInsertRequest> insertValidator,
            IValidator<ReviewUpdateRequest> updateValidator,
            IAuthenticatedUserAccessor userAccessor,
            IInteractionRecorder interactions)
            : base(dbContext, mapper, insertValidator, updateValidator)
        {
            _userAccessor = userAccessor;
            _interactions = interactions;
        }

        public override async Task<ReviewResponse> InsertAsync(ReviewInsertRequest request)
        {
            await _insertValidator.ValidateAndThrowAsync(request);

            var userId = RequireUserId();

            var order = await _dbContext.Orders
                .Include(o => o.Driver)
                .FirstOrDefaultAsync(o => o.Id == request.OrderId)
                ?? throw new KeyNotFoundException($"Order with id {request.OrderId} not found.");

            if (order.PassengerId != userId)
            {
                throw new KeyNotFoundException($"Order with id {request.OrderId} not found.");
            }

            if (order.OrderStatusId != (int)OrderStatusEnum.Completed)
            {
                throw new BusinessException("You can review a ride once it has been completed.");
            }

            if (request.ReviewCompany && order.TaxiCompanyId == null)
            {
                throw new BusinessException("This ride has no company to review.");
            }

            if (request.ReviewDriver && order.DriverId == null)
            {
                throw new BusinessException("This ride has no driver to review.");
            }

            await EnsureNotAlreadyReviewedAsync(order.Id, request);

            var review = new Review
            {
                OrderId = order.Id,
                PassengerId = userId,
                TargetCompanyId = request.ReviewCompany ? order.TaxiCompanyId : null,
                TargetDriverId = request.ReviewDriver ? order.DriverId : null,
                Rating = request.Rating,
                Comment = request.Comment,
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.Reviews.Add(review);

            try
            {
                await _dbContext.SaveChangesAsync();
            }
            catch (DbUpdateException exception) when (exception.IsUniqueViolation())
            {
                throw new BusinessException("You have already reviewed this ride.");
            }

            if (review.TargetCompanyId.HasValue)
            {
                await _interactions.RecordAsync(
                    userId, review.TargetCompanyId.Value, InteractionSignalType.Reviewed, review.Rating, order.Id);
            }

            return await LoadResponseAsync(review.Id);
        }

        public override async Task<ReviewResponse> UpdateAsync(int id, ReviewUpdateRequest request)
        {
            await _updateValidator.ValidateAndThrowAsync(request);

            var review = await LoadOwnReviewAsync(id);

            if (request.Rating.HasValue)
            {
                review.Rating = request.Rating.Value;
            }

            if (request.Comment != null)
            {
                review.Comment = request.Comment;
            }

            review.UpdatedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();

            return await LoadResponseAsync(review.Id);
        }

        public override async Task DeleteAsync(int id)
        {
            await LoadOwnReviewAsync(id);
            await base.DeleteAsync(id);
        }

        public async Task<List<RatingSummaryResponse>> GetCompanyAveragesAsync()
        {
            var summaries = await _dbContext.Reviews
                .Where(r => r.TargetCompanyId != null)
                .GroupBy(r => r.TargetCompanyId!.Value)
                .Select(g => new
                {
                    CompanyId = g.Key,
                    Average = g.Average(r => (double)r.Rating),
                    Count = g.Count()
                })
                .Join(
                    _dbContext.TaxiCompanies,
                    summary => summary.CompanyId,
                    company => company.Id,
                    (summary, company) => new RatingSummaryResponse
                    {
                        Id = company.Id,
                        Name = company.Name,
                        AverageRating = summary.Average,
                        ReviewCount = summary.Count
                    })
                .ToListAsync();

            return Rounded(summaries);
        }

        public async Task<List<RatingSummaryResponse>> GetDriverAveragesAsync()
        {
            var summaries = await _dbContext.Reviews
                .Where(r => r.TargetDriverId != null)
                .GroupBy(r => r.TargetDriverId!.Value)
                .Select(g => new
                {
                    DriverId = g.Key,
                    Average = g.Average(r => (double)r.Rating),
                    Count = g.Count()
                })
                .Join(
                    _dbContext.Drivers,
                    summary => summary.DriverId,
                    driver => driver.Id,
                    (summary, driver) => new RatingSummaryResponse
                    {
                        Id = driver.Id,
                        Name = driver.User.FirstName + " " + driver.User.LastName,
                        CompanyName = driver.TaxiCompany.Name,
                        AverageRating = summary.Average,
                        ReviewCount = summary.Count
                    })
                .ToListAsync();

            return Rounded(summaries);
        }

        private static List<RatingSummaryResponse> Rounded(List<RatingSummaryResponse> summaries)
        {
            foreach (var summary in summaries)
            {
                summary.AverageRating = Math.Round(summary.AverageRating, 2);
            }

            return summaries.OrderByDescending(s => s.AverageRating).ToList();
        }

        protected override Task<IQueryable<Review>> IncludeRelatedEntitiesAsync(ReviewSearch? search, IQueryable<Review> query)
        {
            IQueryable<Review> result = query
                .Include(r => r.Passenger)
                .Include(r => r.TargetCompany)
                .Include(r => r.TargetDriver).ThenInclude(d => d!.User);

            return Task.FromResult(result);
        }

        protected override IQueryable<Review> ApplyFilters(IQueryable<Review> query, ReviewSearch? search)
        {
            query = ApplyReadScope(query);

            if (search == null)
            {
                return query;
            }

            if (search.OrderId.HasValue)
            {
                query = query.Where(r => r.OrderId == search.OrderId.Value);
            }

            if (search.TargetCompanyId.HasValue)
            {
                query = query.Where(r => r.TargetCompanyId == search.TargetCompanyId.Value);
            }

            if (search.TargetDriverId.HasValue)
            {
                query = query.Where(r => r.TargetDriverId == search.TargetDriverId.Value);
            }

            if (search.Rating.HasValue)
            {
                query = query.Where(r => r.Rating == search.Rating.Value);
            }

            if (search.MinRating.HasValue)
            {
                query = query.Where(r => r.Rating >= search.MinRating.Value);
            }

            if (!string.IsNullOrWhiteSpace(search.AuthorName))
            {
                var pattern = $"%{EscapeLikePattern(search.AuthorName)}%";
                query = query.Where(r => EF.Functions.ILike(r.Passenger.FirstName, pattern)
                                      || EF.Functions.ILike(r.Passenger.LastName, pattern));
            }

            var createdFrom = search.CreatedFrom ?? PeriodStart(search.Period);
            if (createdFrom.HasValue)
            {
                query = query.Where(r => r.CreatedAt >= createdFrom.Value);
            }

            if (search.CreatedTo.HasValue)
            {
                query = query.Where(r => r.CreatedAt <= search.CreatedTo.Value);
            }

            return query;
        }

        public override async Task<PageResult<ReviewResponse>> GetAllAsync(ReviewSearch? search = null)
        {
            await ResolveScopeAsync();
            return await base.GetAllAsync(search);
        }

        public override async Task<ReviewResponse> GetByIdAsync(int id)
        {
            await ResolveScopeAsync();

            var review = await FindByIdAsync(id)
                ?? throw new KeyNotFoundException($"Review with id {id} not found.");

            EnsureVisible(review, id);

            return _mapper.Map<ReviewResponse>(review);
        }

        private async Task EnsureNotAlreadyReviewedAsync(int orderId, ReviewInsertRequest request)
        {
            if (request.ReviewCompany
                && await _dbContext.Reviews.AnyAsync(r => r.OrderId == orderId && r.TargetCompanyId != null))
            {
                throw new BusinessException("You have already reviewed the company for this ride.");
            }

            if (request.ReviewDriver
                && await _dbContext.Reviews.AnyAsync(r => r.OrderId == orderId && r.TargetDriverId != null))
            {
                throw new BusinessException("You have already reviewed the driver for this ride.");
            }
        }

        private IQueryable<Review> ApplyReadScope(IQueryable<Review> query)
        {
            var scope = _scope!;

            if (scope.IsAdmin)
            {
                return query;
            }

            if (scope.DriverId.HasValue)
            {
                return query.Where(r => r.TargetDriverId == scope.DriverId.Value);
            }

            if (scope.CompanyId.HasValue)
            {
                return query.Where(r => r.TargetCompanyId == scope.CompanyId.Value
                    || (r.TargetDriver != null && r.TargetDriver.TaxiCompanyId == scope.CompanyId.Value));
            }

            if (scope.PassengerUserId.HasValue)
            {
                return query.Where(r => r.PassengerId == scope.PassengerUserId.Value);
            }

            return query.Where(r => false);
        }

        private void EnsureVisible(Review review, int id)
        {
            var scope = _scope!;

            var visible = scope.IsAdmin
                || (scope.DriverId.HasValue && review.TargetDriverId == scope.DriverId.Value)
                || (scope.CompanyId.HasValue && (review.TargetCompanyId == scope.CompanyId.Value
                    || review.TargetDriver?.TaxiCompanyId == scope.CompanyId.Value))
                || (scope.PassengerUserId.HasValue && review.PassengerId == scope.PassengerUserId.Value);

            if (!visible)
            {
                throw new KeyNotFoundException($"Review with id {id} not found.");
            }
        }

        private async Task<Review> LoadOwnReviewAsync(int id)
        {
            var userId = RequireUserId();

            var review = await _dbContext.Reviews.FirstOrDefaultAsync(r => r.Id == id)
                ?? throw new KeyNotFoundException($"Review with id {id} not found.");

            if (!_userAccessor.IsInRole(Roles.Admin) && review.PassengerId != userId)
            {
                throw new KeyNotFoundException($"Review with id {id} not found.");
            }

            return review;
        }

        private async Task ResolveScopeAsync()
        {
            if (_scope != null)
            {
                return;
            }

            var userId = _userAccessor.GetUserId();
            var scope = new ReviewScope { IsAdmin = _userAccessor.IsInRole(Roles.Admin) };

            if (userId != null && !scope.IsAdmin)
            {
                scope.DriverId = await _dbContext.Drivers
                    .Where(d => d.UserId == userId.Value && d.ResignedAt == null)
                    .Select(d => (int?)d.Id)
                    .FirstOrDefaultAsync();

                if (scope.DriverId == null)
                {
                    scope.CompanyId = await _dbContext.TaxiCompanies
                        .Where(t => t.UserId == userId.Value)
                        .Select(t => (int?)t.Id)
                        .FirstOrDefaultAsync();

                    if (scope.CompanyId == null)
                    {
                        scope.PassengerUserId = userId;
                    }
                }
            }

            _scope = scope;
        }

        private static DateTime? PeriodStart(ShiftPeriod? period) => period switch
        {
            ShiftPeriod.Day => DateTime.UtcNow.AddDays(-1),
            ShiftPeriod.Week => DateTime.UtcNow.AddDays(-7),
            ShiftPeriod.Month => DateTime.UtcNow.AddMonths(-1),
            _ => null
        };

        private int RequireUserId() =>
            _userAccessor.GetUserId() ?? throw new BusinessException("The caller could not be identified.");

        private async Task<ReviewResponse> LoadResponseAsync(int id)
        {
            var review = await _dbContext.Reviews
                .AsNoTracking()
                .Include(r => r.Passenger)
                .Include(r => r.TargetCompany)
                .Include(r => r.TargetDriver).ThenInclude(d => d!.User)
                .FirstAsync(r => r.Id == id);

            return _mapper.Map<ReviewResponse>(review);
        }

        private sealed class ReviewScope
        {
            public bool IsAdmin { get; set; }
            public int? PassengerUserId { get; set; }
            public int? DriverId { get; set; }
            public int? CompanyId { get; set; }
        }
    }
}
