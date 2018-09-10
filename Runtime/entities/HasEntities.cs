using System;
using System.Collections.Generic;

namespace BeatThat.Entities
{
    
    public interface HasEntities<DataType> : HasEntityData<DataType>, HasEntityResolveStatus
	{
        bool GetEntity(string key, out Entity<DataType> d);

        /// <summary>
        /// Gets the entities for the provided list of keys.
        /// The result includes a result for every requested key, even
        /// </summary>
        /// <param name="keys">Keys.</param>
        /// <param name="entities">Entities.</param>
        void GetAllEntities(IEnumerable<string> keys, ICollection<Entity<DataType>> entities);

        /// <summary>
        /// Gets all entity data for entities that have resolved /
        /// objects stored in memory
        /// </summary>
        void GetResolved(ICollection<DataType> resolved);

        /// <summary>
        /// Gets all entity data for entities that have resolved /
        /// objects stored in memory. Result shape is id=>data
        /// </summary>
        void GetResolved(IDictionary<string, DataType> resolved);
	}

    public static class HasEntitiesExt
    {
        public static bool GetDataWithBackgroundRefresh<DataType>(this HasEntities<DataType> entities, string key, out DataType d)
        {
            Entity<DataType> entity;
            if(!entities.GetEntity(key, out entity) || !entity.status.hasResolved) {
                d = default(DataType);
                return false;
            }

            d = entity.data;

            var status = entity.status;
            if(status.IsExpiredAt(DateTimeOffset.Now) && !status.isResolveInProgress) {
                Entity<DataType>.RequestResolve(key);
            }

            return true;
        }
    }
}


