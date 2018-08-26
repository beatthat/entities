using System;
using UnityEngine;

namespace BeatThat.Entities.Examples
{
    public static class DogAPI
    {
        //private static readonly string API_URL = "https://api.thedogapi.co.uk/v2/dog.php";

        public static string GetDogUrl(string id)
        {
            return string.Format("file://{0}/Samples/packages/beatthat/entities/dogs/api-fake/{1}.json", 
                                 Application.dataPath, id);
        }

        /// <summary>
        /// This dog api has a result object with status and then the data
        /// as a child obj, which is pretty common.
        /// </summary>
        [Serializable]
        public struct Result
        {
            public DogData data;
            public string error;
        }
    }
}

