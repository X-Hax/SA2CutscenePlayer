using System.Numerics;

namespace SA3D.SA2CutscenePlayer.Player
{
    internal static class FadeUtil
    {
        public static Vector4 FadeColor(Vector4 from, Vector4 to, float distance)
        {
            float Fade(float from, float to)
            {
                if(from < to)
                {
                    from += distance;
                    if(from > to)
                    {
                        from = to;
                    }
                }
                else if(from > to)
                {
                    from -= distance;
                    if(from < to)
                    {
                        from = to;
                    }
                }

                return from;
            }

            return new(
                Fade(from.X, to.X),
                Fade(from.Y, to.Y),
                Fade(from.Z, to.Z),
                Fade(from.W, to.W));
        }
    }
}
