﻿using System;
using System.Linq;
using System.Reflection;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;

namespace Benchmarks.Utilities
{
    public class MethodResultColumn : IColumn
    {
        private readonly Func<object, string> formatter;

        public MethodResultColumn(string columnName, Func<object, string> formatter, string legend = null)
        {
            this.ColumnName = columnName;
            this.formatter = formatter;
            this.Legend = legend;
        }

        public string GetValue(Summary summary, BenchmarkCase benchmarkCase) => GetValue(summary, benchmarkCase, null);

        public string GetValue(Summary summary, BenchmarkCase benchmarkCase, ISummaryStyle style) => this.formatter(CallMethod(benchmarkCase));

        private static object CallMethod(BenchmarkCase benchmarkCase)
        {
            try
            {
                var descriptor = benchmarkCase.Descriptor;
                var instance = Activator.CreateInstance(descriptor.Type);
                TryInvoke(instance, descriptor.GlobalSetupMethod);
                TryInvoke(instance, descriptor.IterationSetupMethod);
                var result = descriptor.WorkloadMethod.Invoke(instance, Array.Empty<object>());
                TryInvoke(instance, descriptor.IterationCleanupMethod);
                TryInvoke(instance, descriptor.GlobalCleanupMethod);

                return result;
                void TryInvoke(object target, MethodInfo method)
                {
                    try
                    {
                        method?.Invoke(target, Array.Empty<object>());
                    }
                    catch
                    {
                    }
                }
            }
            catch
            {
                return null;
            }
        }

        public bool IsDefault(Summary summary, BenchmarkCase benchmarkCase) => false;

        public bool IsAvailable(Summary summary) => summary.Reports.Any(r => CallMethod(r.BenchmarkCase) != null);

        public string Id => nameof(MethodResultColumn) + "_" + this.ColumnName;
        public string ColumnName { get; }
        public bool AlwaysShow => true;
        public ColumnCategory Category => ColumnCategory.Diagnoser;
        public int PriorityInCategory => 0;
        public bool IsNumeric => true;
        public UnitType UnitType => UnitType.Size;
        public string Legend { get; }
    }
}