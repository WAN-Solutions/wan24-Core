using AutoMapper;
using BenchmarkDotNet.Attributes;
using System.Diagnostics.Contracts;
using wan24.Core;

namespace Wan24_Core_Benchmark_Tests
{
    [MemoryDiagnoser]
    public class ObjectMapping_Tests
    {
        private static readonly ObjectMapping ObjectMapping;
        private static readonly IMapper AutoMapping;

        static ObjectMapping_Tests()
        {
            SourceObject source = new();
            TargetObject target = new();
            // Initialize wan24.Core.ObjectMapping
            ObjectMapping = ObjectMapping<SourceObject, TargetObject>.Create().AddAutoMappings();
            ObjectMapping.ApplyMappings(source, target);
            Contract.Assert(target.Test == source.Test);
            target.Test = null;
            // Initialize AutoMapper
            AutoMapping = new MapperConfiguration(cfg => cfg.CreateMap<SourceObject, TargetObject>()).CreateMapper();
            AutoMapping.Map(source, target);
            Contract.Assert(target.Test == source.Test);
        }

        [Benchmark]
        public void Wan24Mapping() => ObjectMapping.ApplyMappings(new SourceObject(), new TargetObject());

        [Benchmark]
        public void AutoMapper() => AutoMapping.Map(new SourceObject(), new TargetObject());
    }
}
