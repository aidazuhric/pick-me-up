using PickMeUp.Model.SearchObjects;
using PickMeUp.Services.Database;
using FluentValidation;
using FluentValidation.Results;
using Mapster;
using MapsterMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PickMeUp.Services
{
    public abstract class BaseCRUDService<TEntity, TResponse, TSearch, TInsertRequest, TUpdateRequest>
        : BaseReadService<TEntity, TResponse, TSearch>
        where TEntity : class
        where TSearch : BaseSearchObject
    {
        protected readonly IValidator<TInsertRequest> _insertValidator;
        protected readonly IValidator<TUpdateRequest> _updateValidator;

        protected BaseCRUDService(PickMeUpDbContext dbContext, MapsterMapper.IMapper mapper, IValidator<TInsertRequest> insertValidator, IValidator<TUpdateRequest> updateValidator) : base(mapper, dbContext)
        {
            _insertValidator = insertValidator;
            _updateValidator = updateValidator;
        }

        protected virtual TEntity MapInsertRequestToEntity(TInsertRequest request)
        {
            var entity = _mapper.Map<TEntity>(request ?? throw new ArgumentNullException(nameof(request)));
            return entity;
        }

        protected virtual void MapUpdateRequestToEntity(TUpdateRequest request, TEntity entity)
        {
            _mapper.Map(request, entity);
        }

        public virtual async Task<TResponse> InsertAsync(TInsertRequest request)
        {
            var validationResult = await _insertValidator.ValidateAsync(request);
            if (validationResult.IsValid == false)
            {
                var errors = validationResult.Errors.Select(e => _mapper.Map<ValidationFailure>(e));
                throw new FluentValidation.ValidationException(errors);
            }

            var entity = MapInsertRequestToEntity(request);

            var entityType = entity.GetType();
            var idProperty = entityType.GetProperty("Id");

            var createdAtProperty = entityType.GetProperty("CreatedAt");
            if (createdAtProperty?.CanWrite == true)
            {
                createdAtProperty.SetValue(entity, DateTime.UtcNow);
            }

            this._dbContext.Set<TEntity>().Add(entity);
            await this._dbContext.SaveChangesAsync();

            return await Task.FromResult(_mapper.Map<TResponse>(entity));
        }

        public virtual async Task<TResponse> UpdateAsync(int id, TUpdateRequest request)
        {
            var validationResult = await _updateValidator.ValidateAsync(request);
            if (validationResult.IsValid == false)
            {
                var errors = validationResult.Errors.Select(e => _mapper.Map<ValidationFailure>(e));
                throw new FluentValidation.ValidationException(errors);
            }

            var entity = this._dbContext.Set<TEntity>().Find(id);

            if (entity == null)
                throw new KeyNotFoundException($"{typeof(TEntity).Name} with id {id} not found.");

            MapUpdateRequestToEntity(request, entity);

            var updatedAtProperty = entity.GetType().GetProperty("UpdatedAt");
            if (updatedAtProperty?.CanWrite == true)
            {
                updatedAtProperty.SetValue(entity, DateTime.UtcNow);
            }

            await this._dbContext.SaveChangesAsync();

            return await Task.FromResult(_mapper.Map<TResponse>(entity));
        }

        public virtual async Task DeleteAsync(int id)
        {
            var entity = await FindByIdAsync(id);

            if (entity == null)
                throw new KeyNotFoundException($"{typeof(TEntity).Name} with id {id} not found.");

            if (entity is ISoftDeletable softDeletable)
            {
                softDeletable.IsDeleted = true;
                softDeletable.DeletedAt = DateTime.UtcNow;
            }
            else
            {
                this._dbContext.Set<TEntity>().Remove(entity);
            }

            await this._dbContext.SaveChangesAsync();
        }
    }
}
