using System;
using System.Collections.Generic;

namespace BeatThat.Entities
{
    [Serializable]
	public struct ResolvedMultipleDTO<DataType>
	{
        public IEnumerable<ResolveSucceededDTO<DataType>> entities;

        public static ResolvedMultipleDTO<DataType> Create(IEnumerable<ResolveSucceededDTO<DataType>> pEntities)
        {
            return new ResolvedMultipleDTO<DataType>
            {
                entities = pEntities
            };
        }
	}
}

