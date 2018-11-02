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
        virtual protected ResolveResultDTO<DataType> GetStoredEntityAsResolveResult(ResolveRequestDTO req)
        {
            if(string.IsNullOrEmpty(req.key)) {
                return ResolveResultDTO<DataType>.ResolveError(
                    req, "null or empty key"
                );
            }

            var store = Services.Require<HasEntities<DataType>>();
            Entity<DataType> entity;
            if (!store.GetEntity(req.key, out entity) || !entity.status.hasResolved)
            {
                return string.IsNullOrEmpty(entity.status.resolveError) ?
                             ResolveResultDTO<DataType>.ResolveNotFound(req) :
                             ResolveResultDTO<DataType>.ResolveError(
                                 req, entity.status.resolveError
                                );
            }

            var status = entity.status;

            return ResolveResultDTO<DataType>.ResolveSucceeded(
                req, entity.id, entity.data, status.maxAgeSecs, status.timestamp
            );
        }

#if NET_4_6
        /// <summary>
        /// Wraps call to ResolveAsync in a request
        /// </summary>
        virtual public Request<ResolveResultDTO<DataType>> Resolve(
            ResolveRequestDTO req, 
            Action<Request<ResolveResultDTO<DataType>>> callback = null
        )
        {
            var r = new TaskRequest<ResolveResultDTO<DataType>>(ResolveAsync(req));
            r.Execute(callback);
            return r;
        }

#pragma warning disable 1998
        virtual public async Task<ResolveResultDTO<DataType>> ResolveAsync(
            ResolveRequestDTO req
        )
#pragma warning restore 1998
        {
            return GetStoredEntityAsResolveResult(req);
        }
#else
        virtual public Request<ResolveResultDTO<DataType>> Resolve(
            ResolveRequestDTO req, 
            Action<Request<ResolveResultDTO<DataType>>> callback = null)
        {
            var result = GetStoredEntityAsResolveResult(req);
            var request = new LocalRequest<ResolveResultDTO<DataType>>(result);
            request.Execute(callback);
            return request;
        }
#endif

    }
}
