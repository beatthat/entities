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
        public int resolveRequestId;
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

        public static ResolveResultDTO<DataType> ResolveSucceeded(
            ResolveRequestDTO req,
            string id, 
            DataType data, 
            int maxAgeSecs = 0,
            DateTimeOffset? timestamp = null
        )
        {
            return new ResolveResultDTO<DataType>
            {
                status = ResolveStatusCode.OK,
                message = "ok",
                id = id,
                key = req.key,
                data = data,
                resolveRequestId = req.resolveRequestId,
                timestamp = timestamp.HasValue? timestamp.Value: DateTimeOffset.Now,
                maxAgeSecs = maxAgeSecs
            };
        }


        public static ResolveResultDTO<DataType> ResolveError(
            ResolveRequestDTO req,
            string error
        )
        {
            return new ResolveResultDTO<DataType>
            {
                status = ResolveStatusCode.ERROR,
                message = error,
                id = req.key,
                key = req.key,
                resolveRequestId = req.resolveRequestId,
                timestamp = DateTimeOffset.Now
            };
        }

        public static ResolveResultDTO<DataType> ResolveError(
            ResolveRequestDTO req,
            Exception error, 
            string message = null
        )
        {
            return new ResolveResultDTO<DataType>
            {
                status = ResolveStatusCode.ERROR,
                message = !string.IsNullOrEmpty(message)? message: error.Message,
                id = req.key,
                key = req.key,
                resolveRequestId = req.resolveRequestId,
                timestamp = DateTimeOffset.Now
            };
        }

        public static ResolveResultDTO<DataType> ResolveNotFound(
            ResolveRequestDTO req
        )
        {
            return new ResolveResultDTO<DataType>
            {
                status = ResolveStatusCode.NOT_FOUND,
                message = "not found",
                id = req.key,
                key = req.key,
                resolveRequestId = req.resolveRequestId,
                timestamp = DateTimeOffset.Now
            };
        }
	}
}

