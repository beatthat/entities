using System;
using BeatThat.Requests;

namespace BeatThat.Entities
{
    [Serializable]
	public struct ResolveFailedDTO
	{
		public string key;
		public object error;
        public int resolveRequestId;

        public string errorMessage {
            get {
                if(this.error == null) {
                    return null;
                }

                var hasErr = this.error as HasError;
                if(hasErr != null) {
                    return hasErr.error;
                }

                return this.error.ToString();
            }
        }

        public static ResolveFailedDTO For(ResolveRequestDTO req, string error)
        {
            return new ResolveFailedDTO
            {
                key = req.key,
                error = error,
                resolveRequestId = req.resolveRequestId
            };
        }
	}
}

