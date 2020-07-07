using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Packages.unity_aseprite_importer.Editor.Settings
{
    public enum AutoGenerationType
    {
        Sprite,
        //Grid
    }

    [Serializable]
    public class AutoGenerationSettings
    {
        public AutoGenerationType generationType;
        public Vector2 tileSize;
        public Vector2 tileMargin;
        public Vector2 tileOffset;

    }
}
