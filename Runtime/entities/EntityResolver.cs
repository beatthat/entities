using System;
using BeatThat.Requests;

namespace BeatThat.Entities
{
    /// <summary>
    /// an API capable of loading data for individual entities by an entity 'load key'.
    /// The load key is the entity's id, but in many cases an alias should also be accepted
    /// </summary>
    public interface EntityResolver<DataType>
	{
        Request<ResolveResultDTO<DataType>> GetOne(string loadKey, Action<Request<ResolveResultDTO<DataType>>> callback);
	}
}


