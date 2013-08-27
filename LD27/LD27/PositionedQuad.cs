using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LD27
{
    class PositionedQuad : TexturedQuad
    {
        public Vector2 Position { get; set; }
        public Vector3 Rotation { get; set; }
        //public Microsoft.Xna.Framework.Graphics.Texture2D Texture { get; set; }
        private VertexPositionNormalTexture[] _vertices;
        private float aspect;
        private int p;
        public new Microsoft.Xna.Framework.Graphics.VertexPositionNormalTexture[] Vertices {
            get {
                if (this._vertices == null) { GenerateVertices(); };
                VertexPositionNormalTexture[] outVerts = new VertexPositionNormalTexture[6];
                for (int i = 0; i<_vertices.Length; i++) {                    
                    outVerts[i].TextureCoordinate = _vertices[i].TextureCoordinate;
                    outVerts[i].Normal = _vertices[i].Normal;
                    outVerts[i].Position = Vector3.Transform(_vertices[i].Position, Matrix.CreateRotationX(Rotation.X));
                    outVerts[i].Position = Vector3.Transform(outVerts[i].Position, Matrix.CreateRotationY(Rotation.Y));
                    outVerts[i].Position = Vector3.Transform(outVerts[i].Position, Matrix.CreateRotationZ(Rotation.Z));                    
                    outVerts[i].Position = Vector3.Transform(outVerts[i].Position, Matrix.CreateTranslation(new Vector3(Position.X, Position.Y, -1.0f)));
                    
                }
                    return outVerts;
            }
            set {
                _vertices = value;
            }            
        }

        public bool Show { get; set; }

        /*public Matrix GetTranslationMatrix() {
            Matrix outMatrix; 
            var tl = new Vector3(Position.X, Position.Y, -1.0f);
            Matrix.CreateTranslation(tl.X, tl.Y, tl.Z, outMatrix);
            return outMatrix;
        }*/

        public PositionedQuad(TexturedQuad quad, Vector2 position) {
            Texture = quad.Texture;
            Vertices = quad.Vertices;
            Position = position;
            Show = true;
            ScaleX = quad.ScaleX;
            ScaleY = quad.ScaleY;
            GenerateVertices();
        }

        public PositionedQuad(float scale,  float scaley=0)
        {
            // TODO: Complete member initialization
            this.ScaleX = scale;
            this.ScaleY = scaley;
            if (ScaleY == 0)
            {
                ScaleY = ScaleX;
            }
            GenerateVertices();
        }
        public PositionedQuad(Texture2D texture, float scale, float scaley = 1)
        {
            // TODO: Complete member initialization
            this.Texture = Texture;
            this.ScaleX = scale;
            this.ScaleY = scale;
            if (ScaleY == 0)
            {
                ScaleY = ScaleX;
            }
            GenerateVertices();
        }

        protected new void GenerateVertices()
        {
            _vertices = new VertexPositionNormalTexture[6];
            _vertices[0] = genVertice(-1.0f, 1.0f, 0, 0.0f, 0.0f);
            _vertices[1] = genVertice(1.0f, 1.0f, 0, 1.0f, 0.0f);
            _vertices[2] = genVertice(-1.0f, -1.0f, 0, 0.0f, 1.0f);

            _vertices[3] = genVertice(-1.0f, -1.0f, 0, 0.0f, 1.0f);
            _vertices[4] = genVertice(1.0f, 1.0f, 0, 1.0f, 0.0f);
            _vertices[5] = genVertice(1.0f, -1.0f, 0, 1.0f, 1.0f);
        }

        
    }
}
