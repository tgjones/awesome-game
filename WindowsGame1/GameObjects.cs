using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace WindowsGame1
{
    public abstract class GameObject
    {
        public Vector3 position;
        public Vector3 velocity;

        public virtual void Initialize(GraphicsDevice graphicsDevice)
        {
        }

        public virtual void Draw(GraphicsDevice graphicsDevice)
        {
        }

        public virtual void Update(GameTime gameTime)
        {
        }


    }

    public class Triangle : GameObject
    {
        private VertexBuffer vertexBuffer;
        private BasicEffect basicEffect;
        private VertexDeclaration vertexDeclaration;
        private Matrix worldMatrix = Matrix.Identity;

        public override void Initialize(GraphicsDevice graphicsDevice)
        {
            base.Initialize(graphicsDevice);
            
            Matrix viewMatrix = Matrix.CreateLookAt(new Vector3(0.0f, 0.0f, 10.0f), Vector3.Zero, new Vector3(0.0f, 1.0f, 0.0f));
            Matrix projectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45), (float) graphicsDevice.Viewport.Width/(float) graphicsDevice.Viewport.Height, 1.0f, 20.0f);

            basicEffect = new BasicEffect(graphicsDevice, null);
            basicEffect.World = worldMatrix;
            basicEffect.View = viewMatrix;
            basicEffect.Projection = projectionMatrix;
            basicEffect.VertexColorEnabled = true;

            vertexBuffer = new VertexBuffer(graphicsDevice, typeof (VertexPositionColor), 3, ResourceUsage.None);
            vertexDeclaration = new VertexDeclaration(graphicsDevice, VertexPositionColor.VertexElements);
            VertexPositionColor[] vertices = new VertexPositionColor[3];
            vertices[0] = new VertexPositionColor(new Vector3(0.0f, 0.0f, 0.0f), Color.Blue);
            vertices[1] = new VertexPositionColor(new Vector3(0.0f, 1.0f, 0.0f), Color.Blue);
            vertices[2] = new VertexPositionColor(new Vector3(1.0f, 0.0f, 0.0f), Color.Blue);
            vertexBuffer.SetData <VertexPositionColor> (vertices);
        }

        public override void Draw(GraphicsDevice graphicsDevice)
        {
            base.Draw(graphicsDevice);

            graphicsDevice.VertexDeclaration = vertexDeclaration;
            basicEffect.Begin();
            foreach (EffectPass pass in basicEffect.CurrentTechnique.Passes)
            {
                pass.Begin();
                graphicsDevice.Vertices[0].SetSource(vertexBuffer, 0, VertexPositionColor.SizeInBytes);
                graphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, 1);
                pass.End();
            }
            basicEffect.End();
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            worldMatrix = Matrix.CreateRotationZ((float) gameTime.TotalRealTime.TotalMilliseconds * 0.0001f);
            basicEffect.World = worldMatrix;
        }
    }

    public class BlockyCar : GameObject
    {
        private Model m_model;
        private GraphicsDevice m_graphics;
        private Matrix worldMatrix = Matrix.Identity;

        public override void Draw(GraphicsDevice graphicsDevice)
        {
            if (m_model != null)
            {
                //graphicsDevice.VertexDeclaration = vertexDeclaration;
                graphicsDevice.RenderState.CullMode = CullMode.None;
                foreach (ModelMesh mm in m_model.Meshes)
                {
                    mm.Draw();
                }
            }
            base.Draw(graphicsDevice);
        }

        public override void Initialize(GraphicsDevice graphicsDevice)
        {
            m_graphics = graphicsDevice;
            base.Initialize(graphicsDevice);
        }

        public virtual Model model
        {
            set
            {
                m_model = value;

                if (m_model != null)
                {
                    Matrix viewMatrix = Matrix.CreateLookAt(new Vector3(0.0f, 0.0f, 10.0f), Vector3.Zero, new Vector3(0.0f, 1.0f, 0.0f));
                    Matrix projectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45), (float)m_graphics.Viewport.Width / (float)m_graphics.Viewport.Height, 1.0f, 20.0f);

                    foreach (ModelMesh mm in m_model.Meshes)
                    {
                        foreach (ModelMeshPart mmp in mm.MeshParts)
                        {
                            ((BasicEffect)mmp.Effect).World = worldMatrix;
                            ((BasicEffect)mmp.Effect).View = viewMatrix;
                            ((BasicEffect)mmp.Effect).Projection = projectionMatrix;
                        }
                    }
                }
            }
            get
            {
                return m_model;
            }
        }
    }
}