using System;

namespace BeatThat.Entities
{
    [Serializable]
	public struct ResolveResultDTO<DataType>
	{
        public string status;
        public string message;
        public string key;
		public string id;
        public DataType data;
        public float maxAgeSecs;
        public DateTime timestamp;
	}
}

