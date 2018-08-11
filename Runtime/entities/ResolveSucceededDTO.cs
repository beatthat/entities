using System;


namespace BeatThat.Entities
{
    [Serializable]
	public struct ResolveSucceededDTO<DataType>
	{
        public string key;
		public string id;
        public int maxAgeSecs;
        public DateTime timestamp;
        public DataType data;
	}
}

