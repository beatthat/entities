using System.Collections.Generic;

namespace BeatThat.Entities
{
    public interface HasEntityResolveStatus
	{
        /// <summary>
        /// Is the data readily available for the given id (or alias)?
        /// </summary>
		    bool IsResolved(string id);

        /// <summary>
        /// Is there a resolve operation in progress for the given id (or alias)?
        // NOTE: the entity could already be resolved
        // and still also have a resolve in progress, e.g. to refresh stale data.
        /// </summary>
		    bool IsResolveInProgress(string id);

        /// <summary>
        /// Gets the resolve status for an entity id (or alias).
        ///
        /// </summary>
        /// <returns><c>true</c>, TRUE if there is any status stored for the entity, false otherwise.
        /// <param name="id">id or other key (alias) for the entity.</param>
        /// <param name="loadStatus">The result, a ResolveStatus for the entity.</param>
        bool GetResolveStatus(string id, out ResolveStatus loadStatus);



        /// <summary>
        /// Get all stored entity keys.
        /// Keys are distinct from ids because an entity may have various
        /// keys (aliases) that can be used to resolve it, but only a single id.
        ///
        /// The result will include keys for resolved entities (data available)
        /// and also keys for entities with a resolve 'in progress' or a failed resolve.
        /// </summary>
        void GetAllStoredKeys(ICollection<string> keys);

        /// <summary>
        /// Get all stored entity ids.
        /// This will include ids for resolved entities (data available)
        /// and also ids for entities with a resolve 'in progress' or a failed resolve.
        /// </summary>
        void GetStoredIds(ICollection<string> ids);

        /// <summary>
        /// Total count of stored ids (does not include aliases).
        /// Includes count for resolved entities
        /// and also for entities with resolve in progress or resolve failed.
        /// </summary>
        /// <returns>The stored identifier count.</returns>
        int GetStoredIdCount();
	}
}
