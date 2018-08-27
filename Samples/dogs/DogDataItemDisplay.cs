using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace BeatThat.Entities.Examples
{

    public class DogDataItemDisplay : MonoBehaviour
    // It would make sense to have this component be a BeatThat.Controllers.Controller,
    // but since this example is about Entity,
    // trying to avoid introducing another possibly unfamiliar base class
    {
        public UnityEvent<DogData> onDisplayDog { get { return m_onDisplayDog;  } }
        private UnityEvent<DogData> m_onDisplayDog = new DogEvent();
        class DogEvent : UnityEvent<DogData> { }

        public Button m_button;
        public Text m_text;

        public void Init(string id, HasEntities<DogData> dogs)
        {
            this.id = id;
            this.dogs = dogs;

            m_button = (m_button != null) ? m_button : GetComponentInChildren<Button>();
            m_text = (m_text != null) ? m_text : GetComponentInChildren<Text>();

            m_button.onClick.AddListener(this.OnClick);

            UpdateDisplay();
        }


        // this code is a little longer than it needs to be
        // because includes two ways to resolve an entity:
        // 1) with async/await (for NET_4_6 or above)
        // 2) old-style async with a callback function
#if NET_4_6
        private async void UpdateDisplay(bool showDog = false)
        {
            DogData dogData;
            // if the data is already stored, no need to resolve it again...
            if (this.dogs.GetData(this.id, out dogData))
            {
                UpdateDisplay(dogData, showDog);
                return;
            }

            m_text.text = "Loading " + this.id + "...";

            try {
                // for a single resolve, async/await code below
                // is pretty similar to using a callback.
                // But if we're doing a series of resolves, a for loop, etc.
                // it's much cleaner and more readable 
                // to use async/await
                UpdateDisplay(await Entity<DogData>.ResolveAsync(this.id, this.dogs), showDog);
            }
            catch(Exception e) {
                Debug.LogError("Failed to load data for id " + this.id + ": " + e.Message);
            }
        }
#else
        private void UpdateDisplay(bool showDog = false)
        {
            DogData dogData;
            // if the data is already stored, no need to resolve it again...
            if (this.dogs.GetData(this.id, out dogData))
            {
                UpdateDisplay(dogData, showDog);
                return;
            }

            m_text.text = "Loading " + this.id + "...";

            Entity<DogData>.Resolve(this.id, this.dogs, result =>
            {
                if(result.hasError) {
                    Debug.LogError("Failed to resolve dog for id " + id + ": " + result.error);
                    return;
                }
                UpdateDisplay(result.item, showDog);
            });
        }
#endif


        private void UpdateDisplay(DogData dogData, bool showDog = false)
        {
            m_text.text = "Show " + (!string.IsNullOrEmpty(dogData.name) ? dogData.name : this.id);
            if(showDog) {
                m_onDisplayDog.Invoke(dogData);
            }
        }

        private void OnClick()
        {
            UpdateDisplay(true);
        }

        public string id { get; private set; }

        private HasEntities<DogData> dogs;
    }
}

