﻿using System;
using System.Collections.Generic;
using System.Linq;
using HotChocolate.Runtime;
using HotChocolate.Types;

namespace HotChocolate.Configuration
{
    internal class SchemaContext
        : ISchemaContext
    {
        private readonly ServiceFactory _serviceFactory;
        private readonly TypeRegistry _typeRegistry;
        private readonly DirectiveRegistry _directiveRegistry;
        private readonly ResolverRegistry _resolverRegistry;

        public SchemaContext()
        {
            _serviceFactory = new ServiceFactory();
            _typeRegistry = new TypeRegistry(_serviceFactory);
            _resolverRegistry = new ResolverRegistry();
            _directiveRegistry = new DirectiveRegistry();
        }

        public ITypeRegistry Types => _typeRegistry;

        public IResolverRegistry Resolvers => _resolverRegistry;

        public IDirectiveRegistry Directives => _directiveRegistry;

        public IServiceProvider Services => _serviceFactory.Services;

        public IEnumerable<SchemaError> CompleteTypes()
        {
            // compile resolvers
            _resolverRegistry.BuildResolvers();

            // start type completion
            var errors = new List<SchemaError>();
            var processed = new HashSet<INamedType>();

            // directives
            CompleteDirectives(errors);

            // InputObjects
            CompleteTypes(
                _typeRegistry.GetTypes().OfType<InputObjectType>(),
                processed, errors);

            // interfaces
            CompleteTypes(
                _typeRegistry.GetTypes().OfType<InterfaceType>(),
                processed, errors);

            // all the other types
            CompleteTypes(
                _typeRegistry.GetTypes(),
                processed, errors);

            return errors;
        }

        private void CompleteTypes(
            IEnumerable<INamedType> types,
            ISet<INamedType> processed,
            ICollection<SchemaError> errors)
        {
            foreach (INamedType namedType in types)
            {
                if (processed.Add(namedType)
                    && namedType is INeedsInitialization init)
                {
                    var initializationContext = new TypeInitializationContext(
                        this, e => errors.Add(e), namedType, false);
                    init.CompleteType(initializationContext);
                }
            }
        }

        private void CompleteDirectives(
            ICollection<SchemaError> errors)
        {
            foreach (DirectiveType directive in
                _directiveRegistry.GetDirectiveTypes())
            {
                var initializationContext = new TypeInitializationContext(
                    this, e => errors.Add(e), directive, false);
                ((INeedsInitialization)directive)
                    .CompleteType(initializationContext);
            }
        }

        public void RegisterServiceProvider(IServiceProvider services)
        {
            _serviceFactory.Services = services;
        }
    }
}
