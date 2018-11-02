using System;
using System.Collections.Generic;

namespace BeatThat.Entities
{
    [Serializable]
	public struct StoreMultipleDTO<DataType>
	{
        public IEnumerable<StoreEntityDTO<DataType>> entities;

        public static StoreMultipleDTO<DataType> Create(IEnumerable<StoreEntityDTO<DataType>> pEntities)
        {
            return new StoreMultipleDTO<DataType>
            {
                entities = pEntities
            };
        }
	}
}

