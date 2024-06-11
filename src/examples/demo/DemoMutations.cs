using EntityGraphQL.Contracts;
using EntityGraphQL.Schema;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Demo.Mutations
{
    public abstract class TestMutations
    {
        [GraphQLMutation("Add a new entity to the system")]
        public static async Task<Expression<Func<TContext, TEntity>>> AddNew<TEntity, TContext>(
            TContext db, 
            IMutationRepository<TEntity> repo, 
            [GraphQLInputType] TEntity entity) where TEntity : class, IMutation where TContext : class
        {
            var result = await repo.Add(entity);
            return (ctx) => result;
        }

        [GraphQLMutation("Update an entity in the system")]
        public static async Task<Expression<Func<TContext, TEntity>>> Update<TEntity, TContext>(
            TContext db,
            IMutationRepository<TEntity> repo,
            int Id,
            [GraphQLInputType] TEntity entity) where TEntity : class, IMutation where TContext : class
        {
            var result = await repo.Update(entity);
            return (ctx) => result;
        }
    }
}
