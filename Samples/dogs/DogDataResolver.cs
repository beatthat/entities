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
    /// </summary>
    [RegisterService(typeof(EntityResolver<DogData>))]
    public class DogDataResolver : EntityResolverService<DogData>
    {
        public override async Task<ResolveResultDTO<DogData>> ResolveAsync(string key)
        {
            var path = DogAPI.GetDogUrl(key);

            try {
                
                // using BeatThat.Requests.WebRequest here, just an easy way 
                // to make a one-line HTTP request and get a typed result
                var data = await new WebRequest<DogData>(path).ExecuteAsync();

                return new ResolveResultDTO<DogData>
                {
                    key = key,
                    id = data.id,
                    status = ResolveStatusCode.OK,
                    timestamp = DateTimeOffset.Now,
                    data = data
                };
            }
            catch(Exception e) {
                return new ResolveResultDTO<DogData>
                {
                    key = key,
                    id = key,
                    status = ResolveStatusCode.ERROR,
                    message = e.Message,
                    timestamp = DateTimeOffset.Now
                };
            }
        }


    }
}
#else
// TODO: non-async impl
#endif

