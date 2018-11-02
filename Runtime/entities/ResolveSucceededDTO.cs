using System;


namespace BeatThat.Entities
{
    [Serializable]
	public struct ResolveSucceededDTO<DataType>
	{
        public string key;
        public int resolveRequestId;
		public string id;
        public int maxAgeSecs;
        public DateTimeOffset timestamp;
        public DataType data;
	}
}

