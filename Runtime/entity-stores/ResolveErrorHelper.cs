using BeatThat.Notifications;
using BeatThat.Requests;
using UnityEngine;

namespace BeatThat.EntityStores
{

    public static class ResolveErrorHelper
	{
		public static bool HandledError(string id, HasError r, string errorNotification, bool debug = false)
		{
			if (string.IsNullOrEmpty (r.error)) {
				return false;
			}


			#if UNITY_EDITOR || DEBUG_UNSTRIP
			Debug.LogWarning("[" + Time.frameCount + "] error loading '" + id + "': " + r.error);
			#endif

			NotificationBus.Send(errorNotification, new ResolveFailedDTO {
                key = id,
				error = r
			});
			return true;
		}
	}
}

