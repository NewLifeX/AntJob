using XCode;

namespace AntJob.Data
{
    /// <summary>实体基类</summary>
    /// <typeparam name="TEntity"></typeparam>
    public class EntityBase<TEntity> : Entity<TEntity> where TEntity : EntityBase<TEntity>, new()
    {
    }
}