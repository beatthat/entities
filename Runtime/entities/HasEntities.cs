using System;
using System.Collections.Generic;
using UnityEngine;

namespace BeatThat.Entities
{

    public delegate bool EntityFilter<DataType>(ref Entity<DataType> entity);

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
        void GetResolved(
            ICollection<DataType> resolved, 
            EntityFilter<DataType> filter = null
        );

        /// <summary>
        /// Gets all entity data for entities that have resolved /
        /// objects stored in memory. Result shape is id=>data
        /// </summary>
        void GetResolved(
            IDictionary<string, DataType> resolved, 
            EntityFilter<DataType> filter = null
        );
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

#if UNITY_EDITOR || DEBUG_UNSTRIP
            if (Entity<DataType>.DEBUG)
            {
                Debug.Log("[" + Time.frameCount + "][" + typeof(Entity<DataType>).Name 
                     + "] key=" + key 
                     + " - found with timestamp=" + entity.status.timestamp 
                     + ", and maxAgeSecs=" +  entity.status.maxAgeSecs 
                     + ", isExpired=" + entity.status.IsExpiredAt(DateTimeOffset.Now)
                     + ", isResolveInProgress=" + entity.status.isResolveInProgress);
            }
#endif

            var status = entity.status;
            if(status.IsExpiredAt(DateTimeOffset.Now) && !status.isResolveInProgress) {
                Entity<DataType>.RequestResolve(key);
            }

            return true;
        }
    }
}


