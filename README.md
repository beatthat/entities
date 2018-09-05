<a name="readme"></a>Need to manage a collection of data items, like say, downloadable songs for a music app? Entities make it easy.

You can use entities to manage any collection whose items share a common data type and where each item has a unique id. A common use for entities is to manage a client store of items retrieved via a REST API from a database table or collection.

## Install

From your unity project folder:

    npm init --force
    npm install beathat/entities --save

The package and all its dependencies will be installed under Assets/Plugins/packages.

In case it helps, a quick video of the above: https://youtu.be/Uss_yOiLNw8

## USAGE

#### Setting Up a new Entity type

In the example below, we'll set up a DogData entity, where each item has a url for an image of a dog.

You need 4 basic components to set up a new Entity type.

First, you need a ```DataType```.

```csharp
// Your data type can be a struct or a class.
//
// I like to use structs because it clarifies
// and enforces that entity items shouldn't
// be edited directly, i.e. if the DogData entity below
// were a class and you retrieved one from the store
// and edited its properties, you will have in effect
// changed the entity in the store.
public struct DogData
{
  public string id;
  public string imageUrl;
}
```

...next you need an ```EntityStore``` that will hold your entities.
```csharp
// The [RegisterEntittStore] attribute makes the store injectable with dependency injection.
[RegisterEntityStore]
public class DogStore : EntityStore<DogData> {}
```

...a ```Command``` that resolves and stores entities in response to notifications

```csharp
[RegisterCommand]
public class ResolveDogCmd : ResolveEntityCmd<DogData> {}
```

...and finally an ```EntityResolver``` whose job is to resolve an item of entity data given an id (or alias). Whereas the ```DogStore``` and ```ResolveDogCmd``` classes above are templates, your ```EntityResolver``` is the main class where you need to provide some implementation.

```csharp
using System.Threading.Tasks;
using BeatThat.Requests;
using BeatThat.Service;

[RegisterService(typeof(EntityResolver<DogData>))]
public class DogDataResolver : EntityResolverService<DogData>
{
    public override async Task<ResolveResultDTO<DogData>> ResolveAsync(string key)
    {
      // NOTE: this public dog api (api.thedogapi.co.uk) seems to be intermittently unavailable

        var path = string.Format("https://api.thedogapi.co.uk/v2/dog.php?id={0}", loadKey);

        try {

            // using BeatThat.Requests.WebRequest here, just an easy way
            // to make a one-line HTTP request and get a typed result
            var data = await new WebRequest<DogData>(path).ExecuteAsync();

            return new ResolveResultDTO<DogData>
            {
                key = key,
                id = data.id,
                status = ResolveStatusCode.OK,
                timestamp = DateTimeOffset.Now,
                data = data
            };
        }
        catch(Exception e) {
            return new ResolveResultDTO<DogData>
            {
                key = key,
                id = key,
                status = ResolveStatusCode.ERROR,
                message = e.Message,
                timestamp = DateTimeOffset.Now
            };
        }
    }
}
```

...if you're not using .NET 4.6 (if async/await is unsupported in your project), then use this example instead:

```csharp
using BeatThat.Requests;
using BeatThat.Service;

[RegisterService]
public class DogResolver : EntityResolver<DogData>
{
  public Request<ResolveResultDTO<DogData>> Resolve(string loadKey, Action<Request<ResolveResultDTO<GoalData>>> callback)
  {
    var promise = new Promise((resolve, reject) => {
      // NOTE: this public dog api (api.thedogapi.co.uk) seems to be intermittently unavailable

      //https://api.thedogapi.co.uk/v2/dog.php?id=5ta5p7JdHEL
      var path = string.Format("https://api.thedogapi.co.uk/v2/dog.php?id={0}", loadKey);

      new WebRequest<DogData>(path).Execute(result => {
        if(result.hasError) {
          reject(result);
          return;
        }
        resolve(result.item);
      });
    });

    promise.Execute(callback);
    return promise;
  }
}
```


#### Using Entities

Below are a set of short examples of accessing Entity data. All the examples are using dependency injection to get access to an EntityStore service and that requires that this piece of init happened somewhere as close as possible to app launch (before the other examples would run):

```csharp
using BeatThat.Service;
public class MyAppStartup : MonoBehaviour
{
  void Start()
  {
    Services.Init();
  }
}
```

Here's how you get data for an Entity:

```csharp
using BeatThat.Service;
using BeatThat.DependencyInjection;
using BeatThat.Entities;
public class Foo : DependencyInjectedBehaviour
{
  [Inject] HasEntities<DogData> dogs;

  public void DoSomethingWithDog(string dogId)
  {
    DogData data;
    if(this.dogs.GetData(dogId, out data)) {
      // use data, but only if it is already resolved
    }
  }
}
```

...here's how you get an Entity that might not already be resolved

```csharp
using BeatThat.Service;
using BeatThat.DependencyInjection;
using BeatThat.Entities;
public class Foo : DependencyInjectedBehaviour
{
  [Inject] HasEntities<DogData> dogs;

  public async void DoSomethingWithDog(string dogId)
  {
    DogData data;
    if(!this.dogs.GetData(dogId, out data)) {
      data = await Entity<DogData>.ResolveAsync(dogId, this.dogs); // could wrap in try/catch to handle web errors etc.
    }

    // do something with dog data
  }
}
```

...here's how you get an Entity that might not already be resolved if async/await is not supported (.NET < 4.6)

```csharp
using BeatThat.Service;
using BeatThat.DependencyInjection;
using BeatThat.Entities;
public class Foo : DependencyInjectedBehaviour
{
  [Inject] HasEntities<DogData> dogs;

  public void DoSomethingWithDog(string dogId)
  {
    DogData data;
    if(this.dogs.GetData(dogId, out data)) {
      // usually have another function to do something w data
      // to avoid duplicate code

      return;
    }

    Entity<DogData>.Resolve(dogId, this.dogs, result => {
      if(result.hasError) {
        Debug.LogError(result.error);
        return;
      }

      var data = result.item;
      // call your DoSomethingWithDogData function
    });
  }
}
```

Here's how you check whether an Entity's data is available for use.

```csharp
//HasEntities<DogData> dogs;
var canUseData = this.dogs.IsResolved(dogId);
```

...or if you want to know if an entity is in progress loading/resolving

```csharp
//HasEntities<DogData> dogs;
var isResolving = this.dogs.IsResolveInProgress(dogId);
```

...or if you want to inspect an Entity's full resolve status, including possible errors

```csharp
//HasEntities<DogData> dogs;
ResolveStatus status;
if(this.dogs.GetStatus(id, out status)) {
  // If GetStatus returned FALSE above,
  // the entity is not resolved and there
  // has been no attempt to resolve it

  var canUse = status.hasResolved;
  var isResolvingNow = status.isResolveInProgress;
  if(status.hasError) {
    var error = status.error;
  }

  var shouldRefresh = status.IsExpiredAt(DateTimeOffset.Now);
}
```

...or if you want to inspect both the status and the data (which may or may not be available)

```csharp
//HasEntities<DogData> dogs;
Entity<DogData> dog;
if(this.dogs.GetEntity(id, out dog)) {
  DogData data = dog.data;
  ResolveStatus status = dog.status;
}
```

...here's how you request that an Entity resolve without waiting for it

```csharp
Entity<DogData>.RequestResolve("some-dog-id");
```

...and here's how you listen for updates to entity resolve status

```csharp
// using BeatThat.Notifications
NotificationBus.Add(Entity<DogData>.Updated, (id) => {
  // do something with the id that was updated
})
```

#### Using Entities with BeatThat helper classes

There are base classes in BeatThat that simplify working with notifications and dependency injection. You don't need to use these classes but they handle a lot of boilerplate for you.

Below are some examples, with and with out the base classes.

...a global singleton service that listens for Entity updates.

```csharp
using BeatThat.Service;
using BeatThat.Bindings;
using BeatThat.DependencyInjection;
using BeatThat.Entities;
[RegisterService]
public class MyDogService : BindingService
{
  [Inject] HasEntities<DogData> dogs;

  override protected void BindAll()
  {
    Bind(Entity<DogData>.UPDATE, this.OnUpdated);
  }

  private void OnUpdated(string id)
  {
    Entity<DogData> dog;
    if(!this.dogs.GetEntity(id)) {
      return;
    }
    // do something with this dog
  }
}
```

...you can have the same behavior as above with no special base classes doing something like this:

```csharp
using BeatThat.Service;
using BeatThat.Notifications;
using BeatThat.Entities;

// assume you made this a singleton by
// your preferred method
public class MyDogService : MonoBehaviour
{
  private HasEntities<DogData> dogs;

  void OnEnable()
  {
    this.dogs = Services.Require<HasEntities<DogData>>();
    NotificationBus.Add(Entity<DogData>.UPDATE, this.OnUpdated);
  }

  void OnDisable()
  {
    NotificationBus.Remove(Entity<DogData>.UPDATE, this.OnUpdated);
  }

  private void OnUpdated(string id)
  {
    Entity<DogData> dog;
    if(!this.dogs.GetEntity(id)) {
      return;
    }
    // do something with this dog
  }
}
```

#### References

The architecture of entities borrows a lot from [redux](https://redux.js.org/)--there is an EntityStore containing items which have been stored and updated (reduced) in response to notifications. Understanding redux and its motivations will help you understand this implementation of entities or any of the many similar frameworks in the wild. I highly recommend going through Dan Abramov's [redux course on egghead](https://egghead.io/courses/getting-started-with-redux) if you have the time.
