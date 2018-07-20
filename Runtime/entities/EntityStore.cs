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
        public abstract void GetStoredIds(ICollection<string> ids);
        public abstract void GetAllStoredKeys(ICollection<string> keys);
    }

    public class EntityStore<DataType> : EntityStore, HasEntities<DataType>
	{
        public bool m_debug;

		sealed override protected void BindAll()
        {
            Bind<ResolveSucceededDTO<DataType>>(Entity<DataType>.RESOLVE_SUCCEEDED, this.OnResolveSucceeded);
            Bind <ResolvedMultipleDTO<DataType>>(Entity<DataType>.RESOLVED_MULTIPLE, this.OnResolvedMultiple);
            Bind <string>(Entity<DataType>.RESOLVE_STARTED, this.OnResolveStarted);
            Bind <ResolveFailedDTO>(Entity<DataType>.RESOLVE_FAILED, this.OnResolveFailed);
            BindEntityStore();
		}

        virtual protected void BindEntityStore() {}

        virtual protected void Clear()
        {
            using(var ids = ListPool<string>.Get()) {
                GetAllStoredKeys(ids);
                foreach(var i in ids) {
                    try
                    {
                        Entity<DataType>.WillUnload(i);
                    }
                    catch (Exception e)
                    {
#if UNITY_EDITOR || DEBUG_UNSTRIP
                        Debug.LogError("Error on will unload " + i + ":" + e.Message);
#endif
                    }
                }

                m_idByKey.Clear();
                m_entitiesById.Clear();

                foreach (var i in ids)
                {
                    try
                    {
                        Entity<DataType>.Updated(i);
                    }
                    catch (Exception e)
                    {
#if UNITY_EDITOR || DEBUG_UNSTRIP
                        Debug.LogError("Error on will unload " + i + ":" + e.Message);
#endif
                    }
                }
            }
        }

        override public void GetAllStoredKeys(ICollection<string> ids)
		{
            GetStoredIds(ids);
            ids.AddRange(m_idByKey.Keys);
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

        public bool GetEntity(string key, out Entity<DataType> d)
		{
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

        protected void UpdateEntity(string id, ref Entity<DataType> entity, bool sendUpdate = true)
        {
            m_entitiesById[id] = entity;
            if(sendUpdate) {
                Entity<DataType>.Updated(id);
            }
        }

        virtual protected void OnResolveFailed(ResolveFailedDTO err)
		{
            Entity<DataType> entity;
            GetEntity(err.key, out entity);
            entity.status = entity.status.ResolveFailed(err, DateTime.Now);
            UpdateEntity(err.key, ref entity);
		}

        virtual protected void OnResolveStarted(string key)
		{
            Entity<DataType> entity;
            GetEntity(key, out entity);
            entity.status = entity.status.ResolveStarted(DateTime.Now);
            UpdateEntity(key, ref entity);
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
            entity.data = dto.data;
            entity.status = entity.status.ResolveSucceeded(DateTime.Now);

            UpdateEntity(dto.id, ref entity);

            if(dto.id != dto.key && !string.IsNullOrEmpty(dto.key)) {
                m_idByKey[dto.key] = dto.id;
                m_entitiesById.Remove(dto.key);
                Entity<DataType>.Updated(dto.key);
            }
		}

        protected void Remove(string id, bool sendEvents = true)
        {
            DataType data;
            if(!GetData(id, out data)) {
                return;
            }

            if (sendEvents)
            {
                Entity<DataType>.WillUnload(id);
            }

            using(var key2IdMappings = ListPool<KeyValuePair<string, string>>.Get(m_idByKey)) {
                foreach(var key2Id in key2IdMappings){
                    if(key2Id.Value == id) {
                        m_idByKey.Remove(key2Id.Key);
                    }
                }
            }

            if (!m_entitiesById.Remove(id))
            {
                return;
            }
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


