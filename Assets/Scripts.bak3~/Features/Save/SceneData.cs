using System;
using System.Collections.Generic;

namespace Game.Features.Save
{
    [Serializable]
    public class SceneData
    {
        public List<SaveEntry> entries = new List<SaveEntry>();
    }
}
