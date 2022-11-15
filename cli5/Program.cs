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

            //UpdateManifest man = new UpdateManifest()
            //{
            //    Releases = new Release[2] { rel, rel }
            //};

            //JSONhelpers.SerializeNow(man, "./manifest.json");

            UpdateManifest manifest = readFromFile();

            regPreRelease(manifest);
        }

        static void regPreRelease(UpdateManifest manifest)
        {
            string gitStatus = runCommand("git status");
            bool cleanTree = gitStatus.Contains("nothing to commit, working tree clean");
            bool isOnMaster = gitStatus.Contains("On branch master");

            if (cleanTree && isOnMaster)
            {
                // Update tag
                string tag = runCommand("git describe").Replace("\n", "");
                Console.WriteLine("Git clean on tag: " + tag);
                string newTagShort = runCommand("git describe --tags --abbrev=0 | awk -F. '{OFS=\\\".\\\"; $NF+=1; print $0}'").Replace("\n", "").Replace("\r", "");
                newTagShort += "-alpha";
                Console.WriteLine("Incrementing to: " + newTagShort);
                runCommand($"git tag -a \\\"{newTagShort}\\\" -m \\\"version {newTagShort}\\\"");
                Console.WriteLine("Making empty commit");
                runCommand($"git commit --allow-empty -m \\\"Auto commit for tag update to: {newTagShort}\\\"");
                string newTagLong = runCommand("git describe --tags --abbrev=0").Replace("\n", "").Replace("\r", "");

                // Update plist
                Console.WriteLine("Ready to update info.plist with build version: " + newTagShort);
                string prebuildOutput = runCommand("node ./scripts/pre-build", "./AstroWall");
                Console.WriteLine("Update success");

                // Build binaries
                Console.WriteLine("Building binaries...");
                runCommand("msbuild ./AstroWall.sln /property:Configuration=Release");
                Console.WriteLine("Binaries built");

                // Create pkgs
                Console.WriteLine("Creating pkg");
                string shret = runCommand("sh ./scripts/pack.sh", "./AstroWall");
                Console.WriteLine(shret);
                Console.WriteLine("PKGs created");

                // Update manifest
                Release rel = new Release()
                {
                    version = newTagLong,
                    ReleaseDate = DateTime.Now,
                    DirectPKGurl = $"https://github.com/wiegell/AstroWall/releases/download/{newTagShort}/Astro.pkg",
                    Description = "Lorem Ipsum",
                    isPreRelease = true
                };
                if (manifest.PreReleases == null) manifest.PreReleases = new Release[0];
                manifest.PreReleases = (Release[])manifest.PreReleases.Append(rel).ToArray();
                writeToFile(manifest);

                // Commit manifest
                runCommand($"git add . && git commit -m \\\"Updated manifest with release {newTagLong}\\\"");

                // Push new tag to gh
                Console.WriteLine("Pushing new tag to origin");
                runCommand("git push origin --tags");

                // Upload prerelease
                Console.WriteLine("Uploading prerelease to gh");
                string uploadres = runCommand($"gh release create -p --generate-notes {newTagShort} ./AstroWall/bin/Package/Astro.pkg");
                Console.WriteLine("Upload res:\n" + uploadres);

                // Push manifest
                string gitPushReturn = runCommand("git push origin master");
                Console.WriteLine("Success: " + gitPushReturn);
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
                return ("error: " + error);

            string output = proc.StandardOutput.ReadToEnd();

            proc.WaitForExit();

            return output;
        }

        static UpdateManifest readFromFile(string path = "./docs/assets/manifest.json")
        {
            return JSONhelpers.DeSerializeNow<UpdateManifest>(path);
        }

        static void writeToFile(UpdateManifest manifest, string path = "./docs/assets/manifest.json")
        {
            JSONhelpers.SerializeNow(manifest, path);
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

