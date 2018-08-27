using System;
using UnityEngine;

namespace BeatThat.Entities.Examples
{
    public static class DogAPI
    {
        //private static readonly string API_URL = "https://api.thedogapi.co.uk/v2/dog.php";

        public static string GetDogUrl(string id)
        {
            /* Using local file jsons instead of urls for the real dog api 
             * that the data came from.
             * 
             * Public APIs generally require an API KEY 
             * or otherwise aren't highly available.
             * 
             * Using files means the example will always work
             * as long as the directory for the json files doesn't move.
             */
            return string.Format("file://{0}/Samples/packages/beatthat/entities/dogs/api-fake/{1}.json", 
                                 Application.dataPath, id);
        }

        /// <summary>
        /// Ids of the dog data items the fake API can return (locally stored as json files)
        /// </summary>
        public static readonly string[] IDS = {
            "HkNkxlqEX",
            "rJFJVxc4m",
            "SJcCuu2SX",
            "SyPvYOhHm"
        };
    }
}

