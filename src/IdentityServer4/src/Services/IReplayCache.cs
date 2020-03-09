using System;
using System.Threading.Tasks;

namespace IdentityServer4.Services
{
    /// <summary>
    /// Interface for replay cache implementations
    /// </summary>
    public interface IReplayCache
    {
        /// <summary>
        /// Adds a handle to the cache 
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="expireTimeSpan"></param>
        /// <returns></returns>
        Task AddAsync(string handle, TimeSpan expireTimeSpan);


        /// <summary>
        /// Checks if a cached handle exists 
        /// </summary>
        /// <param name="handle"></param>
        /// <returns></returns>
        Task<bool> ExistsAsync(string handle);
    }
}