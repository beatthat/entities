#if NET_4_6
using System;
using System.Threading.Tasks;
using BeatThat.Requests;
using BeatThat.Service;
using UnityEngine;

namespace BeatThat.Entities.Examples
{
    [RegisterService(typeof(EntityResolver<DogData>))]
    public class DogDataResolver : EntityResolverService<DogData>
    {
        public override async Task<ResolveResultDTO<DogData>> ResolveAsync(string key)
        {
            var path = DogAPI.GetDogUrl(key);

            Debug.LogError("path=" + path);

            try {
                
                // using BeatThat.Requests.WebRequest here, just an easy way 
                // to make a one-line HTTP request and get a typed result
                var data = await new WebRequest<DogData>(path).ExecuteAsync();

                Debug.LogError("will return data: " + JsonUtility.ToJson(data));
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

