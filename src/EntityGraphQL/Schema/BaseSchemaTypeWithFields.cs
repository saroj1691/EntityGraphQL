using System;
using System.Collections.Generic;
using System.Linq;
using EntityGraphQL.Compiler;

namespace EntityGraphQL.Schema
{
    public abstract class BaseSchemaTypeWithFields<TFieldType> : ISchemaType where TFieldType : IField
    {
        internal ISchemaProvider Schema { get; }
        internal Dictionary<string, TFieldType> FieldsByName { get; } = new();
        public abstract Type TypeDotnet { get; }
        public string Name { get; }
        public string? Description { get; }
        public abstract bool IsInput { get; }
        public abstract bool IsEnum { get; }
        public abstract bool IsScalar { get; }
        public RequiredAuthorization? RequiredAuthorization { get; set; }

        protected BaseSchemaTypeWithFields(ISchemaProvider schema, string name, string? description, RequiredAuthorization? requiredAuthorization)
        {
            this.Schema = schema;
            Name = name;
            Description = description;
            RequiredAuthorization = requiredAuthorization;
        }

        /// <summary>
        /// Search for a field by name. Use HasField() to check if field exists.
        /// </summary>
        /// <param name="identifier">Field name. Case sensitive</param>
        /// <param name="requestContext">Current request context. Used by EntityGraphQL when compiling queries. If are calling this during schema configure, you can pass null</param>
        /// <returns>The field object for further configuration</returns>
        /// <exception cref="EntityGraphQLAccessException"></exception>
        /// <exception cref="EntityGraphQLCompilerException">If field if not found</exception>
        public IField GetField(string identifier, QueryRequestContext? requestContext)
        {
            if (FieldsByName.ContainsKey(identifier))
            {
                var field = FieldsByName[identifier];
                if (requestContext != null && requestContext.AuthorizationService != null && !requestContext.AuthorizationService.IsAuthorized(requestContext.User, field.RequiredAuthorization))
                    throw new EntityGraphQLAccessException($"You are not authorized to access the '{identifier}' field on type '{Name}'.");
                if (requestContext != null && requestContext.AuthorizationService != null && !requestContext.AuthorizationService.IsAuthorized(requestContext.User, field.ReturnType.SchemaType.RequiredAuthorization))
                    throw new EntityGraphQLAccessException($"You are not authorized to access the '{field.ReturnType.SchemaType.Name}' type returned by field '{identifier}'.");

                return FieldsByName[identifier];
            }

            throw new EntityGraphQLCompilerException($"Field '{identifier}' not found on type '{Name}'");
        }
        /// <summary>
        /// Return all the fields defined on this type
        /// </summary>
        /// <returns>List of Field objects</returns>
        public IEnumerable<IField> GetFields()
        {
            return FieldsByName.Values.Cast<IField>();
        }
        /// <summary>
        /// Checks if this type has a field with the given name
        /// </summary>
        /// <param name="identifier">Field name. Case sensitive</param>
        /// <returns></returns>
        public bool HasField(string identifier, QueryRequestContext? requestContext)
        {
            if (FieldsByName.ContainsKey(identifier))
            {
                var field = FieldsByName[identifier];
                if (requestContext != null && requestContext.AuthorizationService != null && !requestContext.AuthorizationService.IsAuthorized(requestContext.User, field.RequiredAuthorization))
                    return false;

                return true;
            }

            return false;
        }
        public abstract ISchemaType AddAllFields(bool autoCreateNewComplexTypes = false, bool autoCreateEnumTypes = true);

        public void AddFields(IEnumerable<IField> fields)
        {
            foreach (var f in fields)
            {
                AddField(f);
            }
        }

        public IField AddField(IField field)
        {
            if (FieldsByName.ContainsKey(field.Name))
                throw new EntityQuerySchemaException($"Field {field.Name} already exists on type {this.Name}. Use ReplaceField() if this is intended.");

            FieldsByName.Add(field.Name, (TFieldType)field);
            return field;
        }

        /// <summary>
        /// Remove a field by the given name. Case sensitive. If the field does not exist, nothing happens.
        /// </summary>
        /// <param name="name"></param>
        public void RemoveField(string name)
        {
            if (FieldsByName.ContainsKey(name))
            {
                FieldsByName.Remove(name);
            }
        }
    }
}
