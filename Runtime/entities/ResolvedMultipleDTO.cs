using System;
using System.Collections.Generic;

namespace BeatThat.Entities
{
    [Serializable]
	public struct ResolvedMultipleDTO<DataType>
	{
        public IEnumerable<ResolveSucceededDTO<DataType>> entities;
	}
}

