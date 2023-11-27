using SA3D.SA2Event;
using SA3D.Rendering;
using SA3D.SA2CutscenePlayer.Media;
using SA3D.SA2Event.Effects.Enums;
using SA3D.SA2Event.Effects;
using SA3D.SA2Event.Language;

namespace SA3D.SA2CutscenePlayer.Player
{
    public partial class EventPlayer
    {
        public VideoController? VideoController { get; }
        public SubtitleController? SubtitleController { get; }
        public AudioController AudioController { get; }


        public EventLanguage Language { get; }

        private uint _subtitleEnd;

        private void ResetMedia()
        {
            VideoController?.Reset();
            if(SubtitleController != null)
            {
                SubtitleController.DisplayIndex = -1;
            }

            AudioController.Reset();
            _subtitleEnd = 0;
        }

        private void UpdateMedia(RenderContext context, double delta)
        {
            UpdateVideo(context, delta);
            UpdateSubtitles();
            UpdateAudio();
        }

        private void UpdateVideo(RenderContext context, double delta)
        {
            if(Cutscene.Effects == null || VideoController == null)
            {
                return;
            }

            foreach(VideoOverlayEffect overlay in Cutscene.Effects.VideoOverlayEffects)
            {
                if(overlay.Frame <= Timestamp && overlay.Frame > _previousTimeStamp)
                {
                    switch(overlay.Type)
                    {
                        case VideoOverlayType.Overlay:
                        case VideoOverlayType.Overlay1:
                        case VideoOverlayType.Overlay2:
                            VideoController.StartUIVideo(overlay.Filename, overlay.Depth);
                            break;
                        case VideoOverlayType.Mesh:
                            VideoController.StartMeshVideo(overlay.Filename, overlay.TargetTextureID);
                            break;
                        case VideoOverlayType.Pause:
                            VideoController.Paused = true;
                            break;
                        case VideoOverlayType.Resume:
                            VideoController.Paused = false;
                            break;
                    }

                }
            }

            VideoController.Update(context, delta);
        }

        private void UpdateSubtitles()
        {
            if(!Cutscene.LanguageTimestamps.TryGetValue(Language, out EventLanguageTimestamps? language) || SubtitleController == null)
            {
                return;
            }

            for(int i = 0; i < language.SubtitlesTimestamps.Length; i++)
            {
                SubtitleTimestamp sub = language.SubtitlesTimestamps[i];
                if(sub.Frame <= Timestamp && sub.Frame > _previousTimeStamp)
                {
                    SubtitleController.DisplayIndex = i;
                    _subtitleEnd = sub.Frame + sub.Duration;
                    break;
                }
            }

            if(Timestamp > _subtitleEnd)
            {
                SubtitleController.DisplayIndex = -1;
            }

        }

        private void UpdateAudio()
        {
            AudioController.Update();

            if(!Cutscene.LanguageTimestamps.TryGetValue(Language, out EventLanguageTimestamps? language))
            {
                return;
            }

            foreach(AudioTimestamp audioInfo in language.AudioTimestamps)
            {
                if(audioInfo.Frame <= Timestamp && audioInfo.Frame > _previousTimeStamp)
                {
                    if(audioInfo.MusicName is "0" or "-")
                    {
                        AudioController.StopADX();
                    }
                    else if(!string.IsNullOrWhiteSpace(audioInfo.MusicName))
                    {
                        AudioController.PlayADX(audioInfo.MusicName);
                    }

                    if(audioInfo.MasterListVoiceIndex != ushort.MaxValue)
                    {

                    }

                    if(audioInfo.AFSVoiceIndex != ushort.MaxValue)
                    {
                        AudioController.PlayVoice(audioInfo.AFSVoiceIndex);
                    }

                }
            }
        }
    }
}
