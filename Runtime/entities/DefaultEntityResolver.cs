using System;
using BeatThat.Bindings;
using BeatThat.Requests;
using BeatThat.Service;

#if NET_4_6
using System.Threading.Tasks;
#endif

namespace BeatThat.Entities
{
    /// <summary>
    /// The default entity resolver just checks the EntityStore
    /// and returns whatever is already there.
    ///
    /// The more common case is that entites are resolved by, say,
    /// making a REST request.
    ///
    /// Even if you're overriding the resolve behaviour,
    /// you may still want to use this base class
    /// and override ResolveAsync though,
    /// to save you from having to write the boilerplate call that
    /// maps pre NET4.6 resolves to the async version of the function.
    /// </summary>
    public class DefaultEntityResolver<DataType> : BindingService, EntityResolver<DataType>
    {
        virtual protected ResolveResultDTO<DataType> GetStoredEntityAsResolveResult(string key)
        {
            var store = Services.Require<HasEntities<DataType>>();
            Entity<DataType> entity;
            if (!store.GetEntity(key, out entity) || !entity.status.hasResolved)
            {
                return new ResolveResultDTO<DataType>
                {
                    status = string.IsNullOrEmpty(entity.status.resolveError) ?
                                   ResolveStatusCode.ERROR : ResolveStatusCode.NOT_FOUND,
                    message = entity.status.resolveError,
                    id = key,
                    key = key,
                    timestamp = DateTimeOffset.Now
                };
            }

            var status = entity.status;

            return new ResolveResultDTO<DataType>
            {
                status = ResolveStatusCode.OK,
                id = entity.id,
                key = key,
                timestamp = status.timestamp,
                maxAgeSecs = status.maxAgeSecs,
                data = entity.data
            };
        }

#if NET_4_6
        /// <summary>
        /// Wraps call to ResolveAsync in a request
        /// </summary>
        virtual public Request<ResolveResultDTO<DataType>> Resolve(string key, Action<Request<ResolveResultDTO<DataType>>> callback = null)
        {
            var r = new TaskRequest<ResolveResultDTO<DataType>>(ResolveAsync(key));
            r.Execute(callback);
            return r;
        }

#pragma warning disable 1998
        virtual public async Task<ResolveResultDTO<DataType>> ResolveAsync(string key)
#pragma warning restore 1998
        {
            return GetStoredEntityAsResolveResult(key);
        }
#else
        virtual public Request<ResolveResultDTO<DataType>> Resolve(string key, Action<Request<ResolveResultDTO<DataType>>> callback = null)
        {
            var result = GetStoredEntityAsResolveResult(key);
            var request = new LocalRequest<ResolveResultDTO<DataType>>(result);
            request.Execute(callback);
            return request;
        }
#endif

    }
}
