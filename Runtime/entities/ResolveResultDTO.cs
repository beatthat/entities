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
        public int maxAgeSecs;
        public DateTimeOffset timestamp;

        public bool GetData(out DataType data)
        {
            if(this.status != ResolveStatusCode.OK) {
                data = default(DataType);
                return false;
            }
            data = this.data;
            return true;
        }

        public static ResolveResultDTO<DataType> ResolveError(string key, string error)
        {
            return new ResolveResultDTO<DataType>
            {
                status = ResolveStatusCode.ERROR,
                message = error,
                id = key,
                key = key,
                timestamp = DateTimeOffset.Now
            };
        }

        public static ResolveResultDTO<DataType> ResolveError(string key, Exception error, string message = null)
        {
            return new ResolveResultDTO<DataType>
            {
                status = ResolveStatusCode.ERROR,
                message = !string.IsNullOrEmpty(message)? message: error.Message,
                id = key,
                key = key,
                timestamp = DateTimeOffset.Now
            };
        }

        public static ResolveResultDTO<DataType> ResolveNotFound(string key)
        {
            return new ResolveResultDTO<DataType>
            {
                status = ResolveStatusCode.NOT_FOUND,
                message = "not found",
                id = key,
                key = key,
                timestamp = DateTimeOffset.Now
            };
        }
	}
}

