#if NET_4_6
using System;
using System.Threading.Tasks;
using BeatThat.Requests;
using BeatThat.Service;

namespace BeatThat.Entities.Examples
{

    /// <summary>
    /// EntityResolver<T> is the meat of the implementation you need to provide
    /// in most Entity<T> set ups.
    /// 
    /// If you're using NET_4_6 then you can extend EntityResolverService<T>
    /// and the only implemenation you need to provide 
    /// is for ResolveAsync, which takes a key and returns an Entity instance as below.
    /// 
    /// NOTE: the [RegisterService] attribute below is disabled by default
    /// to keep this service from auto registering in your app where you don't want it.
    /// </summary>
#if ENTITY_EXAMPLES_AUTOREGISTER_SERVICES_ENABLED 
    [RegisterService(typeof(EntityResolver<DogData>))]
#endif
    public class DogDataResolver : DefaultEntityResolver<DogData>
    {
        public override async Task<ResolveResultDTO<DogData>> ResolveAsync(
            ResolveRequestDTO req
        )
        {
            var path = DogAPI.GetDogUrl(req.key);

            try {
                
                // using BeatThat.Requests.WebRequest here, just an easy way 
                // to make a one-line HTTP request and get a typed result
                var data = await new WebRequest<DogData>(path).ExecuteAsync();

                return ResolveResultDTO<DogData>.ResolveSucceeded(
                    req, data.id, data
                );
            }
            catch(Exception e) {
                return ResolveResultDTO<DogData>.ResolveError(
                    req, e.Message
                );
            }
        }


    }
}
#else
using System;
using BeatThat.Requests;
using BeatThat.Service;

namespace BeatThat.Entities.Examples
{

    /// <summary>
    /// EntityResolver<T> is the meat of the implementation you need to provide
    /// in most Entity<T> set ups.
    /// 
    /// If you're using NET_4_6 then you can extend EntityResolverService<T>
    /// and the only implemenation you need to provide 
    /// is for ResolveAsync, which takes a key and returns an Entity instance as below.
    /// 
    /// NOTE: the [RegisterService] attribute below is disabled by default
    /// to keep this service from auto registering in your app where you don't want it.
    /// </summary>
#if ENTITY_EXAMPLES_AUTOREGISTER_SERVICES_ENABLED
    [RegisterService(typeof(EntityResolver<DogData>))]
#endif
    public class DogDataResolver : DefaultEntityResolver<DogData>
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

