using System.IO;
using System.Linq;
using demo.Mutations;
using EntityGraphQL.Schema;
using EntityGraphQL.Extensions;
using EntityGraphQL.Schema.FieldExtensions;
using System.Net.Mime;
using System.Linq.Expressions;
using System;
using Demo.Mutations;
using Microsoft.Extensions.Options;
using System.Data;
using System.Reflection;

namespace demo
{
    public class GraphQLSchema
    {
        public static void ConfigureSchema(SchemaProvider<DemoContext> demoSchema)
        {
            // Add custom root fields
            demoSchema.UpdateQuery(queryType =>
            {
                demoSchema.AddType<Connection<Person>>("PersonConnection", "Metadata about a person connection (paging over people)").AddAllFields();
                queryType.AddField("writers", db => db.People.Where(p => p.WriterOf.Any()), "List of writers");
                queryType.AddField("directors", db => db.People.Where(p => p.DirectorOf.Any()), "List of directors")
                    .UseSort((Person et) => et.LastName, SortDirection.ASC);

                queryType.ReplaceField("actors",
                    (db) => db.Actors.Select(a => a.Person),
                    "actors paged by connection & edges and orderable")
                    .UseFilter()
                    .UseSort()
                    .UseConnectionPaging();

                queryType.AddField("actorsOffset",
                    (db) => db.Actors.Select(a => a.Person)
                            .OrderBy(a => a.Id),
                    "Actors with offset paging")
                    .UseOffsetPaging();
            });

            // Add calculated fields to a type
            demoSchema.UpdateType<Person>(type =>
            {
                type.AddField("name", l => $"{l.FirstName} {l.LastName}", "Person's name");
                // really poor example of using services e.g. you could just do below but pretend the service does something crazy like calls an API
                // type.AddField("age", l => (int)((DateTime.Now - l.Dob).TotalDays / 365), "Show the person's age");
                // AgeService needs to be added to the ServiceProvider
                type.AddField("age", "Show the person's age")
                    .Resolve<AgeService>((person, ageService) => ageService.Calc(person.Dob));
                type.AddField("filteredDirectorOf", new
                {
                    filter = ArgumentHelper.EntityQuery<Movie>()
                },
                (person, args) => person.DirectorOf.WhereWhen(args.filter, args.filter.HasValue).OrderBy(a => a.Name),
                "Get Director of based on filter");
                type.ReplaceField("writerOf", m => m.WriterOf.Select(a => a.Movie), "Movies they wrote");
                type.ReplaceField("actorIn", m => m.ActorIn.Select(a => a.Movie), "Movies they acted in");
            });

            // replace fields. e.g. remove a many-to-many relationship
            demoSchema.UpdateType<Movie>(type =>
            {
                type.ReplaceField("actors", m => m.Actors.Select(a => a.Person), "Actors in the movie");
                type.ReplaceField("writers", m => m.Writers.Select(a => a.Person), "Writers in the movie");
                type.AddField("contributedBy", "User who added this movie")
                    .Resolve<UserService>((movie, users) => users.GetUser(movie.CreatedBy));
            });

            // let's pretend the Users come from another service
            demoSchema.AddType<User>("User", "A user of the system", userType =>
            {
                userType.AddAllFields();
                // create a connection back to data from our main context
                userType.AddField("moviesContributed", "Movies this user added")
                    .Resolve<DemoContext>((user, context) => context.Movies.Where(m => m.CreatedBy == user.Id));
            });
            demoSchema.Query().AddField("users", "List of users")
                .Resolve<UserService>((_, users) => users.GetUsers());


            // add some mutations 
            demoSchema.AddInputType<Detail>("Detail", "Detail item").AddAllFields();
            demoSchema.AddInputType<Actor>("ActorDetail", "Actor Detail Item").AddAllFields();
            demoSchema.AddInputType<Writer>("WriterDetail", "Writer Detail Item").AddAllFields();
            demoSchema.AddInputType<Movie>("MovieDetail", "Movie Detail Item").AddAllFields();
            demoSchema.AddInputType<Person>("PersonDetail", "Person Detail Item").AddAllFields();
            //demoSchema.Mutation().AddFrom<DemoMutations>();
            //demoSchema.AddMutationsFrom<PersonMutation>();
            //demoSchema.AddMutationsFrom<MovieMutation>();
            AddMutations(demoSchema);
            File.WriteAllText("schema.graphql", demoSchema.ToGraphQLSchemaString());
        }

        static void AddMutations(SchemaProvider<DemoContext> demoSchema)
        {
            var imutationType = typeof(IMutation);
            var defaultMutationType = typeof(TestMutations);
            var types = typeof(DemoContext).Assembly
                                .GetTypes()
                                .Where(x => x.IsClass && !x.IsAbstract)
                                .Where(x => imutationType.IsAssignableFrom(x));

            foreach (Type type in types)
            {
                foreach (var method in defaultMutationType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly))
                {
                    var methodInfo = method.MakeGenericMethod(type, typeof(DemoContext));
                    demoSchema.Mutation().Add($"{method.Name}{type.Name}", $"{method.Name}{type.Name}", methodInfo.ConvertToDelegate(null));

                }
            }
        }
    }
}
