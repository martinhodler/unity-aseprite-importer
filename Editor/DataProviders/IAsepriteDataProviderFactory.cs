
using System.Collections;
using System.Collections.Generic;
using UnityEditor.U2D.Sprites;
using UnityEngine;


namespace AsepriteImporter
{
    public class IAsepriteDataProviderFactory : ISpriteDataProviderFactory<AseFileImporter>
    {
        public ISpriteEditorDataProvider CreateDataProvider(AseFileImporter obj)
        {
            return obj;
        }
    }
}
