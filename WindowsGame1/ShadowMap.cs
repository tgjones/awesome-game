using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AwesomeGame
{
	public class ShadowMap : GameObject
	{
		protected SpriteBatch _spriteBatch;
		protected RenderTarget2D _shadowMapRenderTarget;
		protected DepthStencilBuffer _shadowMapDepthStencilBuffer;
		protected Texture2D _shadowMapTexture;

		public Texture2D ShadowMapTexture
		{
			get { return _shadowMapTexture; }
		}

		public int ShadowMapSize
		{
			get { return 2048; }
		}

		public ShadowMap(Game game)
			: base(game)
		{
			game.Services.AddService(typeof(ShadowMap), this);
			this.UpdateOrder = 3000;
			this.DrawOrder = 10;
		}

		protected override void LoadGraphicsContent(bool loadAllContent)
		{
			_shadowMapRenderTarget = new RenderTarget2D(this.GraphicsDevice, this.ShadowMapSize, this.ShadowMapSize, 1, SurfaceFormat.Single);
			_shadowMapDepthStencilBuffer = new DepthStencilBuffer(this.GraphicsDevice, this.ShadowMapSize, this.ShadowMapSize, DepthFormat.Depth24Stencil8);
			_spriteBatch = new SpriteBatch(this.GraphicsDevice);
			base.LoadGraphicsContent(loadAllContent);
		}

		public override void Update(GameTime gameTime)
		{
			// enable rendering to shadow map texture
			DepthStencilBuffer oldDepthStencilBuffer = this.GraphicsDevice.DepthStencilBuffer;
			this.GraphicsDevice.DepthStencilBuffer = _shadowMapDepthStencilBuffer;
			this.GraphicsDevice.SetRenderTarget(0, _shadowMapRenderTarget);

			this.GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.White, 1.0f, 0);

			// loop through all other drawable game components, getting them to draw to the shadow map
			foreach (GameComponent gameComponent in this.Game.Components)
			{
				if (gameComponent is Mesh)
				{
					Mesh drawableGameComponent = (Mesh) gameComponent;
					drawableGameComponent.DrawShadowMap(gameTime);
				}
			}

			// extract shadow map texture from render target
			this.GraphicsDevice.ResolveRenderTarget(0);
			_shadowMapTexture = _shadowMapRenderTarget.GetTexture();

			// reset render target to back buffer
			this.GraphicsDevice.SetRenderTarget(0, null);
			this.GraphicsDevice.DepthStencilBuffer = oldDepthStencilBuffer;

			base.Update(gameTime);
		}

		public override void Draw(GameTime gameTime)
		{
			/*this.GraphicsDevice.RenderState.FillMode = FillMode.Solid;

			// render sprites with textures
			_spriteBatch.Begin();

			const int size = 128;
			Rectangle rectangle = new Rectangle(20, this.Game.Window.ClientBounds.Height - size - 20, size, size);
			_spriteBatch.Draw(_shadowMapTexture, rectangle, Color.White);

			_spriteBatch.End();

			base.Draw(gameTime);*/
		}
	}
}
