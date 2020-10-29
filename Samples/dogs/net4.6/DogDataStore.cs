#if NET_4_6
using System;
using BeatThat.Requests;
using BeatThat.Service;
using System.Threading.Tasks;

namespace BeatThat.Entities.Examples
{
    /// <summary>
    /// When NET4.6 is available override the async/await version of Resolve
    ///
    /// NOTE: the [RegisterEntityStore] attribute below is disabled by default
    /// to keep this service from auto registering in your app where you don't want it.
    /// </summary>
#if ENTITY_EXAMPLES_AUTOREGISTER_SERVICES_ENABLED 
    [RegisterEntityService] // basically same as RegisterService, but also ensures that interface HasEntities<DataType> gets registered
#endif
    public class DogDataStore : EntityService<DogData> 
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
#endif
