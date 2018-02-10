﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Query.ResultOperators.Internal;
using Remotion.Linq;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public static class CoreLoggerExtensions
    {
        private const int QueryModelStringLengthLimit = 100;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void SaveChangesFailed(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Update> diagnostics,
            [NotNull] DbContext context,
            [NotNull] Exception exception)
        {
            var definition = CoreStrings.LogExceptionDuringSaveChanges;

            var warningBehavior = definition.GetLogBehavior(diagnostics);
            if (warningBehavior != WarningBehavior.Ignore)
            {
                definition.Log(
                    diagnostics,
                    warningBehavior,
                    context.GetType(), Environment.NewLine, exception,
                    exception);
            }

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new DbContextErrorEventData(
                        definition,
                        SaveChangesFailed,
                        context,
                        exception));
            }
        }

        private static string SaveChangesFailed(EventDefinitionBase definition, EventData payload)
        {
            var d = (EventDefinition<Type, string, Exception>)definition;
            var p = (DbContextErrorEventData)payload;
            return d.GenerateMessage(p.Context.GetType(), Environment.NewLine, p.Exception);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void DuplicateDependentEntityTypeInstanceWarning(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Update> diagnostics,
            [NotNull] IEntityType dependent1,
            [NotNull] IEntityType dependent2)
        {
            var definition = CoreStrings.LogDuplicateDependentEntityTypeInstance;

            var warningBehavior = definition.GetLogBehavior(diagnostics);
            if (warningBehavior != WarningBehavior.Ignore)
            {
                definition.Log(
                    diagnostics,
                    warningBehavior,
                    dependent1.DisplayName(), dependent2.DisplayName());
            }

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new SharedDependentEntityEventData(
                        definition,
                        DuplicateDependentEntityTypeInstanceWarning,
                        dependent1,
                        dependent2));
            }
        }

        private static string DuplicateDependentEntityTypeInstanceWarning(EventDefinitionBase definition, EventData payload)
        {
            var d = (EventDefinition<string, string>)definition;
            var p = (SharedDependentEntityEventData)payload;
            return d.GenerateMessage(p.FirstEntityType.DisplayName(), p.SecondEntityType.DisplayName());
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void QueryIterationFailed(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Query> diagnostics,
            [NotNull] Type contextType,
            [NotNull] Exception exception)
        {
            var definition = CoreStrings.LogExceptionDuringQueryIteration;

            var warningBehavior = definition.GetLogBehavior(diagnostics);
            if (warningBehavior != WarningBehavior.Ignore)
            {
                definition.Log(
                    diagnostics,
                    warningBehavior,
                    contextType, Environment.NewLine, exception,
                    exception);
            }

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new DbContextTypeErrorEventData(
                        definition,
                        QueryIterationFailed,
                        contextType,
                        exception));
            }
        }

        private static string QueryIterationFailed(EventDefinitionBase definition, EventData payload)
        {
            var d = (EventDefinition<Type, string, Exception>)definition;
            var p = (DbContextTypeErrorEventData)payload;
            return d.GenerateMessage(p.ContextType, Environment.NewLine, p.Exception);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void QueryModelCompiling(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Query> diagnostics,
            [NotNull] QueryModel queryModel)
        {
            var definition = CoreStrings.LogCompilingQueryModel;

            var warningBehavior = definition.GetLogBehavior(diagnostics);
            if (warningBehavior != WarningBehavior.Ignore)
            {
                definition.Log(
                    diagnostics,
                    warningBehavior,
                    Environment.NewLine, queryModel.Print());
            }

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new QueryModelEventData(
                        definition,
                        QueryModelCompiling,
                        queryModel));
            }
        }

        private static string QueryModelCompiling(EventDefinitionBase definition, EventData payload)
        {
            var d = (EventDefinition<string, string>)definition;
            var p = (QueryModelEventData)payload;
            return d.GenerateMessage(Environment.NewLine, p.QueryModel.Print());
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void RowLimitingOperationWithoutOrderByWarning(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Query> diagnostics,
            [NotNull] QueryModel queryModel)
        {
            var definition = CoreStrings.LogRowLimitingOperationWithoutOrderBy;

            var warningBehavior = definition.GetLogBehavior(diagnostics);
            if (warningBehavior != WarningBehavior.Ignore)
            {
                definition.Log(
                    diagnostics,
                    warningBehavior,
                    queryModel.Print(removeFormatting: true, characterLimit: QueryModelStringLengthLimit));
            }

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new QueryModelEventData(
                        definition,
                        RowLimitingOperationWithoutOrderByWarning,
                        queryModel));
            }
        }

        private static string RowLimitingOperationWithoutOrderByWarning(EventDefinitionBase definition, EventData payload)
        {
            var d = (EventDefinition<string>)definition;
            var p = (QueryModelEventData)payload;
            return d.GenerateMessage(p.QueryModel.Print(removeFormatting: true, characterLimit: QueryModelStringLengthLimit));
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void FirstWithoutOrderByAndFilterWarning(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Query> diagnostics,
            [NotNull] QueryModel queryModel)
        {
            var definition = CoreStrings.LogFirstWithoutOrderByAndFilter;

            var warningBehavior = definition.GetLogBehavior(diagnostics);
            if (warningBehavior != WarningBehavior.Ignore)
            {
                definition.Log(
                    diagnostics,
                    warningBehavior,
                    queryModel.Print(removeFormatting: true, characterLimit: QueryModelStringLengthLimit));
            }

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new QueryModelEventData(
                        definition,
                        FirstWithoutOrderByAndFilterWarning,
                        queryModel));
            }
        }

        private static string FirstWithoutOrderByAndFilterWarning(EventDefinitionBase definition, EventData payload)
        {
            var d = (EventDefinition<string>)definition;
            var p = (QueryModelEventData)payload;
            return d.GenerateMessage(p.QueryModel.Print(removeFormatting: true, characterLimit: QueryModelStringLengthLimit));
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void QueryModelOptimized(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Query> diagnostics,
            [NotNull] QueryModel queryModel)
        {
            var definition = CoreStrings.LogOptimizedQueryModel;

            var warningBehavior = definition.GetLogBehavior(diagnostics);
            if (warningBehavior != WarningBehavior.Ignore)
            {
                definition.Log(
                    diagnostics,
                    warningBehavior,
                    Environment.NewLine, queryModel.Print());
            }

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new QueryModelEventData(
                        definition,
                        QueryModelOptimized,
                        queryModel));
            }
        }

        private static string QueryModelOptimized(EventDefinitionBase definition, EventData payload)
        {
            var d = (EventDefinition<string, string>)definition;
            var p = (QueryModelEventData)payload;
            return d.GenerateMessage(Environment.NewLine, p.QueryModel.Print());
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void NavigationIncluded(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Query> diagnostics,
            [NotNull] IncludeResultOperator includeResultOperator)
        {
            var definition = CoreStrings.LogIncludingNavigation;

            var warningBehavior = definition.GetLogBehavior(diagnostics);
            if (warningBehavior != WarningBehavior.Ignore)
            {
                definition.Log(
                    diagnostics,
                    warningBehavior,
                    includeResultOperator.DisplayString());
            }

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new IncludeEventData(
                        definition,
                        NavigationIncluded,
                        includeResultOperator));
            }
        }

        private static string NavigationIncluded(EventDefinitionBase definition, EventData payload)
        {
            var d = (EventDefinition<string>)definition;
            var p = (IncludeEventData)payload;
            return d.GenerateMessage(p.IncludeResultOperator.DisplayString());
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void QueryExecutionPlanned(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Query> diagnostics,
            [NotNull] IExpressionPrinter expressionPrinter,
            [NotNull] Expression queryExecutorExpression)
        {
            var definition = CoreStrings.LogQueryExecutionPlanned;

            var warningBehavior = definition.GetLogBehavior(diagnostics);
            if (warningBehavior != WarningBehavior.Ignore)
            {
                definition.Log(
                    diagnostics,
                    warningBehavior,
                    expressionPrinter.Print(queryExecutorExpression));
            }

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new QueryExpressionEventData(
                        definition,
                        QueryExecutionPlanned,
                        queryExecutorExpression,
                        expressionPrinter));
            }
        }

        private static string QueryExecutionPlanned(EventDefinitionBase definition, EventData payload)
        {
            var d = (EventDefinition<string>)definition;
            var p = (QueryExpressionEventData)payload;
            return d.GenerateMessage(p.ExpressionPrinter.Print(p.Expression));
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void SensitiveDataLoggingEnabledWarning<TLoggerCategory>(
            [NotNull] this IDiagnosticsLogger<TLoggerCategory> diagnostics)
            where TLoggerCategory : LoggerCategory<TLoggerCategory>, new()
        {
            var definition = CoreStrings.LogSensitiveDataLoggingEnabled;

            var warningBehavior = definition.GetLogBehavior(diagnostics);
            if (warningBehavior != WarningBehavior.Ignore)
            {
                definition.Log(diagnostics, warningBehavior);
            }

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new EventData(
                        definition,
                        (d, p) => ((EventDefinition)d).GenerateMessage()));
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void IncludeIgnoredWarning(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Query> diagnostics,
            [NotNull] IncludeResultOperator includeResultOperator)
        {
            var definition = CoreStrings.LogIgnoredInclude;

            var warningBehavior = definition.GetLogBehavior(diagnostics);
            if (warningBehavior != WarningBehavior.Ignore)
            {
                definition.Log(
                    diagnostics,
                    warningBehavior,
                    includeResultOperator.DisplayString());
            }

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new IncludeEventData(
                        definition,
                        IncludeIgnoredWarning,
                        includeResultOperator));
            }
        }

        private static string IncludeIgnoredWarning(EventDefinitionBase definition, EventData payload)
        {
            var d = (EventDefinition<string>)definition;
            var p = (IncludeEventData)payload;
            return d.GenerateMessage(p.IncludeResultOperator.DisplayString());
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void PossibleUnintendedCollectionNavigationNullComparisonWarning(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Query> diagnostics,
            [NotNull] IReadOnlyList<IPropertyBase> navigationPath)
        {
            var definition = CoreStrings.LogPossibleUnintendedCollectionNavigationNullComparison;

            var warningBehavior = definition.GetLogBehavior(diagnostics);
            if (warningBehavior != WarningBehavior.Ignore)
            {
                definition.Log(
                    diagnostics,
                    warningBehavior,
                    string.Join(".", navigationPath.Select(p => p.Name)));
            }

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new NavigationPathEventData(
                        definition,
                        PossibleUnintendedCollectionNavigationNullComparisonWarning,
                        navigationPath));
            }
        }

        private static string PossibleUnintendedCollectionNavigationNullComparisonWarning(EventDefinitionBase definition, EventData payload)
        {
            var d = (EventDefinition<string>)definition;
            var p = (NavigationPathEventData)payload;
            return d.GenerateMessage(string.Join(".", p.NavigationPath.Select(pb => pb.Name)));
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void PossibleUnintendedReferenceComparisonWarning(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Query> diagnostics,
            [NotNull] Expression left,
            [NotNull] Expression right)
        {
            var definition = CoreStrings.LogPossibleUnintendedReferenceComparison;

            var warningBehavior = definition.GetLogBehavior(diagnostics);
            if (warningBehavior != WarningBehavior.Ignore)
            {
                definition.Log(
                    diagnostics,
                    warningBehavior,
                    left, right);
            }

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new BinaryExpressionEventData(
                        definition,
                        PossibleUnintendedReferenceComparisonWarning,
                        left,
                        right));
            }
        }

        private static string PossibleUnintendedReferenceComparisonWarning(EventDefinitionBase definition, EventData payload)
        {
            var d = (EventDefinition<object, object>)definition;
            var p = (BinaryExpressionEventData)payload;
            return d.GenerateMessage(p.Left, p.Right);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void ServiceProviderCreated(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Infrastructure> diagnostics,
            [NotNull] IServiceProvider serviceProvider)
        {
            var definition = CoreStrings.LogServiceProviderCreated;

            var warningBehavior = definition.GetLogBehavior(diagnostics);
            if (warningBehavior != WarningBehavior.Ignore)
            {
                definition.Log(diagnostics, warningBehavior);
            }

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new ServiceProviderEventData(
                        definition,
                        (d, p) => ((EventDefinition)d).GenerateMessage(),
                        serviceProvider));
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void ManyServiceProvidersCreatedWarning(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Infrastructure> diagnostics,
            [NotNull] ICollection<IServiceProvider> serviceProviders)
        {
            var definition = CoreStrings.LogManyServiceProvidersCreated;

            var warningBehavior = definition.GetLogBehavior(diagnostics);
            if (warningBehavior != WarningBehavior.Ignore)
            {
                definition.Log(diagnostics, warningBehavior);
            }

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new ServiceProvidersEventData(
                        definition,
                        (d, p) => ((EventDefinition)d).GenerateMessage(),
                        serviceProviders));
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void ContextInitialized(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Infrastructure> diagnostics,
            [NotNull] DbContext context,
            [NotNull] DbContextOptions contextOptions)
        {
            var definition = CoreStrings.LogContextInitialized;

            var warningBehavior = definition.GetLogBehavior(diagnostics);
            if (warningBehavior != WarningBehavior.Ignore)
            {
                definition.Log(
                    diagnostics,
                    warningBehavior,
                    ProductInfo.GetVersion(),
                    context.GetType().ShortDisplayName(),
                    context.Database.ProviderName,
                    contextOptions.BuildOptionsFragment());
            }

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new ContextInitializedEventData(
                        definition,
                        ContextInitialized,
                        context,
                        contextOptions));
            }
        }

        private static string ContextInitialized(EventDefinitionBase definition, EventData payload)
        {
            var d = (EventDefinition<string, string, string, string>)definition;
            var p = (ContextInitializedEventData)payload;
            return d.GenerateMessage(
                ProductInfo.GetVersion(),
                p.Context.GetType().ShortDisplayName(),
                p.Context.Database.ProviderName,
                p.ContextOptions.BuildOptionsFragment());
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void ExecutionStrategyRetrying(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Infrastructure> diagnostics,
            [NotNull] IReadOnlyList<Exception> exceptionsEncountered,
            TimeSpan delay,
            bool async)
        {
            var definition = CoreStrings.LogExecutionStrategyRetrying;

            var warningBehavior = definition.GetLogBehavior(diagnostics);
            if (warningBehavior != WarningBehavior.Ignore)
            {
                var lastException = exceptionsEncountered[exceptionsEncountered.Count - 1];
                definition.Log(
                    diagnostics,
                    warningBehavior,
                    (int)delay.TotalMilliseconds, Environment.NewLine, lastException,
                    lastException);
            }

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new ExecutionStrategyEventData(
                        definition,
                        ExecutionStrategyRetrying,
                        exceptionsEncountered,
                        delay,
                        async));
            }
        }

        private static string ExecutionStrategyRetrying(EventDefinitionBase definition, EventData payload)
        {
            var d = (EventDefinition<int, string, Exception>)definition;
            var p = (ExecutionStrategyEventData)payload;
            return d.GenerateMessage(
                (int)p.Delay.TotalMilliseconds, Environment.NewLine, p.ExceptionsEncountered[p.ExceptionsEncountered.Count - 1]);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void LazyLoadOnDisposedContextWarning(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Infrastructure> diagnostics,
            [NotNull] DbContext context,
            [NotNull] object entityType,
            [NotNull] string navigationName)
        {
            var definition = CoreStrings.LogLazyLoadOnDisposedContextWarning;

            var warningBehavior = definition.GetLogBehavior(diagnostics);
            if (warningBehavior != WarningBehavior.Ignore)
            {
                definition.Log(
                    diagnostics,
                    warningBehavior,
                    navigationName, entityType.GetType().ShortDisplayName());
            }

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new LazyLoadingEventData(
                        definition,
                        LazyLoadOnDisposedContextWarning,
                        context,
                        entityType,
                        navigationName));
            }
        }

        private static string LazyLoadOnDisposedContextWarning(EventDefinitionBase definition, EventData payload)
        {
            var d = (EventDefinition<string, string>)definition;
            var p = (LazyLoadingEventData)payload;
            return d.GenerateMessage(p.NavigationPropertyName, p.Entity.GetType().ShortDisplayName());
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void NavigationLazyLoading(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Infrastructure> diagnostics,
            [NotNull] DbContext context,
            [NotNull] object entityType,
            [NotNull] string navigationName)
        {
            var definition = CoreStrings.LogNavigationLazyLoading;

            var warningBehavior = definition.GetLogBehavior(diagnostics);
            if (warningBehavior != WarningBehavior.Ignore)
            {
                definition.Log(
                    diagnostics,
                    warningBehavior,
                    navigationName, entityType.GetType().ShortDisplayName());
            }

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new LazyLoadingEventData(
                        definition,
                        NavigationLazyLoading,
                        context,
                        entityType,
                        navigationName));
            }
        }

        private static string NavigationLazyLoading(EventDefinitionBase definition, EventData payload)
        {
            var d = (EventDefinition<string, string>)definition;
            var p = (LazyLoadingEventData)payload;
            return d.GenerateMessage(p.NavigationPropertyName, p.Entity.GetType().ShortDisplayName());
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void ShadowPropertyCreated(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Model> diagnostics,
            [NotNull] IProperty property)
        {
            var definition = CoreStrings.LogShadowPropertyCreated;

            var warningBehavior = definition.GetLogBehavior(diagnostics);
            if (warningBehavior != WarningBehavior.Ignore)
            {
                definition.Log(
                    diagnostics,
                    warningBehavior,
                    property.Name, property.DeclaringEntityType.DisplayName());
            }

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new PropertyEventData(
                        definition,
                        ShadowPropertyCreated,
                        property));
            }
        }

        private static string ShadowPropertyCreated(EventDefinitionBase definition, EventData payload)
        {
            var d = (EventDefinition<string, string>)definition;
            var p = (PropertyEventData)payload;
            return d.GenerateMessage(p.Property.Name, p.Property.DeclaringEntityType.DisplayName());
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void RedundantIndexRemoved(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Model> diagnostics,
            [NotNull] IReadOnlyList<IPropertyBase> redundantIndex,
            [NotNull] IReadOnlyList<IPropertyBase> otherIndex)
        {
            var definition = CoreStrings.LogRedundantIndexRemoved;

            var warningBehavior = definition.GetLogBehavior(diagnostics);
            if (warningBehavior != WarningBehavior.Ignore)
            {
                definition.Log(
                    diagnostics,
                    warningBehavior,
                    Property.Format(redundantIndex), Property.Format(otherIndex));
            }

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new TwoPropertyBaseCollectionsEventData(
                        definition,
                        RedundantIndexRemoved,
                        redundantIndex,
                        otherIndex));
            }
        }

        private static string RedundantIndexRemoved(EventDefinitionBase definition, EventData payload)
        {
            var d = (EventDefinition<string, string>)definition;
            var p = (TwoPropertyBaseCollectionsEventData)payload;
            return d.GenerateMessage(
                Property.Format(p.FirstPropertyCollection),
                Property.Format(p.SecondPropertyCollection));
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void IncompatibleMatchingForeignKeyProperties(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Model> diagnostics,
            [NotNull] IReadOnlyList<IPropertyBase> foreignKeyProperties,
            [NotNull] IReadOnlyList<IPropertyBase> principalKeyProperties)
        {
            var definition = CoreStrings.LogIncompatibleMatchingForeignKeyProperties;

            var warningBehavior = definition.GetLogBehavior(diagnostics);
            if (warningBehavior != WarningBehavior.Ignore)
            {
                definition.Log(
                    diagnostics,
                    warningBehavior,
                    Property.Format(foreignKeyProperties, includeTypes: true),
                    Property.Format(principalKeyProperties, includeTypes: true));
            }

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new TwoPropertyBaseCollectionsEventData(
                        definition,
                        IncompatibleMatchingForeignKeyProperties,
                        foreignKeyProperties,
                        principalKeyProperties));
            }
        }

        private static string IncompatibleMatchingForeignKeyProperties(EventDefinitionBase definition, EventData payload)
        {
            var d = (EventDefinition<string, string>)definition;
            var p = (TwoPropertyBaseCollectionsEventData)payload;
            return d.GenerateMessage(
                Property.Format(p.FirstPropertyCollection, includeTypes: true),
                Property.Format(p.SecondPropertyCollection, includeTypes: true));
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void RequiredAttributeOnDependent(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Model> diagnostics,
            [NotNull] INavigation navigation)
        {
            var definition = CoreStrings.LogRequiredAttributeOnDependent;

            var warningBehavior = definition.GetLogBehavior(diagnostics);
            if (warningBehavior != WarningBehavior.Ignore)
            {
                definition.Log(
                    diagnostics,
                    warningBehavior,
                    navigation.Name, navigation.DeclaringEntityType.DisplayName());
            }

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new NavigationEventData(
                        definition,
                        RequiredAttributeOnDependent,
                        navigation));
            }
        }

        private static string RequiredAttributeOnDependent(EventDefinitionBase definition, EventData payload)
        {
            var d = (EventDefinition<string, string>)definition;
            var p = (NavigationEventData)payload;
            return d.GenerateMessage(p.Navigation.Name, p.Navigation.DeclaringEntityType.DisplayName());
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void RequiredAttributeOnBothNavigations(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Model> diagnostics,
            [NotNull] INavigation firstNavigation,
            [NotNull] INavigation secondNavigation)
        {
            var definition = CoreStrings.LogRequiredAttributeOnBothNavigations;

            var warningBehavior = definition.GetLogBehavior(diagnostics);
            if (warningBehavior != WarningBehavior.Ignore)
            {
                definition.Log(
                    diagnostics,
                    warningBehavior,
                    firstNavigation.DeclaringEntityType.DisplayName(),
                    firstNavigation.Name,
                    secondNavigation.DeclaringEntityType.DisplayName(),
                    secondNavigation.Name);
            }

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new TwoPropertyBaseCollectionsEventData(
                        definition,
                        RequiredAttributeOnBothNavigations,
                        new[] { firstNavigation },
                        new[] { secondNavigation }));
            }
        }

        private static string RequiredAttributeOnBothNavigations(EventDefinitionBase definition, EventData payload)
        {
            var d = (EventDefinition<string, string, string, string>)definition;
            var p = (TwoPropertyBaseCollectionsEventData)payload;
            var firstNavigation = p.FirstPropertyCollection[0];
            var secondNavigation = p.SecondPropertyCollection[0];
            return d.GenerateMessage(
                firstNavigation.DeclaringType.DisplayName(),
                firstNavigation.Name,
                secondNavigation.DeclaringType.DisplayName(),
                secondNavigation.Name);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void ConflictingShadowForeignKeys(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Model> diagnostics,
            [NotNull] IForeignKey foreignKey)
        {
            var definition = CoreStrings.LogConflictingShadowForeignKeys;

            var warningBehavior = definition.GetLogBehavior(diagnostics);
            if (warningBehavior != WarningBehavior.Ignore)
            {
                var declaringTypeName = foreignKey.DeclaringEntityType.DisplayName();
                definition.Log(
                    diagnostics,
                    warningBehavior,
                    declaringTypeName,
                    foreignKey.PrincipalEntityType.DisplayName(),
                    declaringTypeName);
            }

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new ForeignKeyEventData(
                        definition,
                        ConflictingShadowForeignKeys,
                        foreignKey));
            }
        }

        private static string ConflictingShadowForeignKeys(EventDefinitionBase definition, EventData payload)
        {
            var d = (EventDefinition<string, string, string>)definition;
            var p = (ForeignKeyEventData)payload;
            return d.GenerateMessage(
                p.ForeignKey.DeclaringEntityType.DisplayName(),
                p.ForeignKey.PrincipalEntityType.DisplayName(),
                p.ForeignKey.DeclaringEntityType.DisplayName());
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void MultiplePrimaryKeyCandidates(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Model> diagnostics,
            [NotNull] IProperty firstProperty,
            [NotNull] IProperty secondProperty)
        {
            var definition = CoreStrings.LogMultiplePrimaryKeyCandidates;

            var warningBehavior = definition.GetLogBehavior(diagnostics);
            if (warningBehavior != WarningBehavior.Ignore)
            {
                definition.Log(
                    diagnostics,
                    warningBehavior,
                    firstProperty.Name,
                    secondProperty.Name,
                    firstProperty.DeclaringEntityType.DisplayName());
            }

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new TwoPropertyBaseCollectionsEventData(
                        definition,
                        MultiplePrimaryKeyCandidates,
                        new[] { firstProperty },
                        new[] { secondProperty }));
            }
        }

        private static string MultiplePrimaryKeyCandidates(EventDefinitionBase definition, EventData payload)
        {
            var d = (EventDefinition<string, string, string>)definition;
            var p = (TwoPropertyBaseCollectionsEventData)payload;
            var firstProperty = p.FirstPropertyCollection[0];
            var secondProperty = p.SecondPropertyCollection[0];
            return d.GenerateMessage(
                firstProperty.Name,
                secondProperty.Name,
                firstProperty.DeclaringType.DisplayName());
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void MultipleNavigationProperties(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Model> diagnostics,
            [NotNull] IEnumerable<Tuple<MemberInfo, Type>> firstPropertyCollection,
            [NotNull] IEnumerable<Tuple<MemberInfo, Type>> secondPropertyCollection)
        {
            var definition = CoreStrings.LogMultipleNavigationProperties;

            var warningBehavior = definition.GetLogBehavior(diagnostics);
            if (warningBehavior != WarningBehavior.Ignore)
            {
                definition.Log(
                    diagnostics,
                    warningBehavior,
                    firstPropertyCollection.First().Item2.ShortDisplayName(),
                    secondPropertyCollection.First().Item2.ShortDisplayName(),
                    Property.Format(firstPropertyCollection.Select(p => p.Item1?.Name)),
                    Property.Format(secondPropertyCollection.Select(p => p.Item1?.Name)));
            }

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new TwoUnmappedPropertyCollectionsEventData(
                        definition,
                        MultipleNavigationProperties,
                        firstPropertyCollection,
                        secondPropertyCollection));
            }
        }

        private static string MultipleNavigationProperties(EventDefinitionBase definition, EventData payload)
        {
            var d = (EventDefinition<string, string, string, string>)definition;
            var p = (TwoUnmappedPropertyCollectionsEventData)payload;
            return d.GenerateMessage(
                p.FirstPropertyCollection.First().Item2.ShortDisplayName(),
                p.SecondPropertyCollection.First().Item2.ShortDisplayName(),
                Property.Format(p.FirstPropertyCollection.Select(i => i.Item1?.Name)),
                Property.Format(p.SecondPropertyCollection.Select(i => i.Item1?.Name)));
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void MultipleInversePropertiesSameTarget(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Model> diagnostics,
            [NotNull] IEnumerable<Tuple<MemberInfo, Type>> conflictingNavigations,
            [NotNull] MemberInfo inverseNavigation,
            [NotNull] Type targetType)
        {
            var definition = CoreStrings.LogMultipleInversePropertiesSameTarget;

            var warningBehavior = definition.GetLogBehavior(diagnostics);
            if (warningBehavior != WarningBehavior.Ignore)
            {
                definition.Log(
                    diagnostics,
                    warningBehavior,
                    string.Join(", ", conflictingNavigations.Select(n => n.Item2.ShortDisplayName() + "." + n.Item1.Name)),
                    inverseNavigation.Name);
            }

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new TwoUnmappedPropertyCollectionsEventData(
                        definition,
                        MultipleInversePropertiesSameTarget,
                        conflictingNavigations,
                        new[] { new Tuple<MemberInfo, Type>(inverseNavigation, targetType) }));
            }
        }

        private static string MultipleInversePropertiesSameTarget(EventDefinitionBase definition, EventData payload)
        {
            var d = (EventDefinition<string, string>)definition;
            var p = (TwoUnmappedPropertyCollectionsEventData)payload;
            return d.GenerateMessage(string.Join(
                ", ", p.FirstPropertyCollection.Select(n => n.Item2.ShortDisplayName() + "." + n.Item1.Name)),
                p.SecondPropertyCollection.First().Item1.Name);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void NonDefiningInverseNavigation(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Model> diagnostics,
            [NotNull] IEntityType declaringType,
            [NotNull] MemberInfo navigation,
            [NotNull] IEntityType targetType,
            [NotNull] MemberInfo inverseNavigation,
            [NotNull] MemberInfo definingNavigation)
        {
            var definition = CoreStrings.LogNonDefiningInverseNavigation;

            var warningBehavior = definition.GetLogBehavior(diagnostics);
            if (warningBehavior != WarningBehavior.Ignore)
            {
                definition.Log(
                    diagnostics,
                    warningBehavior,
                    targetType.DisplayName(),
                    inverseNavigation.Name,
                    declaringType.DisplayName(),
                    navigation.Name,
                    definingNavigation.Name);
            }

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new TwoUnmappedPropertyCollectionsEventData(
                        definition,
                        NonDefiningInverseNavigation,
                        new[] { new Tuple<MemberInfo, Type>(navigation, declaringType.ClrType) },
                        new[]
                        {
                            new Tuple<MemberInfo, Type>(inverseNavigation, targetType.ClrType),
                            new Tuple<MemberInfo, Type>(definingNavigation, targetType.ClrType)
                        }));
            }
        }

        private static string NonDefiningInverseNavigation(EventDefinitionBase definition, EventData payload)
        {
            var d = (EventDefinition<string, string, string, string, string>)definition;
            var p = (TwoUnmappedPropertyCollectionsEventData)payload;
            var navigation = p.FirstPropertyCollection.First();
            var inverseNavigation = p.SecondPropertyCollection.First();
            var definingNavigation = p.SecondPropertyCollection.Last();
            return d.GenerateMessage(
                inverseNavigation.Item2.ShortDisplayName(),
                inverseNavigation.Item1.Name,
                navigation.Item2.ShortDisplayName(),
                navigation.Item1.Name,
                definingNavigation.Item1.Name);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void NonOwnershipInverseNavigation(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Model> diagnostics,
            [NotNull] IEntityType declaringType,
            [NotNull] MemberInfo navigation,
            [NotNull] IEntityType targetType,
            [NotNull] MemberInfo inverseNavigation,
            [NotNull] MemberInfo ownershipNavigation)
        {
            var definition = CoreStrings.LogNonOwnershipInverseNavigation;

            var warningBehavior = definition.GetLogBehavior(diagnostics);
            if (warningBehavior != WarningBehavior.Ignore)
            {
                definition.Log(
                    diagnostics,
                    warningBehavior,
                    targetType.DisplayName(),
                    inverseNavigation.Name,
                    declaringType.DisplayName(),
                    navigation.Name,
                    ownershipNavigation.Name);
            }

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new TwoUnmappedPropertyCollectionsEventData(
                        definition,
                        NonOwnershipInverseNavigation,
                        new[] { new Tuple<MemberInfo, Type>(navigation, declaringType.ClrType) },
                        new[]
                        {
                            new Tuple<MemberInfo, Type>(inverseNavigation, targetType.ClrType),
                            new Tuple<MemberInfo, Type>(ownershipNavigation, targetType.ClrType)
                        }));
            }
        }

        private static string NonOwnershipInverseNavigation(EventDefinitionBase definition, EventData payload)
        {
            var d = (EventDefinition<string, string, string, string, string>)definition;
            var p = (TwoUnmappedPropertyCollectionsEventData)payload;
            var navigation = p.FirstPropertyCollection.First();
            var inverseNavigation = p.SecondPropertyCollection.First();
            var ownershipNavigation = p.SecondPropertyCollection.Last();
            return d.GenerateMessage(
                inverseNavigation.Item2.ShortDisplayName(),
                inverseNavigation.Item1.Name,
                navigation.Item2.ShortDisplayName(),
                navigation.Item1.Name,
                ownershipNavigation.Item1.Name);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void ForeignKeyAttributesOnBothProperties(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Model> diagnostics,
            [NotNull] INavigation firstNavigation,
            [NotNull] INavigation secondNavigation,
            [NotNull] MemberInfo firstProperty,
            [NotNull] MemberInfo secondProperty)
        {
            var definition = CoreStrings.LogForeignKeyAttributesOnBothProperties;

            var warningBehavior = definition.GetLogBehavior(diagnostics);
            if (warningBehavior != WarningBehavior.Ignore)
            {
                definition.Log(
                    diagnostics,
                    warningBehavior,
                    firstNavigation.DeclaringEntityType.ClrType.ShortDisplayName(),
                    firstNavigation.GetDefiningMemberInfo().Name,
                    secondNavigation.DeclaringEntityType.ClrType.ShortDisplayName(),
                    secondNavigation.GetDefiningMemberInfo().Name,
                    firstProperty.Name,
                    secondProperty.Name);
            }

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new TwoUnmappedPropertyCollectionsEventData(
                        definition,
                        ForeignKeyAttributesOnBothProperties,
                        new[]
                        {
                            new Tuple<MemberInfo, Type>(firstNavigation.GetDefiningMemberInfo(), firstNavigation.DeclaringEntityType.ClrType),
                            new Tuple<MemberInfo, Type>(firstProperty, firstNavigation.DeclaringEntityType.ClrType)
                        },
                        new[]
                        {
                            new Tuple<MemberInfo, Type>(secondNavigation.GetDefiningMemberInfo(), secondNavigation.DeclaringEntityType.ClrType),
                            new Tuple<MemberInfo, Type>(secondProperty, secondNavigation.DeclaringEntityType.ClrType)
                        }));
            }
        }

        private static string ForeignKeyAttributesOnBothProperties(EventDefinitionBase definition, EventData payload)
        {
            var d = (EventDefinition<string, string, string, string, string, string>)definition;
            var p = (TwoUnmappedPropertyCollectionsEventData)payload;
            var firstNavigation = p.FirstPropertyCollection.First();
            var firstProperty = p.FirstPropertyCollection.Last();
            var secondNavigation = p.SecondPropertyCollection.First();
            var secondProperty = p.SecondPropertyCollection.Last();
            return d.GenerateMessage(
                firstNavigation.Item2.ShortDisplayName(),
                firstNavigation.Item1.Name,
                secondNavigation.Item2.ShortDisplayName(),
                secondNavigation.Item1.Name,
                firstProperty.Item1.Name,
                secondProperty.Item1.Name);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void ForeignKeyAttributesOnBothNavigations(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Model> diagnostics,
            [NotNull] INavigation firstNavigation,
            [NotNull] INavigation secondNavigation)
        {
            var definition = CoreStrings.LogForeignKeyAttributesOnBothNavigations;

            var warningBehavior = definition.GetLogBehavior(diagnostics);
            if (warningBehavior != WarningBehavior.Ignore)
            {
                definition.Log(
                    diagnostics,
                    warningBehavior,
                    firstNavigation.DeclaringEntityType.DisplayName(),
                    firstNavigation.Name,
                    secondNavigation.DeclaringEntityType.DisplayName(),
                    secondNavigation.Name);
            }

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new TwoPropertyBaseCollectionsEventData(
                        definition,
                        ForeignKeyAttributesOnBothNavigations,
                        new[] { firstNavigation },
                        new[] { secondNavigation }));
            }
        }

        private static string ForeignKeyAttributesOnBothNavigations(EventDefinitionBase definition, EventData payload)
        {
            var d = (EventDefinition<string, string, string, string>)definition;
            var p = (TwoPropertyBaseCollectionsEventData)payload;
            var firstNavigation = p.FirstPropertyCollection[0];
            var secondNavigation = p.SecondPropertyCollection[0];
            return d.GenerateMessage(
                firstNavigation.DeclaringType.DisplayName(),
                firstNavigation.Name,
                secondNavigation.DeclaringType.DisplayName(),
                secondNavigation.Name);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void ConflictingForeignKeyAttributesOnNavigationAndProperty(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Model> diagnostics,
            [NotNull] INavigation navigation,
            [NotNull] MemberInfo property)
        {
            var definition = CoreStrings.LogConflictingForeignKeyAttributesOnNavigationAndProperty;

            var warningBehavior = definition.GetLogBehavior(diagnostics);
            if (warningBehavior != WarningBehavior.Ignore)
            {
                definition.Log(
                    diagnostics,
                    warningBehavior,
                    navigation.DeclaringEntityType.ClrType.ShortDisplayName(),
                    navigation.GetDefiningMemberInfo()?.Name,
                    property.DeclaringType.ShortDisplayName(),
                    property.Name);
            }

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new TwoUnmappedPropertyCollectionsEventData(
                        definition,
                        ConflictingForeignKeyAttributesOnNavigationAndProperty,
                        new[] { new Tuple<MemberInfo, Type>(navigation.GetDefiningMemberInfo(), navigation.DeclaringEntityType.ClrType) },
                        new[] { new Tuple<MemberInfo, Type>(property, property.DeclaringType) }));
            }
        }

        private static string ConflictingForeignKeyAttributesOnNavigationAndProperty(EventDefinitionBase definition, EventData payload)
        {
            var d = (EventDefinition<string, string, string, string>)definition;
            var p = (TwoUnmappedPropertyCollectionsEventData)payload;
            var navigation = p.FirstPropertyCollection.First();
            var property = p.SecondPropertyCollection.First();
            return d.GenerateMessage(
                navigation.Item2.ShortDisplayName(),
                navigation.Item1.Name,
                property.Item2.ShortDisplayName(),
                property.Item1.Name);
        }
    }
}
