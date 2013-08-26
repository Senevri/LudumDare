using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LD27
{
    class Animation
    {
        public int[] FrameIndexes { get; set; }
        public float DelaySeconds { get; set; }
        public int CurrentFrame { get; set; }
        public bool Playing { get; set; }
        public bool Loop { get; set; }

        private float _lastFrameShown;

        public Animation() {
            CurrentFrame = 0;
            Loop = true;
            DelaySeconds = 0.200f;
        }

        public int getNextAllowedIndex(float gameTimeTotalSeconds) {
            if ((gameTimeTotalSeconds - _lastFrameShown) > DelaySeconds) {
                _lastFrameShown = gameTimeTotalSeconds;
                return getNextIndex();
            }
            return FrameIndexes[CurrentFrame];
        }

        public int getNextIndex(int i=0) {
            if (CurrentFrame == FrameIndexes.Count() - 1) {
                if (Loop) {
                    CurrentFrame = 0;
                }
            } else {
                CurrentFrame++;
            }
            return FrameIndexes[CurrentFrame];
        }

        public Microsoft.Xna.Framework.Vector2 Position { get; set; }

        public Animation Copy() {
            var newAnimation = new Animation() { DelaySeconds = this.DelaySeconds, CurrentFrame = this.CurrentFrame, Loop =this.Loop, Position = this.Position};
            newAnimation.FrameIndexes = new int[FrameIndexes.Length];
            Array.Copy(FrameIndexes, newAnimation.FrameIndexes, 0);
            return newAnimation;
        }

        internal Animation PositionCopy(Microsoft.Xna.Framework.Vector2 v1)
        {
            var newAnimation = this.Copy();
            newAnimation.Position = v1;
            return newAnimation;
        }
    }
}
