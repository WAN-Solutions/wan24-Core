namespace wan24.Core
{
    // Mapper info
    public partial class ObjectMapping
    {
        /// <summary>
        /// Mapper information
        /// </summary>
        /// <remarks>
        /// Constructor
        /// </remarks>
        /// <param name="SourceProperty">Source property</param>
        /// <param name="TargetProperty">Target property</param>
        /// <param name="Mapper">Mapper delegate</param>
        /// <param name="Type">Mapper delegate type</param>
        /// <param name="SourceMappingConfig"><c>SourceProperty</c> <see cref="MapAttribute"/></param>
        /// <param name="CustomKey">Custom mapper key</param>
        /// <param name="Condition">Mapping condition</param>
        protected sealed record class MapperInfo(
            in PropertyInfoExt? SourceProperty, 
            in PropertyInfoExt? TargetProperty, 
            in object Mapper, 
            in MapperType Type,
            in MapAttribute? SourceMappingConfig = null,
            in string? CustomKey = null,
            in object? Condition = null
            )
        {
            /// <summary>
            /// Mapper delegate type
            /// </summary>
            public readonly MapperType Type = Type;
            /// <summary>
            /// Mapper delegate
            /// </summary>
            public readonly object Mapper = Mapper;
            /// <summary>
            /// Source property (used as mapper key; if <see langword="null"/>, <see cref="CustomKey"/> will be used instead)
            /// </summary>
            public readonly PropertyInfoExt? SourceProperty = SourceProperty;
            /// <summary>
            /// <see cref="SourceProperty"/> <see cref="MapAttribute"/>
            /// </summary>
            public readonly MapAttribute? SourceMappingConfig = SourceMappingConfig ?? SourceProperty?.GetCustomAttributeCached<MapAttribute>();
            /// <summary>
            /// Target property
            /// </summary>
            public readonly PropertyInfoExt? TargetProperty = TargetProperty;
            /// <summary>
            /// Custom mapper key
            /// </summary>
            public readonly string? CustomKey = CustomKey;
            /// <summary>
            /// Condition
            /// </summary>
            public readonly object? Condition = Condition;
        }

        /// <summary>
        /// Mapper types enumeration
        /// </summary>
        protected enum MapperType : byte
        {
            /// <summary>
            /// Usual mapper
            /// </summary>
            [DisplayText("Usual mapper")]
            Mapper,
            /// <summary>
            /// Generic usual mapper
            /// </summary>
            [DisplayText("Generic usual mapper")]
            GenericMapper,
            /// <summary>
            /// Nested mapper
            /// </summary>
            [DisplayText("Nested mapper")]
            NestedMapper,
            /// <summary>
            /// Generic nested mapper
            /// </summary>
            [DisplayText("Generic nested mapper")]
            GenericNestedMapper,
            /// <summary>
            /// Map method call
            /// </summary>
            [DisplayText("Map method call")]
            MapCall,
            /// <summary>
            /// Generic map method call
            /// </summary>
            [DisplayText("Generic map method call")]
            GenericMapCall,
            /// <summary>
            /// Expression
            /// </summary>
            [DisplayText("Expression")]
            Expression,
            /// <summary>
            /// Generic expression
            /// </summary>
            [DisplayText("Generic expression")]
            GenericExpression,
            /// <summary>
            /// Custom mapper
            /// </summary>
            [DisplayText("Custom mapper")]
            CustomMapper,
            /// <summary>
            /// Any asynchronous mapper
            /// </summary>
            [DisplayText("Any asynchronous mapper")]
            AnyAsync
        }
    }
}
