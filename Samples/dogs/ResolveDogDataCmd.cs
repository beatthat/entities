using BeatThat.Commands;

namespace BeatThat.Entities.Examples
{
#if ENTITY_EXAMPLES_AUTOREGISTER_SERVICES_ENABLED 
    [RegisterCommand]
#endif
    public class ResolveDogDataCmd : ResolveEntityCmd<DogData> { }
}

