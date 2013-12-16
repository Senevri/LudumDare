using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LD27
{
    class TexturedQuad
    {
        public Microsoft.Xna.Framework.Graphics.Texture2D Texture { get; set; }

        public Microsoft.Xna.Framework.Graphics.VertexPositionNormalTexture[] Vertices { get; set; }

        public float ScaleX { get; set; }
        public float ScaleY { get; set; }
        public float ScaleZ { get; set; }


        public TexturedQuad() {
            if (ScaleX == 0) { ScaleX = 1f; }
            if (ScaleY == 0) {
                ScaleY = ScaleX;
            }
            if (ScaleZ == 0) {
                ScaleZ = 1f;
            }
            //GenerateVertices();
        }

        public TexturedQuad(int x, int y, int w, int h) {
            GenerateVerticesWithSubsection(x, y, w, h);
        }

        public TexturedQuad(float scale, float scaleY = 0)
        {            
            ScaleX = scale;
            ScaleY = scaleY;
            if (ScaleY == 0)
            {
                ScaleY = ScaleX;
            }

            GenerateVertices();
        }

        private Vector3 normal = Vector3.Backward;

        public TexturedQuad Rescale(float scale) {
            GenerateVertices();
            return this;
        }

        public void GenerateVertices()
        {
            Vertices = new VertexPositionNormalTexture[6];
            Vertices[0] = genVertice(-1.0f, 1.0f, 0, 0.0f, 0.0f);
            Vertices[1] = genVertice(1.0f, 1.0f, 0, 1.0f, 0.0f);
            Vertices[2] = genVertice(-1.0f, -1.0f, 0, 0.0f, 1.0f);

            Vertices[3] = genVertice(-1.0f, -1.0f, 0, 0.0f, 1.0f);
            Vertices[4] = genVertice(1.0f, 1.0f, 0, 1.0f, 0.0f);
            Vertices[5] = genVertice(1.0f, -1.0f, 0, 1.0f, 1.0f);
        }

        public void GenerateVerticesWithSubsection(int x, int y, int width, int height) {            
            //0.0f
            float xp = (float)x / Texture.Width;
            float yp = (float)y / Texture.Height;

            // 1.0f
            float xw = (float)(x + width) / Texture.Width;
            float yw = (float)(y + height) / Texture.Height;
            Vertices = new VertexPositionNormalTexture[6];
            Vertices[0] = genVertice(-1.0f, 1.0f, 0, xp, yp);
            Vertices[1] = genVertice(1.0f, 1.0f, 0, xw, yp);
            Vertices[2] = genVertice(-1.0f, -1.0f, 0, xp, yw);

            Vertices[3] = genVertice(-1.0f, -1.0f, 0, xp, yw);
            Vertices[4] = genVertice(1.0f, 1.0f, 0, xw, yp);
            Vertices[5] = genVertice(1.0f, -1.0f, 0, xw, yw);
        }
        

        protected VertexPositionNormalTexture genVertice(float x, float y, float z, float tx, float ty) {
            return new VertexPositionNormalTexture(new Vector3(x*ScaleX, y*ScaleY, z*ScaleZ), normal, new Vector2(tx, ty));
        }
    }
}
