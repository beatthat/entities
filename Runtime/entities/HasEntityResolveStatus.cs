using System.Collections.Generic;

namespace BeatThat.Entities
{
    public interface HasEntityResolveStatus
	{
		bool IsResolved(string id);

        bool GetResolveStatus(string id, out ResolveStatus loadStatus);

        void GetAllStoredKeys(ICollection<string> keys);

        void GetStoredIds(ICollection<string> ids);

        int GetStoredIdCount();
	}
}


