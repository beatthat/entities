using System.Collections.Generic;

namespace BeatThat.Entities
{
    public interface HasEntityResolveStatus
	{
		bool IsResolved(string id);

        bool GetResolveStatus(string id, out ResolveStatus loadStatus);

        void GetStoredKeys(ICollection<string> keys);
	}
}


