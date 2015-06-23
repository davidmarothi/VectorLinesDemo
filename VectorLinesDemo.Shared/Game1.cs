using ClipperLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using System;
using System.Collections.Generic;
using System.Linq;
using TriangleNet;
using TriangleNet.Geometry;
using TrianglePoint = TriangleNet.Geometry.Point;

namespace VectorLinesDemo.Shared
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        //Here we are scaling all coordinates up by 100 when they're passed to Clipper 
        //via Polygon (or Polygons) objects because Clipper no longer accepts floating  
        //point values. Likewise when Clipper returns a solution in a Polygons object, 
        //we need to scale down these returned values by the same amount before displaying.
        public static readonly int ClipperScale = 100; //or 1 or 10 or 10000 etc for lesser or greater precision

        private GraphicsDeviceManager graphicsDeviceManager;
        private GestureSample gestureSample;
        private KeyboardState keyboardState;

        private VertexPositionColor[] vertexPositionColorArray;
        private VertexBuffer vertexBuffer;
        private BasicEffect effect;
        private List<Vector2> points;

        private float lineThickness = 10.0f;

        private RasterizerState wireFrameRasterizerState;
        private RasterizerState solidRasterizerState;
        private bool useWireframe;

        public Game1()
        {
            points = new List<Vector2>();
            useWireframe = false;

            Window.AllowUserResizing = true;
            Content.RootDirectory = "Content";

            graphicsDeviceManager = new GraphicsDeviceManager(this);
            graphicsDeviceManager.SupportedOrientations = DisplayOrientation.LandscapeLeft | DisplayOrientation.LandscapeRight;

#if WINDOWS_UAP
            Windows.UI.ViewManagement.ApplicationView.GetForCurrentView().TryResizeView(new Windows.Foundation.Size(960, 540));
#endif
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
#if !WINDOWS_PHONE_APP
            IsMouseVisible = true;
#endif
            TouchPanel.EnableMouseGestures = true;
            TouchPanel.EnableMouseTouchPoint = true;
            TouchPanel.EnabledGestures = GestureType.Tap | GestureType.Hold;

            effect = new BasicEffect(GraphicsDevice);
            wireFrameRasterizerState = new RasterizerState() { CullMode = CullMode.None, FillMode = FillMode.WireFrame };
            solidRasterizerState = new RasterizerState() { CullMode = CullMode.None, FillMode = FillMode.Solid };

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // TODO: use this.Content to load your game content here           
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            while (TouchPanel.IsGestureAvailable == true)
            {
                gestureSample = TouchPanel.ReadGesture();
                switch (gestureSample.GestureType)
                {
                    case GestureType.Tap:
                        points.Add(gestureSample.Position);
                        RecalculateVertices();
                        break;
                    case GestureType.Hold:
                        useWireframe = !useWireframe;
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }

            var previousKeyboardState = keyboardState;
            keyboardState = Keyboard.GetState();
            if (previousKeyboardState.IsKeyUp(Keys.Space) && keyboardState.IsKeyDown(Keys.Space))
            {
                useWireframe = !useWireframe;
            }

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            if (vertexPositionColorArray?.Length > 0)//.? C#6 feature
            {
                GraphicsDevice.SetVertexBuffer(vertexBuffer);
                GraphicsDevice.BlendState = BlendState.AlphaBlend;
                GraphicsDevice.DepthStencilState = DepthStencilState.Default;
                GraphicsDevice.RasterizerState = useWireframe == true ? wireFrameRasterizerState : solidRasterizerState;

                effect.World = Matrix.Identity;
                effect.Projection = Matrix.CreateOrthographicOffCenter(0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height, 0, 0, 10);

                effect.VertexColorEnabled = true;
                effect.CurrentTechnique.Passes[0].Apply();
                GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, vertexPositionColorArray.Length / 3);
            }

            base.Draw(gameTime);
        }

        private void RecalculateVertices()
        {
            if (points.Count < 2)
            {
                return;
            }

            vertexPositionColorArray = null;

            var lineColor = Color.Red;

            #region clipper
            List<IntPoint> clipperPath = new List<IntPoint>(points.Count);
            foreach (var point in points)
            {
                clipperPath.Add(new IntPoint(point.X * ClipperScale, point.Y * ClipperScale));
            }

            List<List<IntPoint>> clipperSolution = new List<List<IntPoint>>();
            ClipperOffset clipperOffset = new ClipperOffset();

            clipperOffset.AddPath(clipperPath, JoinType.jtRound, EndType.etOpenRound);
            clipperOffset.Execute(ref clipperSolution, lineThickness / 2.0f * ClipperScale);
            #endregion

            #region triangle.net
            InputGeometry InputGeometry = new InputGeometry();
            Mesh TriangleMesh = new Mesh();

            for (int iii = 0; iii < clipperSolution.Count; iii++)
            {
                var trianglePath = clipperSolution[iii].Select(p => new TrianglePoint(p.X / (float)ClipperScale, p.Y / (float)ClipperScale)).ToList();

                if (iii == 0)
                {
                    InputGeometry.AddRing(trianglePath);
                }
                else
                {
                    InputGeometry.AddRingAsHole(trianglePath);
                }
            }
            #endregion

            if (InputGeometry.Count > 0)
            {
                TriangleMesh.Triangulate(InputGeometry);

                vertexPositionColorArray = TriangleMesh.GetTriangleList().Select(v => new VertexPositionColor(new Vector3((float)v.X, (float)v.Y, 0.0f), lineColor)).ToArray();

                vertexBuffer = new VertexBuffer(GraphicsDevice, VertexPositionColor.VertexDeclaration, vertexPositionColorArray.Length, BufferUsage.WriteOnly);
                vertexBuffer.SetData<VertexPositionColor>(vertexPositionColorArray);
            }
        }
    }
}
