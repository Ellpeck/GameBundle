using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MLEM.Startup;

namespace Test {
    public class GameImpl : MlemGame {

        public static GameImpl Instance { get; private set; }
        private Texture2D texture;

        public GameImpl() {
            Instance = this;
        }

        protected override void LoadContent() {
            base.LoadContent();
            this.texture = LoadContent<Texture2D>("Textures/Test");
        }

        protected override void DoDraw(GameTime gameTime) {
            this.GraphicsDevice.Clear(Color.Black);
            base.DoDraw(gameTime);
            this.SpriteBatch.Begin(samplerState: SamplerState.PointClamp, transformMatrix: Matrix.CreateScale(10));
            this.SpriteBatch.Draw(this.texture, Vector2.Zero, Color.White);
            this.SpriteBatch.End();
        }

    }
}