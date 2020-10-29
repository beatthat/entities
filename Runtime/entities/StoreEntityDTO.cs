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

        public static StoreEntityDTO<DataType> Create(string id, DataType data)
        {
            return new StoreEntityDTO<DataType>
            {
                id = id,
                data = data,
                maxAgeSecs = -1,
                timestamp = DateTimeOffset.Now
            };
        }
    }
}

