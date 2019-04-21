namespace Stonehenge.Game1

open System

open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Input
open Microsoft.Xna.Framework.Graphics

open Stonehenge.VertexPositionNormal
open Stonehenge.FreeCamera
open Stonehenge.Input
open Stonehenge.Terrain
open Stonehenge.ContentLoader
open Stonehenge.EnvironmentParameters
open Stonehenge.Sphere
open Stonehenge.Sky

type LandGame() as _this =
    inherit Game()
    let graphics = new GraphicsDeviceManager(_this)
    let mutable spriteBatch = Unchecked.defaultof<SpriteBatch>
    let mutable effects = Unchecked.defaultof<Effects>
    let mutable environment = Unchecked.defaultof<EnvironmentParameters>
    let mutable sky = Unchecked.defaultof<Sky>
    let mutable vertices = Unchecked.defaultof<VertexPositionNormalTexture[]>
    let mutable debugVertices = Unchecked.defaultof<VertexPositionTexture[]>
    let mutable indices = Unchecked.defaultof<int[]>
    let mutable world = Unchecked.defaultof<Matrix>
    let mutable view = Unchecked.defaultof<Matrix>
    let mutable projection = Unchecked.defaultof<Matrix>
    let mutable device = Unchecked.defaultof<GraphicsDevice>
    let mutable terrain = Unchecked.defaultof<Terrain>
    let mutable textures = Unchecked.defaultof<Textures>
    let mutable initialLightDirection = Unchecked.defaultof<Vector3>
    let mutable lightDirection = Unchecked.defaultof<Vector3>
    let mutable hdrRenderTarget = Unchecked.defaultof<RenderTarget2D>
    let mutable camera = Unchecked.defaultof<FreeCamera>
    let mutable input = Unchecked.defaultof<Input>
    let mutable originalMouseState = Unchecked.defaultof<MouseState>
    let mutable perlinTexture3D = Unchecked.defaultof<Texture3D>
    let mutable minMaxTerrainHeight = Unchecked.defaultof<Vector2>
    let mutable axesHint = Unchecked.defaultof<VertexPositionColor[]>
    let mutable cubeTriangles = Unchecked.defaultof<VertexPositionNormalTexture[]>
    let mutable stoneWorldMatrices = Unchecked.defaultof<Matrix[]>
    do graphics.GraphicsProfile <- GraphicsProfile.HiDef
    do graphics.PreferredBackBufferWidth <- 900
    do graphics.PreferredBackBufferHeight <- 700
    do graphics.IsFullScreen <- false
    do graphics.ApplyChanges()
    do base.Content.RootDirectory <- "Content"

    let createTerrain =
        terrain <- Terrain 512
        do terrain.DeformCircularFaults 500 1.0f 20.0f 100.0f
        do terrain.Normalize 0.5f 2.0f
        do terrain.Stretch 2.0f
        do terrain.Normalize -5.0f 10.0f
        do terrain.FlattenAroundCenter 20.0f 80.0f 0.0f
        vertices <- GetVertices terrain
        indices <- GetIndices terrain.Size
        minMaxTerrainHeight <-
            let (min, max) = terrain.MinMax()
            new Vector2(min, max)

    override _this.Initialize() =
        device <- base.GraphicsDevice

        base.Initialize()
        ()

    override _this.LoadContent() =
        environment <- loadEnvironment

        createTerrain
        world <- Matrix.Identity
        projection <- Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, device.Viewport.AspectRatio, 1.0f, 5000.0f)

        effects <- loadEffects _this
        textures <- loadTextures _this

        let dir = Vector3(0.0f, 0.27f, -0.96f)
        dir.Normalize()
        initialLightDirection <- dir

        let pp = device.PresentationParameters
        hdrRenderTarget <- new RenderTarget2D(device, pp.BackBufferWidth, pp.BackBufferHeight, false, SurfaceFormat.HalfVector4, DepthFormat.Depth24)

        spriteBatch <- new SpriteBatch(device)

        let startPosition = Vector3(0.0f, 10.0f, -(single terrain.Size) / 8.0f)

        camera <- FreeCamera(startPosition, 0.0f, 0.0f)
        Mouse.SetPosition(_this.Window.ClientBounds.Width / 2, _this.Window.ClientBounds.Height / 2)
        originalMouseState <- Mouse.GetState()
        input <- Input(Keyboard.GetState(), Keyboard.GetState(), Mouse.GetState(), Mouse.GetState(), _this.Window, originalMouseState, 0, 0)

        debugVertices <-
            [|
                VertexPositionTexture(Vector3(-0.9f, 0.5f, 0.0f), new Vector2(0.0f, 0.0f));
                VertexPositionTexture(Vector3(-0.9f, 0.9f, 0.0f), new Vector2(0.0f, 1.0f));
                VertexPositionTexture(Vector3(-0.5f, 0.5f, 0.0f), new Vector2(1.0f, 0.0f));

                VertexPositionTexture(Vector3(-0.5f, 0.5f, 0.0f), new Vector2(1.0f, 0.0f));
                VertexPositionTexture(Vector3(-0.9f, 0.9f, 0.0f), new Vector2(0.0f, 1.0f));
                VertexPositionTexture(Vector3(-0.5f, 0.9f, 0.0f), new Vector2(1.0f, 1.0f));
            |]

        // perlin noise texture

        perlinTexture3D <- new Texture3D(device, 16, 16, 16, false, SurfaceFormat.Color)
        let random = new Random()

        let randomVectorColour x =
            let v = Vector3(single (random.NextDouble() * 2.0 - 1.0),
                            single (random.NextDouble() * 2.0 - 1.0),
                            single (random.NextDouble() * 2.0 - 1.0))
            v.Normalize()
            Color(v)

        let randomVectors = Array.init (16 * 16 * 16) randomVectorColour
        perlinTexture3D.SetData<Color>(randomVectors)

        sky <- new Sky(effects.SkyFromAtmosphere, environment, device)

        axesHint <- Stonehenge.AxesHint.vertices

        cubeTriangles <- Stonehenge.Stone.cubeTriangles
        stoneWorldMatrices <-
            Stonehenge.StoneList.stones
            |> Array.ofList
            |> Array.map Stonehenge.Stone.worldMatrixForStone

    override _this.Update(gameTime) =
        let time = float32 gameTime.TotalGameTime.TotalSeconds

        input <- input.Updated(Keyboard.GetState(), Mouse.GetState(), _this.Window)

        if input.Quit then _this.Exit()

        camera <- camera.Updated(input, time)

        view <- camera.ViewMatrix

        //if input.PageDown then lightDirection <- Vector3.Transform(lightDirection, Matrix.CreateRotationX(0.003f))
        //if input.PageUp then lightDirection <- Vector3.Transform(lightDirection, Matrix.CreateRotationX(-0.003f))

        lightDirection <- Vector3.Transform(initialLightDirection, Matrix.CreateRotationX(time * -MathHelper.TwoPi / 40.0f))

        do base.Update(gameTime)

    override _this.Draw(gameTime) =
        let time = (single gameTime.TotalGameTime.TotalMilliseconds) / 100.0f

        device.SetRenderTarget(hdrRenderTarget)

        do device.Clear(Color.Black)
        _this.DrawApartFromSky false view
        sky.DrawSkyDome world projection lightDirection camera view
        //_this.DrawDebug perlinTexture3D

        device.SetRenderTarget(null)

        let effect = effects.Hdr
        effect.CurrentTechnique <- effect.Techniques.["Plain"]

        spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, 
            SamplerState.LinearClamp, DepthStencilState.Default, 
            RasterizerState.CullNone, effect)
 
        spriteBatch.Draw(hdrRenderTarget, new Rectangle(0, 0, device.PresentationParameters.BackBufferWidth, device.PresentationParameters.BackBufferHeight), Color.White);
 
        spriteBatch.End();

        do base.Draw(gameTime)

    member _this.DrawApartFromSky x (viewMatrix: Matrix) =
        _this.DrawTerrain x viewMatrix
        _this.DrawStones viewMatrix
        //_this.DrawAxesHint viewMatrix

    member _this.DrawTerrain (x: bool) (viewMatrix: Matrix) =
        let effect = effects.GroundFromAtmosphere

        effect.CurrentTechnique <- effect.Techniques.["GroundFromAtmosphere"]
        effect.Parameters.["xWorld"].SetValue(world)
        effect.Parameters.["xView"].SetValue(viewMatrix)
        effect.Parameters.["xProjection"].SetValue(projection)
        effect.Parameters.["xCameraPosition"].SetValue(camera.Position)
        effect.Parameters.["xLightDirection"].SetValue(lightDirection)
        effect.Parameters.["xGrassTexture"].SetValue(textures.Grass)
        // effect.Parameters.["xRockTexture"].SetValue(textures.Rock)
        // effect.Parameters.["xSandTexture"].SetValue(textures.Sand)
        // effect.Parameters.["xSnowTexture"].SetValue(textures.Snow)
        effect.Parameters.["xAmbient"].SetValue(0.5f)
        effect.Parameters.["xMinMaxHeight"].SetValue(minMaxTerrainHeight)
        effect.Parameters.["xPerlinSize3D"].SetValue(15.0f)
        effect.Parameters.["xRandomTexture3D"].SetValue(perlinTexture3D)

        environment.Atmosphere.ApplyToEffect effect

        device.BlendState <- BlendState.Opaque

        effect.CurrentTechnique.Passes |> Seq.iter
            (fun pass ->
                pass.Apply()
                device.DrawUserIndexedPrimitives<VertexPositionNormalTexture>(PrimitiveType.TriangleList, vertices, 0, vertices.Length, indices, 0, indices.Length / 3)
            )
    
    member _this.DrawStones (viewMatrix: Matrix) =
        let effect = effects.GroundFromAtmosphere

        effect.CurrentTechnique <- effect.Techniques.["Rock"]
        effect.Parameters.["xView"].SetValue(viewMatrix)
        effect.Parameters.["xProjection"].SetValue(projection)
        effect.Parameters.["xCameraPosition"].SetValue(camera.Position)
        effect.Parameters.["xLightDirection"].SetValue(lightDirection)
        effect.Parameters.["xRockTexture"].SetValue(textures.Rock)
        effect.Parameters.["xAmbient"].SetValue(0.5f)
        effect.Parameters.["xMinMaxHeight"].SetValue(minMaxTerrainHeight)
        effect.Parameters.["xPerlinSize3D"].SetValue(15.0f)
        effect.Parameters.["xRandomTexture3D"].SetValue(perlinTexture3D)

        environment.Atmosphere.ApplyToEffect effect

        device.BlendState <- BlendState.Opaque

        stoneWorldMatrices
        |> Array.iter (fun wm ->
            effect.Parameters.["xWorld"].SetValue(wm)

            effect.CurrentTechnique.Passes |> Seq.iter
                (fun pass ->
                    pass.Apply()
                    device.DrawUserPrimitives<VertexPositionNormalTexture>(PrimitiveType.TriangleList, cubeTriangles, 0, cubeTriangles.Length / 3)
                ))

    member _this.DrawAxesHint (viewMatrix: Matrix) =
        let effect = effects.Effect

        effect.CurrentTechnique <- effect.Techniques.["ColouredOnly"]
        effect.Parameters.["xWorld"].SetValue(Matrix.CreateTranslation(10.0f * Vector3.UnitY))
        effect.Parameters.["xView"].SetValue(viewMatrix)
        effect.Parameters.["xProjection"].SetValue(projection)

        effect.CurrentTechnique.Passes |> Seq.iter
            (fun pass ->
                pass.Apply()
                device.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.LineList, axesHint, 0, axesHint.Length / 2)
            )

    member _this.DrawDebug (texture: Texture2D) =
        let effect = effects.Effect
        effect.CurrentTechnique <- effect.Techniques.["Debug"]
        effect.Parameters.["xDebugTexture"].SetValue(texture)

        effect.CurrentTechnique.Passes |> Seq.iter
            (fun pass ->
                pass.Apply()
                device.DrawUserPrimitives<VertexPositionTexture>(PrimitiveType.TriangleList, debugVertices, 0, debugVertices.Length / 3)
            )
