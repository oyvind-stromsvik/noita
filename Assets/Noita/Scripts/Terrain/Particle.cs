using UnityEngine;

namespace Noita {

    /// <summary>
    /// A particle is a pixel that is in the process of falling.
    /// </summary>
    public class Particle {
        public Color color;
        public int x;
        public int y;
        public Vector2 velocity;
        public Vector2 subPixelPositionRemainder;
    }

}
