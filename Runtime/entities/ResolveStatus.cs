using System;

namespace BeatThat.Entities
{
    [Serializable]
	public struct ResolveStatus
	{
		public bool hasResolved;
		public bool isResolveInProgress;
		public DateTime resolveStartedAt;
		public DateTime updatedAt;
		public string resolveError;

		public ResolveStatus ResolveFailed(ResolveFailedDTO dto, DateTime updateTime)
		{
			return new ResolveStatus {
				hasResolved = this.hasResolved,
				isResolveInProgress = false,
				resolveStartedAt = this.resolveStartedAt,
				updatedAt = updateTime,
                resolveError = dto.errorMessage
			};
		}

		public ResolveStatus ResolveStarted(DateTime updateTime)
		{
			return new ResolveStatus {
				hasResolved = this.hasResolved,
				isResolveInProgress = true,
				resolveStartedAt = updateTime,
				updatedAt = this.updatedAt,
				resolveError = this.resolveError
			};
		}

		public ResolveStatus ResolveSucceeded(DateTime updateTime)
		{
			return new ResolveStatus {
				hasResolved = true,
				isResolveInProgress = false,
				resolveStartedAt = updateTime,
				updatedAt = updateTime,
				resolveError = this.resolveError
			};
		}
	}
}


