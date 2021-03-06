﻿module Stonehenge.ContentLoader

open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics

open Stonehenge.EnvironmentParameters

type Effects =
    {
        Effect: Effect
        Hdr: Effect
        SkyFromAtmosphere: Effect
        GroundFromAtmosphere: Effect
    }

type Textures =
    {
        Grass: Texture2D;
        Rock: Texture2D;
        Sand: Texture2D;
        Snow: Texture2D;
    }

let loadEffects (game: Game) =
    {
        Effect = game.Content.Load<Effect>("Effects/effects")
        Hdr = game.Content.Load<Effect>("Effects/hdr")
        SkyFromAtmosphere = game.Content.Load<Effect>("Effects/skyFromAtmosphere")
        GroundFromAtmosphere = game.Content.Load<Effect>("Effects/groundFromAtmosphere")
    }

let loadTextures (game: Game) =
    {
        Grass = game.Content.Load<Texture2D>("Textures/grass")
        Rock = game.Content.Load<Texture2D>("Textures/rock")
        Sand = game.Content.Load<Texture2D>("Textures/sand")
        Snow = game.Content.Load<Texture2D>("Textures/snow")
    }

let loadEnvironment =
    {
        Atmosphere =
            {
                InnerRadius = 100000.0f;
                OuterRadius = 102500.0f;
                ScaleDepth = 0.25f;
                KR = 0.0025f;
                KM = 0.0010f;
                ESun = 20.0f;
                G = -0.95f;
                Wavelengths = Vector3(0.650f, 0.570f, 0.440f);
            }
    }
