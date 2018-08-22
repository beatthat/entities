using System;
using System.Collections.Generic;

namespace BeatThat.Entities
{
    [Serializable]
	public struct ResolveMultipleResultDTO<DataType>
	{
        public IEnumerable<ResolveResultDTO<DataType>> entities;
	}
}

