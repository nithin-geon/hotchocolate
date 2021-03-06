﻿using System.Linq;
using System.Text;
using HotChocolate.Utilities;

namespace HotChocolate.Resolvers.CodeGeneration
{
    internal sealed class AsyncResolverMethodGenerator
        : ResolverSourceCodeGenerator<ResolverDescriptor>
    {
        protected override bool IsAsync => true;

        protected override void GenerateResolverInvocation(
            ResolverDescriptor resolverDescriptor,
            StringBuilder source)
        {
            source.AppendLine($"var resolver = ctx.{nameof(IResolverContext.Resolver)}<{resolverDescriptor.ResolverType.GetTypeName()}>();");

            source.Append($"return await resolver.{resolverDescriptor.Field.Member.Name}(");
            if (resolverDescriptor.Arguments.Count > 0)
            {
                string arguments = string.Join(", ",
                    resolverDescriptor.Arguments
                        .Select(t => t.VariableName));
                source.Append(arguments);
            }
            source.Append(");");
        }

        protected override bool CanHandle(ResolverDescriptor descriptor)
        {
            return descriptor.IsAsync && descriptor.IsMethod;
        }
    }
}
