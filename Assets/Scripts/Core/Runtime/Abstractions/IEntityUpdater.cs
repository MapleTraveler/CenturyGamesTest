namespace Core.Runtime.Abstractions
{
    public interface IEntityUpdater<in TEntity>
    {
        void Update(TEntity entity, float deltaTime);
    }

}