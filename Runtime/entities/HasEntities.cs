namespace BeatThat.Entities
{
    public interface HasEntities<DataType> : HasEntityData<DataType>, HasEntityResolveStatus
	{
        bool GetEntity(string key, out Entity<DataType> d);
	}
}


