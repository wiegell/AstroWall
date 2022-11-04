using System;
using System.IO;
using UpdateLibrary;
using System.Linq;

namespace cli5
{
    class Program
    {

        static void Main(string[] args)
        {
            //Release rel = new Release()
            //{
            //    version = "0.0.1-alpha-riotenrti",
            //    ReleaseDate = DateTime.Now,
            //    DirectPKGurl = "https://github.com/wiegell/AstroWall/releases/download/v0.0.2-alpha/Astro.pkg",
            //    Description = "First on update manifest",
            //    isPreRelease = true
            //};

            //UpdateManifest man = new UpdateManifest()
            //{
            //    Releases = new Release[2] { rel, rel }
            //};

            //JSONhelpers.SerializeNow(man, "./manifest.json");

            UpdateManifest manifest = readFromFile("./docs/assets/manifest.json");

            listPreReleases(manifest);
        }

        //static regPreRelease()
        //{

        //}

        static UpdateManifest readFromFile(string path)
        {
            return JSONhelpers.DeSerializeNow<UpdateManifest>(path);
        }

        static void listReleases(UpdateManifest man)
        {
            Console.WriteLine("Releases: ");
            foreach (Release rel in (man.Releases.Where(rel => !rel.isPreRelease)))
            {
                Console.WriteLine("Date: {0}, Version: {1}", rel.ReleaseDate.ToString("dd/MM-yy"), rel.version);
            }
        }

        static void listPreReleases(UpdateManifest man)
        {
            Console.WriteLine("PreReleases: ");
            foreach (Release rel in (man.Releases.Where(rel => rel.isPreRelease)))
            {
                Console.WriteLine("Date: {0}, Version: {1}", rel.ReleaseDate.ToString("dd/MM-yy"), rel.version);
            }
        }
    }
}

