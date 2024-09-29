namespace wan24.Core
{
    /// <summary>
    /// Base class for a pipeline stream element which can process an object (implementing types need to implement at last one <see cref="IPipelineElementObject{T}"/> interface)
    /// </summary>
    /// <param name="name">Name</param>
    public abstract class PipelineElementObjectBase(in string name) : PipelineElementBase(name), IPipelineElementObject
    {
    }
}
