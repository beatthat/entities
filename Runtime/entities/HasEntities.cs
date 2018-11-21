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

    [Flags]
    public enum RequestResolvePolicyFlags
    {
        NEVER = 0,
        IF_MISSING = 1,
        IF_EXPIRED = 1 << 1,
        IF_HAS_ERROR = 1 << 2
    }

    public static class HasEntitiesExt
    {
        /// <summary>
        /// A convenience extension/wrapper around HasEntities&lt;DataType>&gt::GetData
        /// that also issues a resolve request under specifiable conditions,
        /// e.g. if the entity is missing or if it is expired
        /// </summary>
        /// <returns><c>true</c>, if data was gotten, <c>false</c> otherwise.</returns>
        /// <param name="entities">Entities.</param>
        /// <param name="key">Key.</param>
        /// <param name="d">D.</param>
        /// <param name="requestResolvePolicy">Request resolve policy.</param>
        /// <typeparam name="DataType">The 1st type parameter.</typeparam>
        public static bool GetData<DataType>(
            this HasEntities<DataType> entities,
            string key,
            out DataType d,
            RequestResolvePolicyFlags requestResolvePolicy
        )
        {
            Entity<DataType> entity;
            if (!entities.GetEntity(key, out entity) || !entity.status.hasResolved)
            {
                d = default(DataType);

                // By default DO NOT try to resolve an entity that is in error status
                if (entity.status.HasError() && !requestResolvePolicy.HasFlag(RequestResolvePolicyFlags.IF_HAS_ERROR)) {
                    return false;
                }

                if(requestResolvePolicy.HasFlag(RequestResolvePolicyFlags.IF_MISSING))
                {
                    Entity<DataType>.RequestResolve(new ResolveRequestDTO
                    {
                        key = key
                    });
                }
                return false;
            }

            d = entity.data;

#if UNITY_EDITOR || DEBUG_UNSTRIP
            if (Entity<DataType>.DEBUG)
            {
                Debug.Log("[" + Time.frameCount + "][" + typeof(Entity<DataType>).Name
                     + "] key=" + key
                     + " - found with timestamp=" + entity.status.timestamp
                     + ", and maxAgeSecs=" + entity.status.maxAgeSecs
                     + ", isExpired=" + entity.status.IsExpiredAt(DateTimeOffset.Now)
                     + ", isResolveInProgress=" + entity.status.isResolveInProgress);
            }
#endif
            if (entity.status.HasError() && !requestResolvePolicy.HasFlag(RequestResolvePolicyFlags.IF_HAS_ERROR))
            {
                // By default DO NOT try to resolve an entity that is in error status
                return true;
            }

            if (requestResolvePolicy.HasFlag(RequestResolvePolicyFlags.IF_EXPIRED))
            {
                var status = entity.status;
                if (status.IsExpiredAt(DateTimeOffset.Now) && !status.isResolveInProgress)
                {
                    Entity<DataType>.RequestResolve(key);
                }
            }

            return true;
        }

#if !NET_4_6
        /// polyfill for HasFlag method in .NET 4+
        public static bool HasFlag(this RequestResolvePolicyFlags val, RequestResolvePolicyFlags flag) 
        {
            return ((int)val & (int)flag) == (int)flag;
        }
#endif
    }
}


