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

        public Release[] getAllReleasesDateSorted()
        {
            int length = (Releases != null ? Releases.Length:0) + (PreReleases != null ? PreReleases.Length:0);
            Release[] combinedReleases = new Release[length];
            Releases.CopyTo(combinedReleases,0);
            PreReleases.CopyTo(combinedReleases, Releases.Length);
            Array.Sort(combinedReleases);
            return combinedReleases;
        }
    }

    [JsonObject]
    public class Release : IComparable
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

        public int CompareTo(Object obj)
        {
            if (obj.GetType() != typeof(Release)) throw new ArgumentException("must be type Release");
            return ReleaseDate.CompareTo(((Release)obj).ReleaseDate);
        }
    }

}

