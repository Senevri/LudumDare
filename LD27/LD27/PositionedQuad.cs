﻿using System;
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
        public new Microsoft.Xna.Framework.Graphics.VertexPositionNormalTexture[] Vertices {
            get {
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
            Scale = quad.Scale;
            ScaleY = quad.ScaleY;
        }

        
    }
}