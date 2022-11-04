using System;
using Newtonsoft.Json;

namespace UpdateLibrary
{
    [JsonObject]
    public class UpdateManifest
    {
        [JsonProperty]
        private string warning;
        [JsonProperty]
        public Release[] Releases;
        [JsonProperty]
        public Release[] PreReleases;

    }

    [JsonObject]
    public class Release
    {
        [JsonProperty]
        public string version;
        [JsonProperty]
        public DateTime ReleaseDate;
        [JsonProperty]
        public string DirectPKGurl;
        [JsonProperty]
        public string Description;
        [JsonProperty]
        public bool isPreRelease;
    }


}

