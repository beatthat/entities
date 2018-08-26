#if NET_4_6
using System;
using System.Threading.Tasks;
using BeatThat.Bindings;
using BeatThat.Requests;

namespace BeatThat.Entities
{
    /// <summary>
    /// Base class for EntityResolver,
    /// exists mainly to provide a hidden/default impl of (non async) Resolve
    /// that wraps the async version.
    /// 
    /// If async/await is avaialable, you should probably only be implementing ResolveAsync
    /// </summary>
    public abstract class EntityResolverService<DataType> : BindingService, EntityResolver<DataType>
    {
        /// <summary>
        /// Wraps call to ResolveAsync in a request
        /// </summary>
        public Request<ResolveResultDTO<DataType>> Resolve(string key, Action<Request<ResolveResultDTO<DataType>>> callback = null)
        {
            var r = new TaskRequest<ResolveResultDTO<DataType>>(ResolveAsync(key));
            r.Execute(callback);
            return r;
        }

        abstract public Task<ResolveResultDTO<DataType>> ResolveAsync(string key);
    }
}
#endif

