using System;


namespace BeatThat.Entities
{
    [Serializable]
	public struct StoreEntityDTO<DataType>
	{
		public string id;
        public int maxAgeSecs;
        public DateTimeOffset timestamp;
        public DataType data;
	}
}

