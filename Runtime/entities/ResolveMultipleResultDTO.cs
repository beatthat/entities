using System;
using System.Collections.Generic;

namespace BeatThat.Entities
{
    [Serializable]
	public struct ResolveMultipleResultDTO<DataType>
	{
        public IEnumerable<ResolveResultDTO<DataType>> entities;

        public static ResolveMultipleResultDTO<DataType> Create(IEnumerable<ResolveResultDTO<DataType>> pEntities)
        {
            return new ResolveMultipleResultDTO<DataType>
            {
                entities = pEntities
            };
        }
    }
}

