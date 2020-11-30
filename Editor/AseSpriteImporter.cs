using System;
using System.Collections.Generic;
using System.IO;
using Aseprite;
using Aseprite.Chunks;
using Aseprite.Utils;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.UI;

namespace AseImporter {
    public class AseSpriteImporter {
        private AseFileTextureSettings settings;
        private int padding = 1;
        private Vector2Int size;
        private string fileName;
        private string directoryName;
        private string filePath;
        private static EditorApplication.CallbackFunction onUpdate;
        private int updateLimit;
        private int rows;
        private int cols;
        private Texture2D []frames;
        private AseFile file;
        
        public void Import(string path, AseFile file, AseFileTextureSettings settings) {
            this.file = file;
            this.settings = settings;
            this.size = new Vector2Int(file.Header.Width, file.Header.Height); 

            frames = file.GetFrames();
            BuildAtlas(path);
            
            // async process
            if (onUpdate == null) {
                onUpdate = OnUpdate;
            }

            updateLimit = 300;
            EditorApplication.update = Delegate.Combine(EditorApplication.update, onUpdate) as EditorApplication.CallbackFunction;
        }

        private void OnUpdate() {
            AssetDatabase.Refresh();
            var done = false;
            if (GenerateSprites(filePath, settings, size)) {
                GeneratorAnimations();
                done = true;
            } else {
                updateLimit--;
                if (updateLimit <= 0) {
                    done = true;
                }
            }

            if (done) {
                EditorApplication.update = Delegate.Remove(EditorApplication.update, onUpdate) as EditorApplication.CallbackFunction;
            }
        }

        private void BuildAtlas(string acePath) {
            fileName = Path.GetFileNameWithoutExtension(acePath);
            directoryName = Path.GetDirectoryName(acePath) + "/" + fileName;
            if (!AssetDatabase.IsValidFolder(directoryName)) {
                AssetDatabase.CreateFolder(Path.GetDirectoryName(acePath), fileName);
            }

            filePath = directoryName + "/" + fileName + ".png";

            var atlas = GenerateAtlas(frames);
            try {
                File.WriteAllBytes(filePath, atlas.EncodeToPNG());
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            } catch (Exception e) {
                Debug.LogError(e.Message);
            }
        }

        public Texture2D GenerateAtlas(Texture2D []sprites) {
            var area = size.x * size.y * sprites.Length;
            var sqrt = Mathf.Sqrt(area);
            cols = Mathf.CeilToInt(sqrt / size.x);
            rows = Mathf.CeilToInt(sqrt / size.y);

            var width = cols * (size.x + padding * 2);
            var height = rows * (size.y + padding * 2);
            var atlas = Texture2DUtil.CreateTransparentTexture(width, height);

            var index = 0;
            for (var row = 0; row < rows; row++) {
                for (var col = 0; col < cols; col++) {
                    if (index == sprites.Length) {
                        break;
                    }

                    var sprite = sprites[index];
                    var rect = new RectInt(col * (size.x + padding * 2) + padding, 
                                           height - (row + 1) * (size.y + padding * 2) + padding, 
                                           size.x, 
                                           size.y);
                    CopyColors(sprite, atlas, rect);
                    index++;
                }
            }

            return atlas;
        }

        private Color[] GetPixels(Texture2D sprite) {
            var res = sprite.GetPixels();
            return res;
        }

        private void CopyColors(Texture2D sprite, Texture2D atlas, RectInt to ) {
            atlas.SetPixels(to.x, to.y, to.width, to.height, GetPixels(sprite));
        }
        
        private bool GenerateSprites(string path, AseFileTextureSettings settings, Vector2Int size) {
            this.settings = settings;
            this.size = size; 

            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) {
                return false;
            }

            importer.textureType = TextureImporterType.Sprite;
            importer.spritePixelsPerUnit = settings.pixelsPerUnit;
            importer.mipmapEnabled = false;
            importer.filterMode = FilterMode.Point;
            importer.spritesheet = CreateMetaData(fileName);

            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.spriteImportMode = SpriteImportMode.Multiple;

            EditorUtility.SetDirty(importer);
            try {
                importer.SaveAndReimport();
            } catch (Exception e) {
                Debug.LogWarning("There was a problem with generating sprite file: " + e);
            }

            AssetDatabase.Refresh();
            AssetDatabase.SaveAssets();
            return true;
        }

        private SpriteMetaData[] CreateMetaData(string fileName) {
            var res = new SpriteMetaData[rows * cols];
            var index = 0;
            var height = rows * (size.y + padding * 2);
            
            for (var row = 0; row < rows; row++) {
                for (var col = 0; col < cols; col++) {
                    Rect rect = new Rect(col * (size.x + padding * 2) + padding,
                                         height - (row + 1) * (size.y + padding * 2) + padding, 
                                         size.x,
                                         size.y);
                    var meta = new SpriteMetaData();
                    var no = col + row * rows;
                    meta.name = fileName + "_" + no;
                    meta.rect = rect;
                    meta.alignment = settings.spriteAlignment;
                    meta.pivot = settings.spritePivot;
                    res[index] = meta;
                    index++;
                }
            }

            return res;
        }

        private void GeneratorAnimations() {
            var sprites = GetAllSpritesFromAssetFile(filePath);
            var clips = GenerateAnimations(file, sprites);

            if (settings.animType == AseAnimatorType.AnimatorController) {
                CreateAnimatorController(clips);
            } else if (settings.animType == AseAnimatorType.AnimatorOverrideController) {
                CreateAnimatorOverrideController(clips);
            }
        }

        private WrapMode GetDefaultWrapMode(string animName) {
            animName = animName.ToLower();
            if (animName.IndexOf("walk") >= 0 || 
                animName.IndexOf("run") >= 0 || 
                animName.IndexOf("idle") >= 0) {
                return WrapMode.Loop;
            }

            return WrapMode.Once;
        }
        
        private List<AnimationClip> GenerateAnimations(AseFile aseFile, Sprite[] sprites) {
            List<AnimationClip> res = new List<AnimationClip>();
            var animations = aseFile.GetAnimations();
            if (animations.Length <= 0) {
                return res;
            }

            var metadatas = aseFile.GetMetaData(settings.spritePivot, settings.pixelsPerUnit);

            int index = 0;
            foreach (var animation in animations) {
                var path = directoryName + "/" + fileName + "_" + animation.TagName + ".anim";
                AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
                if (clip == null) {
                    clip = new AnimationClip();
                    AssetDatabase.CreateAsset(clip, path);
                    clip.wrapMode = GetDefaultWrapMode(animation.TagName);
                } else {
                    AnimationClipSettings animSettings = AnimationUtility.GetAnimationClipSettings(clip);
                    clip.wrapMode = animSettings.loopTime ? WrapMode.Loop : WrapMode.Once; 
                }
                
                clip.name = fileName + "_" + animation.TagName;
                clip.frameRate = 25;

                EditorCurveBinding editorBinding = new EditorCurveBinding();
                editorBinding.path = "";
                editorBinding.propertyName = "m_Sprite";

                switch (this.settings.bindType) {
                    case AseEditorBindType.SpriteRenderer:
                        editorBinding.type = typeof(SpriteRenderer);
                        break;
                    case AseEditorBindType.UIImage:
                        editorBinding.type = typeof(Image);
                        break;
                }

                // plus last frame to keep the duration
                int length = animation.FrameTo - animation.FrameFrom + 1;
                ObjectReferenceKeyframe[] spriteKeyFrames = new ObjectReferenceKeyframe[length + 1];
                Dictionary<string, AnimationCurve> transformCurveX = new Dictionary<string, AnimationCurve>(),
                                                   transformCurveY = new Dictionary<string, AnimationCurve>();

                float time = 0;
                int from = (animation.Animation != LoopAnimation.Reverse) ? animation.FrameFrom : animation.FrameTo;
                int step = (animation.Animation != LoopAnimation.Reverse) ? 1 : -1;

                int keyIndex = from;
                for (int i = 0; i < length; i++) {
                    if (i >= length) {
                        keyIndex = from;
                    }

                    ObjectReferenceKeyframe frame = new ObjectReferenceKeyframe();
                    frame.time = time;
                    frame.value = sprites[keyIndex];

                    time += aseFile.Frames[keyIndex].FrameDuration / 1000f;
                    spriteKeyFrames[i] = frame;

                    foreach (var metadata in metadatas) {
                        if (metadata.Type == MetaDataType.TRANSFORM && metadata.Transforms.ContainsKey(keyIndex)) {
                            var childTransform = metadata.Args[0];
                            if (!transformCurveX.ContainsKey(childTransform)) {
                                transformCurveX[childTransform] = new AnimationCurve();
                                transformCurveY[childTransform] = new AnimationCurve();
                            }
                            var pos = metadata.Transforms[keyIndex];
                            transformCurveX[childTransform].AddKey(i, pos.x);
                            transformCurveY[childTransform].AddKey(i, pos.y);
                        }
                    }

                    keyIndex += step;
                }

                float frameTime = 1f / clip.frameRate;
                ObjectReferenceKeyframe lastFrame = new ObjectReferenceKeyframe();
                lastFrame.time = time - frameTime;
                lastFrame.value = sprites[keyIndex - step];

                spriteKeyFrames[spriteKeyFrames.Length - 1] = lastFrame;
                foreach (var metadata in metadatas) {
                    if (metadata.Type == MetaDataType.TRANSFORM && metadata.Transforms.ContainsKey(keyIndex - step)) {
                        var childTransform = metadata.Args[0];
                        var pos = metadata.Transforms[keyIndex - step];
                        transformCurveX[childTransform].AddKey(spriteKeyFrames.Length - 1, pos.x);
                        transformCurveY[childTransform].AddKey(spriteKeyFrames.Length - 1, pos.y);
                    }
                }

                AnimationUtility.SetObjectReferenceCurve(clip, editorBinding, spriteKeyFrames);
                foreach (var childTransform in transformCurveX.Keys) {
                    EditorCurveBinding
                    bindingX = new EditorCurveBinding {
                        path = childTransform, 
                        type = typeof(Transform), 
                        propertyName = "m_LocalPosition.x"
                    },
                    bindingY = new EditorCurveBinding {
                        path = childTransform, 
                        type = typeof(Transform), 
                        propertyName = "m_LocalPosition.y"
                    };
                    MakeConstant(transformCurveX[childTransform]);
                    AnimationUtility.SetEditorCurve(clip, bindingX, transformCurveX[childTransform]);
                    MakeConstant(transformCurveY[childTransform]);
                    AnimationUtility.SetEditorCurve(clip, bindingY, transformCurveY[childTransform]);
                }

                AnimationClipSettings clipSettings = AnimationUtility.GetAnimationClipSettings(clip);
                clipSettings.loopTime = (clip.wrapMode == WrapMode.Loop);

                AnimationUtility.SetAnimationClipSettings(clip, clipSettings);
                EditorUtility.SetDirty(clip);
                index++;
                res.Add(clip);
            }

            return res;
        }

        private static void MakeConstant(AnimationCurve curve) {
            for (int i = 0; i < curve.length; ++i) {
                AnimationUtility.SetKeyRightTangentMode(curve, i, AnimationUtility.TangentMode.Constant);
            }
        }
        
        private static Sprite[] GetAllSpritesFromAssetFile(string imageFilename) {
            var assets = AssetDatabase.LoadAllAssetsAtPath(imageFilename);

            // make sure we only grab valid sprites here
            List<Sprite> sprites = new List<Sprite>();
            foreach (var item in assets) {
                if (item is Sprite) {
                    sprites.Add(item as Sprite);
                }
            }

            return sprites.ToArray();
        }
        
        private void CreateAnimatorController(List<AnimationClip> animations) {
            var path = directoryName + "/" + fileName + ".controller";
            AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(path);

            if (controller == null) {
                controller = AnimatorController.CreateAnimatorControllerAtPath(path);
                controller.AddLayer("Default");

                foreach (var animation in animations) {
                    var stateName = animation.name;
                    stateName = stateName.Replace(fileName + "_", "");
                    
                    AnimatorState state = controller.layers[0].stateMachine.AddState(stateName);
                    state.motion = animation;
                }
            } else {
                var clips = new Dictionary<string, AnimationClip>();
                foreach (var anim in animations) {
                    var stateName = anim.name;
                    stateName = stateName.Replace(fileName + "_", "");
                    clips[stateName] = anim;
                }
                
                var childStates = controller.layers[0].stateMachine.states;
                foreach (var childState in childStates) {
                    if (clips.TryGetValue(childState.state.name, out AnimationClip clip)) {
                        childState.state.motion = clip;
                    }
                }
            }

            EditorUtility.SetDirty(controller);
            AssetDatabase.SaveAssets();
        }
        
        private void CreateAnimatorOverrideController(List<AnimationClip> animations) {
            var path = directoryName + "/" + fileName + ".overrideController";
            var controller = AssetDatabase.LoadAssetAtPath<AnimatorOverrideController>(path);
            var baseController = controller?.runtimeAnimatorController;
            if (controller == null) {
                controller = new AnimatorOverrideController();
                AssetDatabase.CreateAsset(controller, path);
                baseController = settings.baseAnimator;
            } 
            
            if (baseController == null) {
                Debug.LogError("Can not make override controller");
                return;
            }

            controller.runtimeAnimatorController = baseController;
            var clips = new Dictionary<string, AnimationClip>();
            foreach (var anim in animations) {
                var stateName = anim.name;
                stateName = stateName.Replace(fileName + "_", "");
                clips[stateName] = anim;
            }
            
            var clipPairs = new List<KeyValuePair<AnimationClip, AnimationClip>>(controller.overridesCount);
            controller.GetOverrides(clipPairs);

            foreach (var pair in clipPairs) {
                string animationName = pair.Key.name;
                if (clips.TryGetValue(animationName, out AnimationClip clip)) {
                    controller[animationName] = clip;
                }
            }

            EditorUtility.SetDirty(controller);
            AssetDatabase.SaveAssets();
        }
    }
}