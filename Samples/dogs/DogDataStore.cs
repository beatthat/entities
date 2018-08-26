namespace BeatThat.Entities.Examples
{
    [RegisterEntityStore] // basically same as RegisterService, but also ensures that interface HasEntities<DataType> gets registered
    public class DogDataStore : EntityStore<DogData> { }
}

