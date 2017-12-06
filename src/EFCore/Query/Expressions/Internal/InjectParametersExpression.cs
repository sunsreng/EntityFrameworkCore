// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Utilities;
using JetBrains.Annotations;
using System.Diagnostics;
using System.Reflection;

namespace Microsoft.EntityFrameworkCore.Query.Expressions.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class InjectParametersExpression : Expression
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public InjectParametersExpression(
            [NotNull] IReadOnlyList<Expression> parameterNames,
            [NotNull] IReadOnlyList<Expression> parameterValues,
            [NotNull] Expression query)
        {
            Check.NotEmpty(parameterNames, nameof(parameterNames));
            Check.NotEmpty(parameterValues, nameof(parameterValues));
            Check.NotNull(query, nameof(query));

            Debug.Assert(parameterNames.Count == parameterValues.Count);

            ParameterNames = parameterNames;
            ParameterValues = parameterValues;
            Query = query;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public IReadOnlyList<Expression> ParameterNames { get; private set; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public IReadOnlyList<Expression> ParameterValues { get; private set; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public Expression Query { get; private set; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override Type Type => Query.Type;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override bool CanReduce => true;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override ExpressionType NodeType => ExpressionType.Extension;

        private static readonly MethodInfo _setParameterMethodInfo
            = typeof(QueryContext).GetTypeInfo().GetDeclaredMethod(nameof(QueryContext.SetParameter));

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override Expression Reduce()
        {
            var expressions = new List<Expression>();

            for (var i = 0; i < ParameterNames.Count; i++)
            {
                expressions.Add(
                    Call(
                        EntityQueryModelVisitor.QueryContextParameter,
                        _setParameterMethodInfo,
                        ParameterNames[i],
                        ParameterValues[i]));
            }

            expressions.Add(Query);

            return Block(expressions);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            return this;
        }
    }
}
