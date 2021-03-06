﻿using System.Collections.Generic;
using BeatThat.DependencyInjection;
using BeatThat.Service;

namespace BeatThat.Entities.Examples
{
    /// <summary>
    /// User can tap a button for a dog and the image for that dog will display.
    /// Each dog button is a DogDataDisplayItem and the meat of the example
    /// for how to use Entity::Resolve etc. is there.
    /// </summary>
    public class Example_Entities_Dogs : DependencyInjectedBehaviour    // DependencyInjectedBehaviour makes sure InjectDependencies.On(this) is called.
                                                                        // If you want dependency injection without the base class, 
                                                                        // you can call InjectDependencies.On(this) from Start or similar.

    {
        [Inject] HasEntities<DogData> dogs; // this will be an injected reference 
                                            // to our global DogDataStore

        public WebImage m_image;
        public DogDataItemDisplay m_dogItemProto;
        public DogDataItemDisplay[] m_dogItems;

#if !ENTITY_EXAMPLES_AUTOREGISTER_SERVICES_ENABLED
        override protected void DidAwake()
        {
            // The ServiceLoader code is only necessary because attribute-based
            // registration is disabled for the example.
            // In your own code, you'd generally just use the attributes/autowiring.
            // Autowiring is disabled by default for this example, because
            // when it's enabled those services will register in your real app
            // where you don't want or need them.
            ServiceLoader.onAfterSetServiceRegistratonsStatic = (serviceLoader) =>
            {
                serviceLoader.Register<DogDataStore>();
                serviceLoader.Register<EntityResolver<DogData>, DogDataStore>();
                serviceLoader.Register<HasEntities<DogData>, DogDataStore>();
                serviceLoader.Register<HasEntityData<DogData>, DogDataStore>();
            };
            base.DidAwake();
        }
#endif

        override protected void DidStart() 
        {
            Services.Init(() =>
            {
                // Since we're using dependency-injection 
                // and this component is really like startup for the whole app,
                // safer to init services and then proceed when done.
                InitDogItems();
            });
        }

        private void InitDogItems()
        {
            var items = new List<DogDataItemDisplay>();
            items.Add(m_dogItemProto);
            for (var i = 1; i < DogAPI.IDS.Length; i++) {
                var cur = Instantiate(m_dogItemProto);
                cur.transform.SetParent(m_dogItemProto.transform.parent);
                cur.transform.SetSiblingIndex(items[i - 1].transform.GetSiblingIndex() + 1);
                items.Add(cur);
            }
            m_dogItems = items.ToArray();
            for (var i = 0; i < DogAPI.IDS.Length; i++)
            {
                m_dogItems[i].Init(DogAPI.IDS[i], this.dogs);
                m_dogItems[i].onDisplayDog.AddListener(d =>
                {
                    m_image.LoadAndDisplayImage(d.url);
                });
            }

        }
    }
}

