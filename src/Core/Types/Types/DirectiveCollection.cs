﻿using System;
using System.Collections;
using System.Collections.Generic;

namespace HotChocolate.Types
{
    internal sealed class DirectiveCollection
        : TypeSystemBase
        , IDirectiveCollection
    {
        private readonly List<IDirective> _directives = new List<IDirective>();
        private readonly TypeSystemBase _source;
        private readonly DirectiveLocation _location;
        private readonly IReadOnlyCollection<DirectiveDescription> _descs;

        internal DirectiveCollection(
            TypeSystemBase source,
            DirectiveLocation location,
            IReadOnlyCollection<DirectiveDescription> directiveDescriptions)
        {
            _source = source
                ?? throw new ArgumentNullException(nameof(source));
            _location = location;
            _descs = directiveDescriptions
                ?? throw new ArgumentNullException(
                    nameof(directiveDescriptions));
        }

        public int Count => _directives.Count;

        #region Initialization

        protected override void OnCompleteType(
            ITypeInitializationContext context)
        {
            var processed = new HashSet<string>();

            foreach (DirectiveDescription description in _descs)
            {
                CompleteDirective(context, description, processed);
            }
        }

        private void CompleteDirective(
            ITypeInitializationContext context,
            DirectiveDescription description,
            HashSet<string> processed)
        {
            DirectiveReference reference =
                DirectiveReference.FromDescription(description);
            DirectiveType directiveType = context.GetDirectiveType(reference);

            if (directiveType != null)
            {
                if (!processed.Add(directiveType.Name)
                    && !directiveType.IsRepeatable)
                {
                    context.ReportError(new SchemaError(
                        $"The specified directive `@{directiveType.Name}` " +
                        "is unique and cannot be added twice.",
                        context.Type as INamedType));
                }
                else if (directiveType.Locations.Contains(_location))
                {
                    _directives.Add(Directive.FromDescription(
                        directiveType, description, _source));
                }
                else
                {
                    context.ReportError(new SchemaError(
                        $"The specified directive `@{directiveType.Name}` " +
                        "is not allowed on the current location " +
                        $"`{_location}`.",
                        context.Type as INamedType));
                }
            }
        }

        #endregion

        public IEnumerator<IDirective> GetEnumerator()
        {
            return _directives.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
