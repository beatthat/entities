Use entities to manage collections of data that share a common type and where each item has a unique id. A common use for entities is to manage a client store of items retrieved via a REST API from a database table or collection.

## Install

From your unity project folder:

    npm init
    npm install TEMPLATE --save
    echo Assets/packages >> .gitignore
    echo Assets/packages.meta >> .gitignore

The package and all its dependencies will be installed under Assets/Plugins/packages.

In case it helps, a quick video of the above: https://youtu.be/Uss_yOiLNw8

## USAGE

#### Setting Up Entity Services

You need 4 basic components to set up an entities collection.

First, you need a ```DataType```

```csharp
// Your data type can be a struct or a class.
//
// I like to use structs because it clarifies
// and enforces that entity items shouldn't
// be edited directly, i.e. if the DogData entity below
// were a class and you retrieved one from the store // and edited its properties, you will have in effect
// changed the entity in the store.
public struct DogData
{
  public string id;
  public string imageUrl;
}
```

Next you need an ```EntityStore``` that will hold your entities.
```csharp
// The [RegisterEntittStore] attribute makes the store injectable with dependency injection.
[RegisterEntityStore]
public class DogStore : EntityStore<DogData> {}
```

...a ```Command``` that manages the resolve process
```csharp
[RegisterCommand]
public class ResolveDogCmd : ResolveEntityCmd<DogData> {}
```

...and finally an ```EntityResolver``` whose job is to resolve an item of entity data given an id (or alias). This is the main class where you need to provide some implementation

```csharp
using BeatThat.Requests;

[RegisterService]
public class DogResolver : EntityResolver<DogData>
{
  public Request<ResolveResultDTO<DogData>> Resolve(string loadKey, Action<Request<ResolveResultDTO<GoalData>>> callback)
  {
    var promise = new Promise((resolve, reject) => {
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

#### References

The architecture of entities borrows a lot from [redux](https://redux.js.org/)--there is an EntityStore containing items which have been stored and updated (reduced) in response to notifications. Understanding redux and its motivations will help you understand this implementation of entities or any of the many similar frameworks in the wild. I highly recommend going through Dan Abramov's [redux course on egghead](https://egghead.io/courses/getting-started-with-redux) if you have the time.
