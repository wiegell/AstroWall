using System;
using Newtonsoft.Json;

namespace AstroWall.BusinessLayer.Preferences
{
    public enum PostProcessType
    {
        AddText,
        ScaleAndCrop
    }

    [JsonObject]
    public abstract class PostProcess
    {
        [JsonProperty]
        public readonly bool isEnabled;
        [JsonProperty]
        public readonly PostProcessType name;

        public PostProcess(PostProcessType name, bool isEnabled)
        {
            this.name = name;
            this.isEnabled = isEnabled;
        }
        public PostProcess(PostProcess otherObj, PostProcessType name)
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





