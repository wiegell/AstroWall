using System;
using Newtonsoft.Json;

namespace AstroWall.BusinessLayer.Preferences
{
    [JsonObject]
    public class AddText : PostProcess
    {
        public AddText(bool isEnabled) : base(PostProcessType.AddText, isEnabled)
        {

        }
        public AddText(AddText otherObj, bool isEnabled) : base(otherObj, isEnabled)
        {
            // Meant to create value copy of object, care
            // not to copy future ref.based properties
        }


    }
}

