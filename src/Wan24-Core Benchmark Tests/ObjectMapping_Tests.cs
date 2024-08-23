using AutoMapper;
using BenchmarkDotNet.Attributes;
using System.Diagnostics.Contracts;
using wan24.Core;

namespace Wan24_Core_Benchmark_Tests
{
    [MemoryDiagnoser]
    public class ObjectMapping_Tests
    {
        private static readonly SourceObject Source = new();
        private static readonly TargetObject Target = new();
        private static readonly ObjectMapping ObjectMapping;
        private static readonly IMapper AutoMapping;

        static ObjectMapping_Tests()
        {
            // Initialize wan24.Core.ObjectMapping
            ObjectMapping = ObjectMapping<SourceObject, TargetObject>.Create().AddAutoMappings();
            ObjectMapping.ApplyMappings(Source, Target);
            Contract.Assert(Target.Test);
            // Initialize AutoMapper
            AutoMapping = new MapperConfiguration(cfg => cfg.CreateMap<SourceObject, TargetObject>()).CreateMapper();
            Target.Test = false;
            AutoMapping.Map(Source, Target);
            Contract.Assert(Target.Test);
        }

        [Benchmark]
        public void Wan24Create()=> ObjectMapping<SourceObject, TargetObject>.Create().AddAutoMappings();

        [Benchmark]
        public void AutoMapperCreate() => new MapperConfiguration(cfg => cfg.CreateMap<SourceObject, TargetObject>()).CreateMapper();

        [Benchmark]
        public void Wan24Mapping() => ObjectMapping.ApplyMappings(Source, Target);

        [Benchmark]
        public void AutoMapper() => AutoMapping.Map(Source, Target);

        private sealed class SourceObject
        {
            public bool Test => true;
        }

        private sealed class TargetObject
        {
            public bool Test { get; set; }
        }
    }
}
