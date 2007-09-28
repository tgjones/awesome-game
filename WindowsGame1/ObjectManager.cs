using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace WindowsGame1
{
    class ObjectManager
    {
        List<GameObject> objectList = new List<GameObject>();

        public void AddObject(GameObject gameObject)
        {
            objectList.Add(gameObject);
        }

        public void RemoveObject(GameObject gameObject)
        {
            objectList.Remove(gameObject);
        }

        public void Draw(GraphicsDevice graphicsDevice)
        {
            foreach (GameObject gameObject in objectList)
            {
                gameObject.Draw(graphicsDevice);
            }
        }

        public void Update(GameTime gameTime)
        {
            foreach (GameObject gameObject in objectList)
            {
                gameObject.Update(gameTime);
            }
        }
    }
}
