using EntityGraphQL.Schema;
using System.Linq.Expressions;
using System;

namespace demo.Mutations
{
    public class BaseMutation<TEntity> where TEntity : class
    {
        [GraphQLMutation("Add a new person to the system")]
        public static Expression<Func<DemoContext, TEntity>> AddNew(DemoContext db, [GraphQLInputType]TEntity person)
        {
            //db.People.Add(person);
            //db.SaveChanges();

            return (ctx) => default(TEntity);
        }

        [GraphQLMutation("Update a person in the system")]
        public static Expression<Func<DemoContext, TEntity>> Update(DemoContext db, [GraphQLInputType]TEntity person)
        {
            //db.People.Add(person);
            //db.SaveChanges();

            return (ctx) => default(TEntity);
        }
    }
}
