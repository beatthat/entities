using BeatThat.CollectionsExt;
using System;
using System.Collections.Generic;
using BeatThat.Bindings;
using UnityEngine;

namespace BeatThat.EntityStores
{
    /// <summary>
    /// Non generic base class mainly to enable things like a default Unity editor
    /// </summary>
    public abstract class EntityStore : BindingService, HasEntityResolveStatus
    {
        public abstract bool GetResolveStatus(string id, out ResolveStatus status);
        public abstract bool IsResolved(string id);
        public abstract void GetResolvedIds(ICollection<string> ids);
    }

    public class EntityStore<DataType> : EntityStore, HasEntities<DataType>
	{
        public bool m_debug;

		override protected void BindAll()
		{
            Bind <ResolveSucceededDTO<DataType>>(Entity<DataType>.RESOLVE_SUCCEEDED, this.OnResolveSucceeded);
            Bind <string>(Entity<DataType>.RESOLVE_STARTED, this.OnResolveStarted);
            Bind <ResolveFailedDTO>(Entity<DataType>.RESOLVE_FAILED, this.OnResolveFailed);
		}

        override public void GetResolvedIds(ICollection<string> ids)
		{
            ids.AddRange (m_storedDataById.Keys);
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

            return m_storedDataById.TryGetValue (id, out d);
		}

        private void OnResolveFailed(ResolveFailedDTO err)
		{
            Entity<DataType> entity;
            GetEntity(err.key, out entity);
            entity.status = entity.status.ResolveFailed(err, DateTime.Now);
            m_storedDataById[err.key] = entity;
            Entity<DataType>.Updated(err.key);
		}

        private void OnResolveStarted(string key)
		{
            Entity<DataType> entity;
            GetEntity(key, out entity);
            entity.status = entity.status.ResolveStarted(DateTime.Now);
            m_storedDataById[key] = entity;
            Entity<DataType>.Updated(key);
		}

        private void OnResolveSucceeded(ResolveSucceededDTO<DataType> dto)
		{
            if (string.IsNullOrEmpty(dto.id))
            {
#if UNITY_EDITOR || DEBUG_UNSTRIP
                Debug.LogWarning("[" + Time.frameCount + "] null or empty id");
#endif
                return;
            }

            Entity<DataType> entity;
            GetEntity(dto.id, out entity);
            entity.data = dto.data;
            entity.status = entity.status.ResolveSucceeded(DateTime.Now);
            m_storedDataById[dto.id] = entity;
            Entity<DataType>.Updated(dto.id);

            if(dto.id != dto.key && !string.IsNullOrEmpty(dto.key)) {
                m_idByAlias[dto.key] = dto.id;
                m_storedDataById.Remove(dto.key);
                Entity<DataType>.Updated(dto.key);
            }
		}

        private string IdForKey(string key)
        {
            string id;
            return m_idByAlias.TryGetValue(key, out id) ? id : key;
        }

        private Dictionary<string, string> m_idByAlias = new Dictionary<string, string>();
        private Dictionary<string, Entity<DataType>> m_storedDataById = new Dictionary<string, Entity<DataType>> ();
	}

}


