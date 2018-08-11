using System;
using UnityEngine;

namespace BeatThat.Entities
{
    [Serializable]
	public struct ResolveStatus
	{
		public bool hasResolved;
		public bool isResolveInProgress;
		public DateTime updatedAt;
		public DateTime timestamp;
		public string resolveError;
        public int maxAgeSecs;

        public bool IsExpiredAt(DateTime time)
        {
            if(!this.hasResolved) {
                return true;
            }

            if(this.maxAgeSecs < 0f) {
                return false;
            }

            if(Mathf.Approximately(this.maxAgeSecs, 0f)) {
                return true;
            }

            return timestamp.AddSeconds(this.maxAgeSecs) < time;
        }
        
		public ResolveStatus ResolveFailed(ResolveFailedDTO dto, DateTime updateTime)
		{
			return new ResolveStatus {
				isResolveInProgress = false,
                updatedAt = updateTime,
                resolveError = dto.errorMessage,
                hasResolved = this.hasResolved,
                timestamp = this.timestamp,
                maxAgeSecs = this.maxAgeSecs,
			};
		}

		public ResolveStatus ResolveStarted(DateTime updateTime)
		{
			return new ResolveStatus {
                isResolveInProgress = true,
                updatedAt = updateTime,
                hasResolved = this.hasResolved,
                timestamp = this.timestamp,
                maxAgeSecs = this.maxAgeSecs,
				resolveError = this.resolveError
			};
		}

		public ResolveStatus ResolveSucceeded(DateTime timestamp, DateTime updateTime, int maxAgeSecs)
		{
			return new ResolveStatus {
				hasResolved = true,
				isResolveInProgress = false,
				updatedAt = updateTime,
                timestamp = timestamp,
				resolveError = this.resolveError,
                maxAgeSecs = maxAgeSecs
			};
		}
	}
}


