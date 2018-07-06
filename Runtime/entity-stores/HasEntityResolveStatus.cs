using System.Collections.Generic;

namespace BeatThat.EntityStores
{
    public interface HasEntityResolveStatus
	{
		bool IsResolved(string id);

        bool GetResolveStatus(string id, out ResolveStatus loadStatus);

        void GetResolvedIds(ICollection<string> ids);
	}
}


