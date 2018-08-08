using System;

namespace BeatThat.Entities
{
    
    public interface HasEntities<DataType> : HasEntityData<DataType>, HasEntityResolveStatus
	{
        bool GetEntity(string key, out Entity<DataType> d);
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
            if(status.IsExpiredAt(DateTime.Now) && !status.isResolveInProgress) {
                Entity<DataType>.RequestResolve(key);
            }

            return true;
        }
    }
}


