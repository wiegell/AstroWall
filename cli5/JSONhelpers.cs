using System;
using System.IO;
using Newtonsoft.Json;

namespace cli5
{
    public class JSONhelpers
    {
        public JSONhelpers()
        {
        }

        public static void SerializeNow(Object c, string path)
        {
            string jsonString = JsonConvert.SerializeObject(c);
            File.WriteAllText(path, jsonString);
        }

        public static T DeSerializeNow<T>(string path)
        {

            string jsonString = String.Join("", File.ReadAllLines(path));
            //dynamic json = JsonConvert.DeserializeObject(jsonString);
            T result = JsonConvert.DeserializeObject<T>(jsonString);
            return result;
        }
    }
}

