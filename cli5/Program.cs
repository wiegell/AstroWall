using System;
using System.IO;
using UpdateLibrary;
using System.Linq;
using System.Diagnostics;

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

            regPreRelease();
        }

        static void regPreRelease()
        {
            string gitStatus = runCommand("git status");
            bool cleanTree = gitStatus.Contains("nothing to commit, working tree clean");
            bool isOnMaster = gitStatus.Contains("On branch master");

            if (true || cleanTree && isOnMaster)
            {
                string tag = runCommand("git describe");
                Console.WriteLine("Git clean on tag: " + tag);
                string newTag = runCommand("git describe --tags --abbrev=0 | awk -F. '{OFS=\\\".\\\"; $NF+=1; print $0}'");
                Console.WriteLine("Incrementing to: " + newTag);
                Console.WriteLine("Git clean, ready to update info.plist with build version: " + tag);
                string prebuildOutput = runCommand("node ./scripts/pre-build", "./AstroWall");
                Console.WriteLine("Update success");
                Console.WriteLine("Building binaries...");
                runCommand("msbuild ./AstroWall.sln /property:Configuration=Release");
                Console.WriteLine("Binaries built");
                Console.WriteLine("Creating pkg");
                string shret = runCommand("sh ./scripts/pack.sh", "./AstroWall");
                Console.WriteLine(shret);
                Console.WriteLine("PKGs created");
            }
            else
            {
                throw new Exception("Git not clean");
            }
        }

        static string runCommand(string command, string workdir = null)
        {
            ProcessStartInfo psi = new ProcessStartInfo();
            psi.FileName = "sh";
            psi.Arguments = "-c \"" + command + "\"";
            psi.UseShellExecute = false;
            psi.RedirectStandardOutput = true;
            psi.RedirectStandardError = true;
            if (workdir != null) psi.WorkingDirectory = workdir;

            Process proc = new Process
            {
                StartInfo = psi
            };


            proc.Start();

            string error = proc.StandardError.ReadToEnd();

            if (!string.IsNullOrEmpty(error))
                throw new Exception("error: " + error);

            string output = proc.StandardOutput.ReadToEnd();

            proc.WaitForExit();

            return output;
        }

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

