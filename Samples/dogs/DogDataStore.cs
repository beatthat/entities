namespace BeatThat.Entities.Examples
{
    /// <summary>
    /// NOTE: the [RegisterEntityStore] attribute below is disabled by default
    /// to keep this service from auto registering in your app where you don't want it.
    /// </summary>
#if ENTITY_EXAMPLES_AUTOREGISTER_SERVICES_ENABLED 
    [RegisterEntityStore] // basically same as RegisterService, but also ensures that interface HasEntities<DataType> gets registered
#endif
    public class DogDataStore : EntityStore<DogData> { }
}

