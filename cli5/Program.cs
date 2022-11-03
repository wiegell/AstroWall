using System;
using UpdateLibrary;

namespace cli5
{
    class Program
    {
        static void Main(string[] args)
        {
            Release rel = new Release()
            {
                version = "0.0.1-alpha-riotenrti",
                ReleaseDate = DateTime.Now,
                DirectPKGurl = "https://github.com/wiegell/AstroWall/releases/download/v0.0.2-alpha/Astro.pkg",
                Description = "First on update manifest",
                isPreRelease = true
            };

            UpdateManifest man = new UpdateManifest()
            {
                Releases = new Release[2] { rel, rel }
            };

            JSONhelpers.SerializeNow(man, "./manifest.json");
        }
    }
}

