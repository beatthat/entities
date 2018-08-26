using System;
using BeatThat.DependencyInjection;
using UnityEngine;

namespace BeatThat.Entities.Examples
{
    public class Example_Dogs_Main : DependencyInjectedBehaviour // if you don't inherit di, just call InjectDependencies.On(this) from, say, Start
    {
        [Inject] HasEntities<DogData> dogs;

        private bool didInit;

        void Update()
        {
            if(didInit) {
                return;
            }

            if(Input.GetKeyDown(KeyCode.Space)) {
                this.didInit = true;
                Init();
            }
        }

        private async void Init()
        {
            try {
                var d = await Entity<DogData>.ResolveAsync("SyPvYOhHm", this.dogs);
                Debug.Log("[" + Time.frameCount + "] got a dog: " + JsonUtility.ToJson(d));
            }
            catch(Exception e){
                Debug.LogError(e.Message + "\n" + e.StackTrace);
            }
        }
    }
}

