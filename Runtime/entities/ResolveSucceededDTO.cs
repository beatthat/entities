using System;


namespace BeatThat.Entities
{
    [Serializable]
	public struct ResolveSucceededDTO<DataType>
	{
        public string key;
		public string id;
        public float maxAgeSecs;
        public DateTime timestamp;
        public DataType data;
	}
}

