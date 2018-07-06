using System;

namespace BeatThat.EntityStores
{
    [Serializable]
	public struct ResolveResultDTO<DataType>
	{
        public string status;
        public string message;
        public string key;
		public string id;
        public DataType data;
	}
}

