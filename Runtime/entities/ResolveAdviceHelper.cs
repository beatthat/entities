using System;
using BeatThat.Notifications;
using UnityEngine;


namespace BeatThat.Entities
{

    public static class ResolveAdviceHelper
	{
		public const float DEFAULT_RESOLVE_TIMEOUT_SECS = 5f;
		public const float DEFAULT_RETRY_MIN_INTERVAL_SECS = 2f;

        public static ResolveAdvice AdviseOnAndSendErrorIfCoolingDown(
            string id,
            HasEntityResolveStatus hasData,
            string errorNotification,
            float resolveTimeoutSecs = DEFAULT_RESOLVE_TIMEOUT_SECS,
            float retryMinIntervalSecs = DEFAULT_RETRY_MIN_INTERVAL_SECS,
            bool debug = false)
        {
            var advice = AdviseOn(id, hasData, resolveTimeoutSecs, retryMinIntervalSecs, debug);
            if(advice == ResolveAdvice.CANCEL_ERROR_COOL_DOWN) {
                NotificationBus.Send(errorNotification, new ResolveFailedDTO
                {
                    key = id,
                    error = "resolve has failed for id and is in cooldown period"
                });
            }
            return advice;
        }

		public static ResolveAdvice AdviseOn(
			string id, 
			HasEntityResolveStatus hasData, 
			float resolveTimeoutSecs = DEFAULT_RESOLVE_TIMEOUT_SECS,
			float retryMinIntervalSecs = DEFAULT_RETRY_MIN_INTERVAL_SECS, 
			//float ttlSecs = DEFAULT_TTL_SECS, 
			bool debug = false)
		{
			ResolveStatus data;
			hasData.GetResolveStatus (id, out data);

            var now = DateTime.Now;

            if(data.hasResolved && !data.IsExpiredAt(now)) {
				#if UNITY_EDITOR || DEBUG_UNSTRIP
				if(debug) {
					Debug.Log("[" + Time.frameCount + "] skipping load attempt for id '" + id + "' (already loaded and not expired)");
				}
				#endif
				return ResolveAdvice.CANCEL_RESOLVED_AND_UNEXPIRED;
			}

			if (data.isResolveInProgress && data.updatedAt.AddSeconds(resolveTimeoutSecs) > DateTime.Now) {
				#if UNITY_EDITOR || DEBUG_UNSTRIP
				if(debug) {
					Debug.Log("[" + Time.frameCount + "] skipping resolve attempt for id '" + id 
						+ "' (resolve in progress started at " + data.updatedAt + ")");
				}
				#endif
				return ResolveAdvice.CANCEL_IN_PROGRESS;
			}

			if (!string.IsNullOrEmpty(data.resolveError) && data.timestamp.AddSeconds(retryMinIntervalSecs) > DateTime.Now) {
				#if UNITY_EDITOR || DEBUG_UNSTRIP
				if(debug) {
					Debug.Log("[" + Time.frameCount + "] skipping resolve attempt for id '" + id 
						+ "' (resolve in progress started at " + data.updatedAt + ")");
				}
				#endif
				return ResolveAdvice.CANCEL_ERROR_COOL_DOWN;
			}

			return ResolveAdvice.PROCEED;
		}
	}
}

