﻿namespace Domain.Abstractions
{
    public interface IRepository<TEntity, TKey> where TEntity : class
    {
        IQueryable<TEntity> GetAll();
        void Add(TEntity entity);
        void Update(TEntity entity);
        void Delete(TEntity entity);
        Task<TEntity?> FindById(TKey id);
    }
}