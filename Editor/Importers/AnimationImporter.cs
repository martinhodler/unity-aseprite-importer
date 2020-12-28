using System.Collections.Generic;
using System.Text;
using Aseprite;
using Aseprite.Chunks;
using AsepriteImporter.Settings;
using UnityEditor;
using UnityEngine;

namespace AsepriteImporter.Importers
{
    public class AnimationImporter
    {
        private readonly AseFile aseFile;

        public AnimationImporter(AseFile aseFile)
        {
            this.aseFile = aseFile;
        }

        public AseFileAnimationSettings[] GetAnimationImportSettings()
        {
            List<AseFileAnimationSettings> animationSettings = new List<AseFileAnimationSettings>();

            FrameTag[] frameTags = aseFile.GetAnimations();

            foreach (var frameTag in frameTags)
            {
                int frames = frameTag.FrameTo - frameTag.FrameFrom + 1;

                AseFileAnimationSettings setting = new AseFileAnimationSettings(frameTag.TagName)
                {
                    about = GetAnimationAbout(frameTag),
                    loopTime = true,
                    sprites = new Sprite[frames],
                    frameNumbers = new int[frames]
                };

                int frameFrom = frameTag.FrameFrom;
                int frameTo = frameTag.FrameTo;
                int step = (frameTag.Animation != LoopAnimation.Reverse) ? 1 : -1;

                int frameIndex = frameFrom;
                int i = 0;
                while (frameIndex != frameTo)
                {
                    setting.frameNumbers[i] = frameIndex;
                    frameIndex += step;
                    ++i;
                }

                setting.frameNumbers[i] = frameTo;

                animationSettings.Add(setting);
            }

            return animationSettings.ToArray();
        }

        public AnimationClip[] GenerateAnimations(string parentName, AseFileAnimationSettings[] animationSettings)
        {
            var animations = aseFile.GetAnimations();

            if (animations.Length <= 0)
                return new AnimationClip[0];

            AnimationClip[] animationClips = new AnimationClip[animations.Length];

            int index = 0;

            foreach (var animation in animations)
            {
                AseFileAnimationSettings importSettings = GetAnimationSettingFor(animationSettings, animation);
                if (importSettings == null)
                    continue;

                if (importSettings.HasInvalidSprites)
                    continue;

                AnimationClip animationClip = new AnimationClip
                {
                    name = parentName + "_" + animation.TagName,
                    frameRate = 25
                };

                EditorCurveBinding spriteBinding = new EditorCurveBinding
                {
                    type = typeof(SpriteRenderer),
                    path = "",
                    propertyName = "m_Sprite"
                };


                int length = animation.FrameTo - animation.FrameFrom + 1;
                ObjectReferenceKeyframe[]
                    spriteKeyFrames = new ObjectReferenceKeyframe[length + 1]; // plus last frame to keep the duration

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

                    ObjectReferenceKeyframe frame = new ObjectReferenceKeyframe
                    {
                        time = time,
                        value = importSettings.sprites[i]
                    };

                    time += aseFile.Frames[keyIndex].FrameDuration / 1000f;

                    keyIndex += step;
                    spriteKeyFrames[i] = frame;
                }

                float frameTime = 1f / animationClip.frameRate;

                ObjectReferenceKeyframe lastFrame = new ObjectReferenceKeyframe
                {
                    time = time - frameTime,
                    value = importSettings.sprites[length - 1]
                };

                spriteKeyFrames[spriteKeyFrames.Length - 1] = lastFrame;


                AnimationUtility.SetObjectReferenceCurve(animationClip, spriteBinding, spriteKeyFrames);
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
                animationClips[index] = animationClip;

                index++;
            }

            return animationClips;
        }


        public AseFileAnimationSettings GetAnimationSettingFor(AseFileAnimationSettings[] animationSettings,
            FrameTag animation)
        {
            for (int i = 0; i < animationSettings.Length; i++)
            {
                if (animationSettings[i].animationName == animation.TagName)
                    return animationSettings[i];
            }

            return null;
        }

        public string GetAnimationAbout(FrameTag animation)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("Animation Type:\t{0}", animation.Animation.ToString());
            sb.AppendLine();
            sb.AppendFormat("Animation:\tFrom: {0}; To: {1}", animation.FrameFrom, animation.FrameTo);

            return sb.ToString();
        }
    }
}