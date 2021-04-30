using System.Application.Entities;

namespace System.Application.Repositories
{
    public interface IGetPrimaryKey<TEntity> where TEntity : class
    {
        object GetPrimaryKey(TEntity entity);
    }

    public interface IGetPrimaryKey<TEntity, TPrimaryKey> : IGetPrimaryKey<TEntity>
      where TEntity : class, IEntity<TPrimaryKey>
      where TPrimaryKey : IEquatable<TPrimaryKey>
    {
        public new TPrimaryKey GetPrimaryKey(TEntity entity) => DefaultGetPrimaryKey(entity);

        protected static TPrimaryKey DefaultGetPrimaryKey(TEntity entity) => entity.Id;
    }
}