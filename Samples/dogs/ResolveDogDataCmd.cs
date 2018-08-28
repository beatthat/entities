using BeatThat.Commands;

namespace BeatThat.Entities.Examples
{
    /// <summary>
    /// NOTE: the [RegisterCommand] attribute below is disabled by default
    /// to keep this service from auto registering in your app where you don't want it.
    /// </summary>
#if ENTITY_EXAMPLES_AUTOREGISTER_SERVICES_ENABLED 
    [RegisterCommand]
#endif
    public class ResolveDogDataCmd : ResolveEntityCmd<DogData> { }
}

