using System;
using BeatThat.Requests;

namespace BeatThat.Entities
{
    /// <summary>
    /// an API capable of resolving an Entity given a 'key'.
    /// The key may be the entity's id, but an alias or a full uri can be accepted
    /// </summary>
    public interface EntityResolver<DataType>
	{
        Request<ResolveResultDTO<DataType>> Resolve(string key, Action<Request<ResolveResultDTO<DataType>>> callback);
	}
}


