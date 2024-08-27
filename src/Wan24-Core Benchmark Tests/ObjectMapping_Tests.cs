using AutoMapper;
using BenchmarkDotNet.Attributes;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using wan24.Core;

namespace Wan24_Core_Benchmark_Tests
{
    [MemoryDiagnoser]
    public class ObjectMapping_Tests
    {
        private static readonly SourceObject Source = new();
        private static readonly TargetObject Target = new();
        private static readonly ObjectMapping<SourceObject, TargetObject> ObjectMapping;
        private static readonly Action<SourceObject, TargetObject> ObjectMappingCompiled;
        private static readonly IMapper AutoMapping;

        static ObjectMapping_Tests()
        {
            // Initialize wan24.Core.ObjectMapping
            ObjectMapping = ObjectMapping<SourceObject, TargetObject>.Create();
            ObjectMapping.AddAutoMappings();
            ObjectMapping.ApplyMappings(Source, Target);
            Contract.Assert(Target.Test);
            ObjectMapping<SourceObject, TargetObject>  objectMappingCompiled = ObjectMapping<SourceObject, TargetObject>.Create();
            objectMappingCompiled.AddAutoMappings();
            objectMappingCompiled.CompileMapping();
            ObjectMappingCompiled = objectMappingCompiled.CompiledMapping ?? throw new InvalidProgramException();
            Target.Test = false;
            ObjectMappingCompiled(Source, Target);
            Contract.Assert(Target.Test);
            // Initialize AutoMapper
            AutoMapping = new MapperConfiguration(cfg => cfg.CreateMap<SourceObject, TargetObject>()).CreateMapper();
            Target.Test = false;
            AutoMapping.Map(Source, Target);
            Contract.Assert(Target.Test);
        }

        [Benchmark]
        public void Wan24Create() => ObjectMapping<SourceObject, TargetObject>.Create().AddAutoMappings();

        [Benchmark]
        public void Wan24CreateCompiled()
        {
            ObjectMapping<SourceObject, TargetObject>  mapping = ObjectMapping<SourceObject, TargetObject>.Create();
            mapping.AddAutoMappings();
            mapping.CompileMapping();
        }

        [Benchmark]
        public void AutoMapperCreate() => new MapperConfiguration(cfg => cfg.CreateMap<SourceObject, TargetObject>()).CreateMapper();

        [Benchmark]
        public void Wan24Mapping() => ObjectMapping.ApplyMappings(Source, Target);

        [Benchmark]
        public void Wan24MappingCompiled() => ObjectMappingCompiled(Source, Target);

        [Benchmark]
        public void AutoMapper() => AutoMapping.Map(Source, Target);

        [Benchmark]
        public void NetMapping() => Source.MapTo(Target);

        private sealed class SourceObject
        {
            public bool Test { get; } = true;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void MapTo(TargetObject target) => target.Test = Test;
        }

        private sealed class TargetObject
        {
            public bool Test { get; set; }
        }
    }
}
