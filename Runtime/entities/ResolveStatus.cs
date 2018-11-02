using System;

namespace BeatThat.Entities
{
    [Serializable]
	public struct ResolveStatus
	{
		public bool hasResolved;
		public bool isResolveInProgress;
		public DateTimeOffset updatedAt;
		public DateTimeOffset timestamp;
		public string resolveError;
        public int maxAgeSecs;
        public int requestIdLastStarted;
        public int requestIdLastCompleted;

        public bool IsExpiredAt(DateTimeOffset time)
        {
            if(!this.hasResolved) {
                return true;
            }

            if(this.maxAgeSecs < 0) {
                return false;
            }

            if(this.maxAgeSecs == 0) {
                return true;
            }

            return this.timestamp.AddSeconds(this.maxAgeSecs) < time;
        }

		public ResolveStatus ResolveFailed(ResolveFailedDTO dto, DateTimeOffset updateTime)
		{
			return new ResolveStatus {
				isResolveInProgress = false,
                updatedAt = updateTime,
                resolveError = dto.errorMessage,
                hasResolved = this.hasResolved,
                timestamp = this.timestamp,
                maxAgeSecs = this.maxAgeSecs,
                requestIdLastStarted = this.requestIdLastStarted,
                requestIdLastCompleted = dto.resolveRequestId
			};
		}

        public ResolveStatus ResolveStarted(int requestId, DateTimeOffset updateTime)
		{
			return new ResolveStatus {
                isResolveInProgress = true,
                updatedAt = updateTime,
                hasResolved = this.hasResolved,
                timestamp = this.timestamp,
                maxAgeSecs = this.maxAgeSecs,
				resolveError = this.resolveError,
                requestIdLastStarted = requestId,
                requestIdLastCompleted = this.requestIdLastCompleted
			};
		}

		public ResolveStatus ResolveSucceeded(
            int requestId, 
            DateTimeOffset timestamp, 
            DateTimeOffset updateTime, 
            int maxAgeSecs
        ) {
			return new ResolveStatus {
				hasResolved = true,
				isResolveInProgress = false,
				updatedAt = updateTime,
                timestamp = timestamp,
				resolveError = this.resolveError,
                maxAgeSecs = maxAgeSecs,
                requestIdLastStarted = this.requestIdLastStarted,
                requestIdLastCompleted = requestId
			};
		}

        public ResolveStatus Resolved(
            DateTimeOffset timestamp,
            DateTimeOffset updateTime,
            int maxAgeSecs
        )
        {
            return new ResolveStatus
            {
                hasResolved = true,
                isResolveInProgress = false,
                updatedAt = updateTime,
                timestamp = timestamp,
                resolveError = this.resolveError,
                maxAgeSecs = maxAgeSecs,
                requestIdLastStarted = this.requestIdLastStarted,
                requestIdLastCompleted = this.requestIdLastCompleted
            };
        }
	}
}
