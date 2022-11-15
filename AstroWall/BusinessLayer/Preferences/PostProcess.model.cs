using System;
using Newtonsoft.Json;

namespace AstroWall.BusinessLayer.Preferences
{

    [JsonObject]
    public abstract class PostProcess
    {
        [JsonProperty]
        public readonly bool isEnabled;
        [JsonProperty]
        public readonly string name;

        public PostProcess(string name, bool isEnabled)
        {
            this.name = name;
            this.isEnabled = isEnabled;
        }
        public PostProcess(PostProcess otherObj, string name)
        {
            this.name = name;
            this.isEnabled = otherObj.isEnabled;
        }
        public PostProcess(PostProcess otherObj, bool isEnabled)
        {
            this.name = otherObj.name;
            this.isEnabled = isEnabled;
        }
        public PostProcess(PostProcess otherObj)
        {
            this.name = otherObj.name;
            this.isEnabled = otherObj.isEnabled;
        }
    }
}





