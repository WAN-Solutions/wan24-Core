namespace wan24.Core
{
    /// <summary>
    /// Interface for an extended plugin
    /// </summary>
    public interface IPluginExt : IPlugin
    {
        /// <summary>
        /// Initialize the plugin
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        Task OnInitAsync(CancellationToken cancellationToken = default);
        /// <summary>
        /// Unload the plugin
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        Task OnUnloadAsync(CancellationToken cancellationToken = default);
    }
}
