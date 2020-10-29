#if !NET_4_6
using System;
using BeatThat.Requests;
using BeatThat.Service;

namespace BeatThat.Entities.Examples
{
    /// <summary>
    /// When using Net2.0 (no async/await) override the version of Resolve below
    ///
    /// NOTE: the [RegisterEntityStore] attribute below is disabled by default
    /// to keep this service from auto registering in your app where you don't want it.
    /// </summary>
#if ENTITY_EXAMPLES_AUTOREGISTER_SERVICES_ENABLED 
    [RegisterEntityService] // basically same as RegisterService, but also ensures that interface HasEntities<DataType> gets registered
#endif
    public class DogDataStore : EntityService<DogData> 
    {
        public override Request<ResolveResultDTO<DogData>> Resolve(ResolveRequestDTO req, Action<Request<ResolveResultDTO<DogData>>> callback = null)
        {
            var key = req.key;
            var path = DogAPI.GetDogUrl(key);
            var promise = new Promise<ResolveResultDTO<DogData>>((resolve, reject) =>
            {
                new WebRequest<DogData>(path).Execute(r => {
                    if(r.hasError) {
                        reject(ResolveResultDTO<DogData>.ResolveError(req, r.error));
                        return;
                    }

                    resolve(ResolveResultDTO<DogData>.ResolveSucceeded(
                        req, r.item.id, r.item
                    ));
                });
            });
            promise.Execute(callback);
            return promise;
        }
    }
}
#endif
