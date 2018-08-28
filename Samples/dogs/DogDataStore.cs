namespace BeatThat.Entities.Examples
{
#if ENTITY_EXAMPLES_AUTOREGISTER_SERVICES_ENABLED 
    [RegisterEntityStore] // basically same as RegisterService, but also ensures that interface HasEntities<DataType> gets registered
#endif
    public class DogDataStore : EntityStore<DogData> { }
}

