using Microsoft.Extensions.DependencyInjection;

namespace wan24.Core
{
    // Service provider
    public partial class ScopedDiHelper
    {
        /// <summary>
        /// DI service provider (will be disposed!)
        /// </summary>
        new public IServiceProvider? ServiceProvider { get; set; }
    }
}
