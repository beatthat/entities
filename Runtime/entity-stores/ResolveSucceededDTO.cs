using System;


namespace BeatThat.EntityStores
{
    [Serializable]
	public struct ResolveSucceededDTO<DataType>
	{
        public string key;
		public string id;
        public DataType data;
	}
}

