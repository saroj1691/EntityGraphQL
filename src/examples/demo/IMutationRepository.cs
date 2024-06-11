using EntityGraphQL.Schema.Mutations;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Demo.Mutations
{
    public interface IMutationRepository<TEntity> where TEntity : class, IMutation
    {
        Task<TEntity> Add(TEntity entity);

        Task<TEntity> Update(TEntity entity);

        Task<TEntity> Update<TKey>(TKey id, TEntity entity) where TKey : IEquatable<TKey>;

        Task<TEntity> Delete(TEntity entity);

        Task<TEntity> Delete<TKey>(TKey id) where TKey : IEquatable<TKey>;
    }
}
