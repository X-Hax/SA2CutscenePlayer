using SA3D.SA2Event.Effects;
using SA3D.SA2Event.Model;
using SA3D.Rendering;
using SA3D.Modeling.ObjectData;
using System;
using System.Collections.Generic;
using SA3D.SA2CutscenePlayer.Player;

namespace SA3D.SA2CutscenePlayer.Effects
{
    public class BlareGhost
    {
        public EventEntry Entity { get; }
        public float Timestamp { get; }
        public int Lifespan { get; }
        public int LifeCounter { get; set; }

        public BlareGhost(EventEntry entity, float timestamp, int lifespan)
        {
            Entity = entity;
            Timestamp = timestamp;
            Lifespan = lifespan;
            LifeCounter = 0;
        }

        public void Render(RenderContext context)
        {
            context.Settings = new(context.Settings)
            {
                TransparencySubtract = LifeCounter / (float)Lifespan
            };

            Entity.ProcessEntityAnimation(context, Timestamp);
            context.RenderModel(Entity.DisplayModel);
        }
    }

    public class BlareEffectController
    {
        private readonly Queue<BlareGhost> _ghosts = new();
        private readonly GhostTracker[] _trackers = new GhostTracker[64];

        private struct GhostTracker
        {
            public bool use;
            public int counter;
            public int lifespan;
            public int duration;
        }

        public void Clear()
        {
            _ghosts.Clear();
            Array.Clear(_trackers);
        }

        private IEnumerable<int> EnumerateModelIndices(BlareEffect effect)
        {
            yield return effect.ModelIndex1;
            yield return effect.ModelIndex2;
            yield return effect.ModelIndex3;
            yield return effect.ModelIndex4;
            yield return effect.ModelIndex5;
            yield return effect.ModelIndex6;
        }

        public void CreateGhostTracker(BlareEffect effect)
        {
            foreach(int index in EnumerateModelIndices(effect))
            {
                if(index is < 0 or > 64)
                {
                    continue;
                }

                _trackers[index] = new()
                {
                    use = true,
                    duration = effect.Duration,
                    lifespan = effect.GhostLifeSpan
                };
            }
        }

        public void AddGhosts(ModelData data, int sceneIndex, float timestamp)
        {
            for(int i = 0; i < _trackers.Length; i++)
            {
                GhostTracker tracker = _trackers[i];
                if(!tracker.use)
                {
                    continue;
                }

                Node? blareModel = data.BlareModels[i];
                if(blareModel == null)
                {
                    _trackers[i].use = false;
                    continue;
                }

                foreach(EventEntry entity in data.Scenes[sceneIndex].Entries)
                {
                    if(entity.DisplayModel == blareModel)
                    {
                        _ghosts.Enqueue(new(entity, timestamp, tracker.lifespan));

                        tracker.counter++;
                        if(tracker.counter > tracker.duration)
                        {
                            _trackers[i].use = false;
                        }

                        break;
                    }
                }
            }
        }

        public void UpdateGhosts()
        {
            int ghostCount = _ghosts.Count;
            for(int i = 0; i < ghostCount; i++)
            {
                BlareGhost ghost = _ghosts.Dequeue();
                ghost.LifeCounter++;
                if(ghost.LifeCounter < ghost.Lifespan)
                {
                    _ghosts.Enqueue(ghost);
                }
            }
        }

        public void Render(RenderContext context)
        {
            foreach(BlareGhost ghost in _ghosts)
            {
                ghost.Render(context);
            }

            context.Settings = new(context.Settings)
            {
                TransparencySubtract = 0
            };
        }

    }
}
