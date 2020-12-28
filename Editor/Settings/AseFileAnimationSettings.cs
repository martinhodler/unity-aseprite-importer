using System.Collections.Generic;
using UnityEngine;

namespace AsepriteImporter.Settings
{
    [System.Serializable]
    public class AseFileAnimationSettings
    {

        public AseFileAnimationSettings()
        {
        }

        public AseFileAnimationSettings(string name)
        {
            animationName = name;
        }

        [SerializeField] public string animationName;
        [SerializeField] public bool loopTime = true;
        [SerializeField] public string about;
        [SerializeField] public Sprite[] sprites;
        [SerializeField] public int[] frameNumbers;

        public override string ToString()
        {
            return animationName;
        }

        public bool HasInvalidSprites
        {
            get
            {
                foreach (Sprite sprite in sprites)
                {
                    if (sprite == null)
                        return true;
                }

                return false;
            }
        }
    }
}