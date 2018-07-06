namespace BeatThat.Entities
{
    public interface HasEntityData<DataType>
	{
        bool GetData(string id, out DataType data);
	}
}


