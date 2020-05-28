using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor.Experimental.AssetImporters;
using System.IO;
using Aseprite;
using UnityEditor;
using Aseprite.Chunks;
using System.Text;

namespace AsepriteImporter
{
    public enum AseFileImportType
    {
        Sprite,
        Tileset,
        LayerToSprite
    }

    public enum AseEditorBindType
    {
        SpriteRenderer,
        UIImage
    }

    [ScriptedImporter(1, new []{ "ase", "aseprite" })]
    public class AseFileImporter : ScriptedImporter
    {
        [SerializeField] public AseFileTextureSettings textureSettings = new AseFileTextureSettings();
        [SerializeField] public AseFileAnimationSettings[] animationSettings;
        [SerializeField] public Texture2D atlas;
        [SerializeField] public AseFileImportType importType;
        [SerializeField] public AseEditorBindType bindType;

        public override void OnImportAsset(AssetImportContext ctx)
        {
            name = GetFileName(ctx.assetPath);

            AseFile aseFile = ReadAseFile(ctx.assetPath);
            int frameCount = aseFile.Header.Frames;

            SpriteAtlasBuilder atlasBuilder = new SpriteAtlasBuilder(textureSettings, aseFile.Header.Width, aseFile.Header.Height);

            Texture2D[] frames = null;
            if (importType != AseFileImportType.LayerToSprite)
                frames = aseFile.GetFrames();
            else
                frames = aseFile.GetLayersAsFrames();

            SpriteImportData[] spriteImportData = new SpriteImportData[0];

            //if (textureSettings.transparentMask)
            //{
            //    atlas = atlasBuilder.GenerateAtlas(frames, out spriteImportData, textureSettings.transparentColor, false);
            //}
            //else
            //{
            //    atlas = atlasBuilder.GenerateAtlas(frames, out spriteImportData, false);

            //}

            atlas = atlasBuilder.GenerateAtlas(frames, out spriteImportData, textureSettings.transparentMask, false);


            atlas.filterMode = textureSettings.filterMode;
            atlas.alphaIsTransparency = false;
            atlas.wrapMode = textureSettings.wrapMode;
            atlas.name = "Texture";

            ctx.AddObjectToAsset("Texture", atlas);

            ctx.SetMainObject(atlas);

            switch (importType)
            {
                case AseFileImportType.LayerToSprite:
                case AseFileImportType.Sprite:
                    ImportSprites(ctx, aseFile, spriteImportData);
                    break;
                case AseFileImportType.Tileset:
                    ImportTileset(ctx, atlas);
                    break;
            }

            ctx.SetMainObject(atlas);
        }

        private void ImportSprites(AssetImportContext ctx, AseFile aseFile, SpriteImportData[] spriteImportData)
        {
            int spriteCount = spriteImportData.Length;


            Sprite[] sprites = new Sprite[spriteCount];

            for (int i = 0; i < spriteCount; i++)
            {
                Sprite sprite = Sprite.Create(atlas,
                    spriteImportData[i].rect,
                    spriteImportData[i].pivot, textureSettings.pixelsPerUnit, textureSettings.extrudeEdges,
                    textureSettings.meshType, spriteImportData[i].border, textureSettings.generatePhysics);
                sprite.name = string.Format("{0}_{1}", name, spriteImportData[i].name);

                ctx.AddObjectToAsset(sprite.name, sprite);
                sprites[i] = sprite;
            }

            GenerateAnimations(ctx, aseFile, sprites);
        }

        private void ImportTileset(AssetImportContext ctx, Texture2D atlas)
        {
            int cols = atlas.width / textureSettings.tileSize.x;
            int rows = atlas.height / textureSettings.tileSize.y;

            int width = textureSettings.tileSize.x;
            int height = textureSettings.tileSize.y;

            int index = 0;

            for (int y = rows - 1; y >= 0; y--)
            {
                for (int x = 0; x < cols; x++)
                {
                    Rect tileRect = new Rect(x * width, y * height, width, height);

                    Sprite sprite = Sprite.Create(atlas, tileRect, textureSettings.spritePivot,
                        textureSettings.pixelsPerUnit, textureSettings.extrudeEdges, textureSettings.meshType,
                        Vector4.zero, textureSettings.generatePhysics);
                    sprite.name = string.Format("{0}_{1}", name, index);

                    ctx.AddObjectToAsset(sprite.name, sprite);

                    index++;
                }
            }
        }

        private string GetFileName(string assetPath)
        {
            string[] parts = assetPath.Split('/');
            string filename = parts[parts.Length - 1];

            return filename.Substring(0, filename.LastIndexOf('.'));
        }

        private static AseFile ReadAseFile(string assetPath)
        {
            FileStream fileStream = new FileStream(assetPath, FileMode.Open, FileAccess.Read);
            AseFile aseFile = new AseFile(fileStream);
            fileStream.Close();

            return aseFile;
        }

        private void GenerateAnimations(AssetImportContext ctx, AseFile aseFile, Sprite[] sprites)
        {
            if (animationSettings == null)
                animationSettings = new AseFileAnimationSettings[0];

            var animSettings = new List<AseFileAnimationSettings>(animationSettings);
            var animations = aseFile.GetAnimations();

            if (animations.Length <= 0)
                return;

            if (animationSettings != null)
                RemoveUnusedAnimationSettings(animSettings, animations);

            int index = 0;

            foreach (var animation in animations)
            {
                AnimationClip animationClip = new AnimationClip();
                animationClip.name = name + "_" + animation.TagName;
                animationClip.frameRate = 25;

                AseFileAnimationSettings importSettings = GetAnimationSettingFor(animSettings, animation);
                importSettings.about = GetAnimationAbout(animation);


                EditorCurveBinding editorBinding = new EditorCurveBinding();
                editorBinding.path = "";
                editorBinding.propertyName = "m_Sprite";

                switch (bindType)
                {
                    case AseEditorBindType.SpriteRenderer:
                        editorBinding.type = typeof(SpriteRenderer);
                        break;
                    case AseEditorBindType.UIImage:
                        editorBinding.type = typeof(Image);
                        break;
                }


                int length = animation.FrameTo - animation.FrameFrom + 1;
                ObjectReferenceKeyframe[] spriteKeyFrames = new ObjectReferenceKeyframe[length + 1]; // plus last frame to keep the duration

                float time = 0;

                int from = (animation.Animation != LoopAnimation.Reverse) ? animation.FrameFrom : animation.FrameTo;
                int step = (animation.Animation != LoopAnimation.Reverse) ? 1 : -1;

                int keyIndex = from;

                for (int i = 0; i < length; i++)
                {
                    if (i >= length)
                    {
                        keyIndex = from;
                    }


                    ObjectReferenceKeyframe frame = new ObjectReferenceKeyframe();
                    frame.time = time;
                    frame.value = sprites[keyIndex];

                    time += aseFile.Frames[keyIndex].FrameDuration / 1000f;

                    keyIndex += step;
                    spriteKeyFrames[i] = frame;
                }

                float frameTime = 1f / animationClip.frameRate;

                ObjectReferenceKeyframe lastFrame = new ObjectReferenceKeyframe();
                lastFrame.time = time - frameTime;
                lastFrame.value = sprites[keyIndex - step];

                spriteKeyFrames[spriteKeyFrames.Length - 1] = lastFrame;


                AnimationUtility.SetObjectReferenceCurve(animationClip, editorBinding, spriteKeyFrames);
                AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(animationClip);

                switch (animation.Animation)
                {
                    case LoopAnimation.Forward:
                        animationClip.wrapMode = WrapMode.Loop;
                        settings.loopTime = true;
                        break;
                    case LoopAnimation.Reverse:
                        animationClip.wrapMode = WrapMode.Loop;
                        settings.loopTime = true;
                        break;
                    case LoopAnimation.PingPong:
                        animationClip.wrapMode = WrapMode.PingPong;
                        settings.loopTime = true;
                        break;
                }

                if (!importSettings.loopTime)
                {
                    animationClip.wrapMode = WrapMode.Once;
                    settings.loopTime = false;
                }

                AnimationUtility.SetAnimationClipSettings(animationClip, settings);
                ctx.AddObjectToAsset(animation.TagName, animationClip);

                index++;
            }

            animationSettings = animSettings.ToArray();
        }

        private void RemoveUnusedAnimationSettings(List<AseFileAnimationSettings> animationSettings,
            FrameTag[] animations)
        {
            for (int i = 0; i < animationSettings.Count; i++)
            {
                bool found = false;
                if (animationSettings[i] != null)
                {
                    foreach (var anim in animations)
                    {
                        if (animationSettings[i].animationName == anim.TagName)
                            found = true;
                    }
                }

                if (!found)
                {
                    animationSettings.RemoveAt(i);
                    i--;
                }
            }
        }

        public AseFileAnimationSettings GetAnimationSettingFor(List<AseFileAnimationSettings> animationSettings,
            FrameTag animation)
        {
            if (animationSettings == null)
                animationSettings = new List<AseFileAnimationSettings>();

            for (int i = 0; i < animationSettings.Count; i++)
            {
                if (animationSettings[i].animationName == animation.TagName)
                    return animationSettings[i];
            }

            animationSettings.Add(new AseFileAnimationSettings(animation.TagName));
            return animationSettings[animationSettings.Count - 1];
        }

        private string GetAnimationAbout(FrameTag animation)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("Animation Type:\t{0}", animation.Animation.ToString());
            sb.AppendLine();
            sb.AppendFormat("Animation:\tFrom: {0}; To: {1}", animation.FrameFrom, animation.FrameTo);

            return sb.ToString();
        }
    }
}
