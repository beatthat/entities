using BeatThat.CollectionsExt;
using System;
using System.Collections.Generic;
using BeatThat.Bindings;
using UnityEngine;
using BeatThat.Pools;

namespace BeatThat.Entities
{
    /// <summary>
    /// Non generic base class mainly to enable things like a default Unity editor
    /// </summary>
    public abstract class EntityStore : BindingService, HasEntityResolveStatus
    {
        public abstract bool GetResolveStatus(string id, out ResolveStatus status);
        public abstract bool IsResolved(string id);
        public abstract bool IsResolveInProgress(string id);
        public abstract void GetStoredIds(ICollection<string> ids);
        public abstract int GetStoredIdCount();
        public abstract void GetAllStoredKeys(ICollection<string> keys);
        public abstract bool GetDataAsObject(string key, out object data);
    }

    public class EntityStore<DataType> : EntityStore, HasEntities<DataType>
	{
        public bool m_debug;
        virtual protected bool debug { get { return m_debug; } }

		sealed override protected void BindAll()
        {
            Bind<ResolveSucceededDTO<DataType>>(Entity<DataType>.RESOLVE_SUCCEEDED, this.OnResolveSucceeded);
            Bind<ResolvedMultipleDTO<DataType>>(Entity<DataType>.RESOLVED_MULTIPLE, this.OnResolvedMultiple);
            Bind<ResolveRequestDTO>(Entity<DataType>.RESOLVE_STARTED, this.OnResolveStarted);
            Bind<ResolveFailedDTO>(Entity<DataType>.RESOLVE_FAILED, this.OnResolveFailed);
            Bind<StoreEntityDTO<DataType>>(Entity<DataType>.STORE, this.OnStore);
            Bind<StoreMultipleDTO<DataType>>(Entity<DataType>.STORE_MULTIPLE, this.OnStoreMultiple);
            Bind<bool>(Entity<DataType>.UNLOAD_ALL_REQUESTED, this.Clear);
            BindEntityStore();
		}

        virtual protected void BindEntityStore() {}

        virtual protected void Clear(bool sendNotifications = true)
        {
            if(sendNotifications) {
                Entity<DataType>.WillUnloadAll();
            }
            m_idByKey.Clear();
            m_entitiesById.Clear();
            if(sendNotifications) {
                Entity<DataType>.DidUnloadAll();
            }
        }

        public void GetResolved(
            ICollection<DataType> resolved, 
            EntityFilter<DataType> filter = null
        )
        {
            foreach(var e in m_entitiesById.Values)
            {
                if(!e.status.hasResolved) {
                    continue;
                }
                if(filter == null) {
                    resolved.Add(e.data);
                    continue;
                }
                var eRef = e;
                if(!filter(ref eRef)) {
                    continue;
                }

                resolved.Add(e.data);
            }
        }

        public void GetResolved(
            IDictionary<string, DataType> resolved,
            EntityFilter<DataType> filter = null
        )
        {
            foreach (var kv in m_entitiesById)
            {
                var e = kv.Value;
                if (!e.status.hasResolved)
                {
                    continue;
                }
                if(filter != null && !filter(ref e)) {
                    continue;
                }

                resolved[kv.Key] = e.data;
            }
        }

        override public void GetAllStoredKeys(ICollection<string> ids)
		{
            GetStoredIds(ids);
            ids.AddRange(m_idByKey.Keys);
		}

        override public int GetStoredIdCount()
        {
            return m_entitiesById.Count;
        }

        override public void GetStoredIds(ICollection<string> ids)
        {
            ids.AddRange(m_entitiesById.Keys);
        }

		override public bool IsResolved(string id)
		{
            ResolveStatus status;
            return GetResolveStatus(id, out status) && status.hasResolved;
		}

        override public bool IsResolveInProgress(string id)
		{
            ResolveStatus status;
            return GetResolveStatus(id, out status) && status.isResolveInProgress;
		}


        override public bool GetResolveStatus (string id, out ResolveStatus status)
		{
            Entity<DataType> entity;
            if (!GetEntity(id, out entity)) {
                status = default(ResolveStatus);
				return false;
			}

            status = entity.status;
			return true;
		}

        public bool GetData(string key, out DataType data)
		{
            Entity<DataType> entity;
            if (!GetEntity(key, out entity) || !entity.status.hasResolved)
            {
                data = default(DataType);
                return false;
            }
            data = entity.data;
            return true;
		}

        override public bool GetDataAsObject(string key, out object data)
        {
            DataType d;
            if(GetData(key, out d)) {
                data = d;
                return true;
            }
            else {
                data = null;
                return false;
            }
        }

        public bool GetEntity(string key, out Entity<DataType> d)
		{
#if UNITY_EDITOR || DEBUG_UNSTRIP
            if(this.debug) {
                Debug.Log("[" + Time.frameCount + "] " + GetType() + "::GetEntity " + key);
            }
#endif

            if (string.IsNullOrEmpty(key))
            {
#if UNITY_EDITOR || DEBUG_UNSTRIP
                Debug.LogWarning("[" + Time.frameCount + "] null or empty key");
#endif
                d = default(Entity<DataType>);
                return false;
            }

            var id = IdForKey(key);

            return m_entitiesById.TryGetValue (id, out d);
		}

        public void GetAllEntities(IEnumerable<string> keys, ICollection<Entity<DataType>> entities)
        {
            Entity<DataType> cur;
            foreach(var k in keys) {
                if(string.IsNullOrEmpty(k)) {
#if UNITY_EDITOR || DEBUG_UNSTRIP
                    Debug.LogWarning("attempt to get entity with null or empty key");
#endif
                    continue;
                }

                var id = IdForKey(k);
                if (!m_entitiesById.TryGetValue(id, out cur))
                {
                    cur = new Entity<DataType>
                    {
                        id = id
                    };
                }
                entities.Add(cur);
            }
        }

        protected void UpdateEntity(string id, ref Entity<DataType> entity, bool sendUpdate = true)
        {
            if(string.IsNullOrEmpty(id)) {
#if UNITY_EDITOR || DEBUG_UNSTRIP
                Debug.LogWarning("attempt to update entity with null or empty key");
#endif

                return;
            }
            m_entitiesById[id] = entity;
            if(sendUpdate) {
                Entity<DataType>.Updated(id);
            }
        }

        virtual protected void OnResolveFailed(ResolveFailedDTO err)
		{
            if(string.IsNullOrEmpty(err.key)) {
#if UNITY_EDITOR || DEBUG_UNSTRIP
                Debug.LogWarning("received resolved-failed notification for a null or empty key (message-" + err.errorMessage + ")");
#endif
                return;
            }
            Entity<DataType> entity;
            GetEntity(err.key, out entity);
            entity.status = entity.status.ResolveFailed(err, DateTimeOffset.Now);
            UpdateEntity(err.key, ref entity);
		}

        virtual protected void OnResolveStarted(ResolveRequestDTO dto)
		{
            Entity<DataType> entity;
            GetEntity(dto.key, out entity);

            entity.id = (!string.IsNullOrEmpty(entity.id)) 
                ? entity.id : dto.key;

            entity.status = entity.status.ResolveStarted(
                dto.resolveRequestId, DateTimeOffset.Now
            );

            UpdateEntity(dto.key, ref entity);
		}

        virtual protected void OnResolvedMultiple(ResolvedMultipleDTO<DataType> dto)
        {
            foreach(var entity in dto.entities) {
                try
                {
                    OnResolveSucceeded(entity);
                }
                catch (Exception e)
                {
#if UNITY_EDITOR || DEBUG_UNSTRIP
                    Debug.LogError("Error on process entity: " + e.Message);
#endif
                }
            }
        }

        virtual protected void OnResolveSucceeded(ResolveSucceededDTO<DataType> dto)
		{
            if (string.IsNullOrEmpty(dto.id))
            {
#if UNITY_EDITOR || DEBUG_UNSTRIP
                Debug.LogWarning("[" + Time.frameCount + "] null or empty id in resolve for "
                                 + typeof(DataType).Name + ": " + JsonUtility.ToJson(dto));
#endif
                return;
            }

            Entity<DataType> entity;
            GetEntity(dto.id, out entity);
            entity.id = dto.id;
            entity.data = dto.data;

            entity.status = entity.status.ResolveSucceeded(
                dto.resolveRequestId, dto.timestamp, 
                DateTimeOffset.Now, dto.maxAgeSecs
            );

            UpdateEntity(dto.id, ref entity);

            if(dto.id != dto.key && !string.IsNullOrEmpty(dto.key)) {
                m_idByKey[dto.key] = dto.id;
                m_entitiesById.Remove(dto.key);
                Entity<DataType>.Updated(dto.key);
            }
		}

        virtual protected void OnStore(StoreEntityDTO<DataType> dto)
        {
            if (string.IsNullOrEmpty(dto.id))
            {
#if UNITY_EDITOR || DEBUG_UNSTRIP
                Debug.LogWarning("[" + Time.frameCount + "] null or empty id in store for "
                                 + typeof(DataType).Name + ": " + JsonUtility.ToJson(dto));
#endif
                return;
            }

            Entity<DataType> entity;
            GetEntity(dto.id, out entity);
            entity.id = dto.id;
            entity.data = dto.data;

            entity.status = entity.status.Resolved(
                dto.timestamp, DateTimeOffset.Now, dto.maxAgeSecs
            );

            UpdateEntity(dto.id, ref entity);
        }

        virtual protected void OnStoreMultiple(StoreMultipleDTO<DataType> dto)
        {
            foreach (var entity in dto.entities)
            {
                try
                {
                    OnStore(entity);
                }
                catch (Exception e)
                {
#if UNITY_EDITOR || DEBUG_UNSTRIP
                    Debug.LogError("Error on process entity: " + e.Message);
#endif
                }
            }
        }

        virtual protected bool Remove(string id, bool sendEvents = true)
        {
            DataType data;
            if(!GetData(id, out data)) {
                return false;
            }

            if (sendEvents)
            {
                Entity<DataType>.WillRemove(id);
            }

            using(var key2IdMappings = ListPool<KeyValuePair<string, string>>.Get(m_idByKey)) {
                foreach(var key2Id in key2IdMappings){
                    if(key2Id.Value == id) {
                        m_idByKey.Remove(key2Id.Key);
                    }
                }
            }

            m_entitiesById.Remove(id);
            if (sendEvents)
            {
                Entity<DataType>.DidRemove(id);
            }

            return true;
        }

        private string IdForKey(string key)
        {
            string id;
            return m_idByKey.TryGetValue(key, out id) ? id : key;
        }

        public int count { get { return m_entitiesById.Count; } }

        protected void GetAll(ICollection<Entity<DataType>> result)
        {
            result.AddRange(m_entitiesById.Values);
        }

        private Dictionary<string, string> m_idByKey = new Dictionary<string, string>();
        private Dictionary<string, Entity<DataType>> m_entitiesById = new Dictionary<string, Entity<DataType>> ();
	}

}
