using System;
using UnityEditor.Animations;
using UnityEngine;


namespace AseImporter {
    public enum AseFileImportType {
        Sprite,
        Tileset,
    }

    public enum TileNameType {
        Index,
        RowCol,
    }

    public enum EmptyTileBehaviour {
        Keep,
    }
    
    public enum AseEditorBindType {
        SpriteRenderer,
        UIImage
    }
    
    public enum AseAnimatorType {
        None,
        AnimatorController,
        AnimatorOverrideController
    }


    [Serializable]
    public class AseFileTextureSettings {
        [SerializeField] public AseFileImportType importType = AseFileImportType.Sprite;
        [SerializeField] public int pixelsPerUnit = 16;
        [SerializeField] public int spriteAlignment = 0;
        [SerializeField] public Vector2 spritePivot = new Vector2(0.5f, 0.5f);
        
        [SerializeField] public AseEditorBindType bindType = AseEditorBindType.SpriteRenderer;
        [SerializeField] public AseAnimatorType animType = AseAnimatorType.None;
        [SerializeField] public AnimatorController baseAnimator = null;

        [SerializeField] public Vector2Int tileSize = new Vector2Int(16, 16);
        [SerializeField] public TileNameType tileNameType = TileNameType.Index;
        [SerializeField] public EmptyTileBehaviour tileEmpty = EmptyTileBehaviour.Keep;
        
        public AseFileTextureSettings Clone() {
            return (AseFileTextureSettings)this.MemberwiseClone();
        }
    }
}