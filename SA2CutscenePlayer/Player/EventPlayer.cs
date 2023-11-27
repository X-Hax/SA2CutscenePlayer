using SA3D.SA2Event;
using SA3D.SA2Event.Language;
using SA3D.SA2Event.Model;
using SA3D.Rendering;
using SA3D.Rendering.Input;
using SA3D.Rendering.Shaders;
using SA3D.Modeling.ObjectData;
using SA3D.SA2CutscenePlayer.Media;
using SA3D.Texturing;
using System;
using SA3D.SA2CutscenePlayer.Effects;
using System.Numerics;

namespace SA3D.SA2CutscenePlayer.Player
{
    public partial class EventPlayer
    {
        private readonly int[] _sceneOffsets;
        private readonly bool[] _upgrades;

        public Event Cutscene { get; }

        public TextureSet EventTextures { get; }

        public TextureSet EffectTextures { get; }

        public CameraController? CameraController { get; set; }


        public int SceneIndex { get; private set; }

        public Scene CurrentScene => Cutscene.ModelData.Scenes[SceneIndex + 1];

        public int CurrentSceneFrameOffset => _sceneOffsets[SceneIndex];

        private double _previousTimeStamp;
        public double Timestamp { get; private set; }
        public float SceneTimestamp => (float)Timestamp - CurrentSceneFrameOffset;
        public double DeltaTimestamp => Timestamp - _previousTimeStamp;
        public double PlaybackSpeed { get; set; }
        public bool Paused { get; private set; }



        public EventPlayer(
            Event cutscene,
            TextureSet eventTextures,
            TextureSet effectTextures,
            SubtitleFile? subtitles,
            uint eventIndex,
            EventLanguage language,
            string? videoDirectoryPath,
            string? adxDirectoryPath,
            string? mltDirectoryPath,
            string? voiceArchiveFilePath)
        {
            Cutscene = cutscene;
            EventTextures = eventTextures;
            EffectTextures = effectTextures;
            SubtitleController = new(subtitles, eventIndex);
            Language = language;

            VideoController = videoDirectoryPath == null ? null : new(eventTextures, videoDirectoryPath);
            AudioController = new(eventIndex, adxDirectoryPath, mltDirectoryPath, voiceArchiveFilePath, null)
            {
                ADXVolume = 0.6f,
                SoundEffectVolume = 0.4f,
            };
            AudioController.Init();


            PulseParticleController = new();
            LensFlareController = new(effectTextures);
            ScreenEffectController = new(eventTextures);
            BlareController = new();
            ReflectionHandler = new(cutscene.ModelData.Reflections.Reflections);

            AnimateCamera = true;
            PlaybackSpeed = 30;

            SceneIndex = -1;
            _sceneOffsets = new int[Cutscene.ModelData.Scenes.Count - 1];
            int offset = 0;
            for(int i = 1; i < Cutscene.ModelData.Scenes.Count; i++)
            {
                _sceneOffsets[i - 1] = offset;
                offset += Cutscene.ModelData.Scenes[i].FrameCount;
            }

            _upgrades = new bool[31];
            UpdateUpgradeModels();

            BufferModelData();

            if(cutscene.ModelData.TailsTails != null)
            {
                Node tails = cutscene.ModelData.TailsTails;
                if(tails.ChildCount >= 2
                    && tails[0].Attach != null
                    && tails[1].Attach != null)
                {
#pragma warning disable CS8604 // Possible null reference argument.
                    TailsAnimations = new TailsTailAnimation[]{
                        new(tails[0].Attach, false),
                        new(tails[1].Attach, true),
                    };
#pragma warning restore CS8604 // Possible null reference argument.
                }

            }
        }

        public void Pause()
        {
            if(Paused)
            {
                return;
            }

            Paused = true;
            AudioController.Pause();
        }

        public void Resume()
        {
            if(!Paused)
            {
                return;
            }

            Paused = false;
            AudioController.Resume();
        }

        private void BufferModelData()
        {
            foreach(Node model in Cutscene.ModelData.GetModels(true))
            {
                model.BufferMeshData(true);
            }
        }

        private void UpdateUpgradeModels()
        {
            for(int i = 0; i < _upgrades.Length; i++)
            {
                bool enable = _upgrades[i];

                void setNodes(int index, bool state)
                {
                    Node? node = Cutscene.ModelData.IntegratedUpgrades[i, index];
                    if(node == null)
                    {
                        return;
                    }

                    foreach(Node child in node.GetBranchNodes(false))
                    {
                        child.SkipDraw = state;
                    }
                }

                setNodes(0, !enable);
                setNodes(1, !enable);
                setNodes(2, enable);
            }
        }

        public void ToggleAllUpgrades(bool state)
        {
            Array.Fill(_upgrades, state);
            UpdateUpgradeModels();
        }

        public void ToggleUpgrade(int index, bool state)
        {
            _upgrades[index] = state;
            UpdateUpgradeModels();
        }

        public void Load(RenderContext context)
        {
            context.Settings = new()
            {
                DisableSurfaceAmbient = true,
                DisableBackfaceCulling = true
            };

            context.LoadTextureSet(EventTextures);
            context.LoadTextureSet(EffectTextures);
            ReflectionHandler.Initialize(context);

            StartScene(0);
        }

        public void StartScene(int index)
        {
            SceneIndex = index;
            Timestamp = index == 0 ? 0.99f : CurrentSceneFrameOffset;

            if(index == 0)
            {
                _previousTimeStamp = 0;
                if(TailsAnimations != null)
                {
                    foreach(TailsTailAnimation item in TailsAnimations)
                    {
                        item.Reset();
                    }
                }

                ResetLighting();
                ResetMedia();
                ResetEffects();
            }
        }

        private bool PauseHandling(RenderContext context, double delta)
        {
            if(context.Input.IsPressed(InputCode.P))
            {
                if(Paused)
                {
                    Resume();
                }
                else
                {
                    Pause();
                }
            }

            if(context.Input.IsPressed(InputCode.M))
            {
                AnimateCamera = !AnimateCamera;

                context.Camera.Orbiting = !AnimateCamera;
                if(AnimateCamera)
                {
                    context.Input.LockCursor = false;
                    context.Camera.Distance = 0;

                    if(Paused)
                    {
                        float motionTimestamp = (float)(Timestamp - CurrentSceneFrameOffset);
                        UpdateCamera(context, motionTimestamp);
                    }
                }
            }

            if(Paused)
            {
                if(!AnimateCamera)
                {
                    CameraController?.Run(delta);
                }
                // more on pause handling here
            }

            if(SubtitleController != null)
            {
                SubtitleController.Paused = Paused;
            }

            return Paused;
        }

        public void Update(RenderContext context, double delta)
        {
            if(PauseHandling(context, delta) || SceneIndex == -1)
            {
                return;
            }

            _previousTimeStamp = Timestamp;
            Timestamp += delta * PlaybackSpeed;

            if(Timestamp > Cutscene.ModelData.Scenes[0].FrameCount)
            {
                StartScene(0);
            }
            else if(SceneIndex < _sceneOffsets.Length - 1 && Timestamp >= _sceneOffsets[SceneIndex + 1])
            {
                StartScene(SceneIndex + 1);
            }

            UpdateAnimations(context);
            UpdateLighting(context);
            UpdateEffects(context);
            UpdateMedia(context, delta);
        }

        public void Render(RenderContext context)
        {
            context.SetMeshShader(Shaders.SurfaceDebug);
            context.ActiveTextures = EventTextures;

            foreach(EventEntry entity in Cutscene.ModelData.Scenes[0].Entries)
            {
                context.RenderModel(entity.DisplayModel);
            }

            foreach(EventEntry entity in CurrentScene.Entries)
            {
                context.RenderModel(entity.DisplayModel);
            }

            for(int i = 0; i < Cutscene.ModelData.OverlayUpgrades.Length; i++)
            {
                int lut = OverlayUpgrade.UpgradeEventLUT[i];
                if(lut == -2 || _upgrades[lut])
                {
                    OverlayUpgrade upgrade = Cutscene.ModelData.OverlayUpgrades[i];
                    if(upgrade.Model1 != null)
                    {
                        context.RenderModel(upgrade.Model1);
                    }

                    if(upgrade.Model2 != null)
                    {
                        context.RenderModel(upgrade.Model2);
                    }
                }
            }

            ReflectionHandler.Render(context, Cutscene.ModelData.Scenes[0], CurrentScene);

            BlareController.Render(context);
            PulseParticleController.Render(context, Cutscene.ModelData, SceneIndex, (float)Timestamp);
        }

    }
}
