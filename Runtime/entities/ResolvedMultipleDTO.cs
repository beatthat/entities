using System;


namespace BeatThat.Entities
{
    [Serializable]
	public struct ResolvedMultipleDTO<DataType>
	{
        public ResolveSucceededDTO<DataType>[] entities;
	}
}

