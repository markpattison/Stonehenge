module Stonehenge.Stone

open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics

type StoneOrientation = Flat | Upright

type StoneLocation =
    { Radius: single
      Rotation: single
      Orientation: StoneOrientation
    }

let cubeTriangles =
    let _vertices : VertexPositionNormalTexture[] = Array.zeroCreate 36

    // Calculate the position of the vertices on the top face.
    let topLeftFront  = Vector3(-1.0f, 1.0f, -1.0f)
    let topLeftBack   = Vector3(-1.0f, 1.0f, 1.0f)
    let topRightFront = Vector3(1.0f, 1.0f, -1.0f)
    let topRightBack  = Vector3(1.0f, 1.0f, 1.0f)
 
    // Calculate the position of the vertices on the bottom face.
    let btmLeftFront  = Vector3(-1.0f, -1.0f, -1.0f)
    let btmLeftBack   = Vector3(-1.0f, -1.0f, 1.0f)
    let btmRightFront = Vector3(1.0f, -1.0f, -1.0f)
    let btmRightBack  = Vector3(1.0f, -1.0f, 1.0f)
 
    // Normal vectors for each face (needed for lighting / display)
    let normalFront  = Vector3(0.0f, 0.0f, 1.0f)
    let normalBack   = Vector3(0.0f, 0.0f, -1.0f)
    let normalTop    = Vector3(0.0f, 1.0f, 0.0f)
    let normalBottom = Vector3(0.0f, -1.0f, 0.0f)
    let normalLeft   = Vector3(-1.0f, 0.0f, 0.0f)
    let normalRight  = Vector3(1.0f, 0.0f, 0.0f)
 
    // UV texture coordinates
    let textureTopLeft = Vector2(1.0f, 0.0f)
    let textureTopRight = Vector2(0.0f, 0.0f)
    let textureBottomLeft = Vector2(1.0f, 1.0f)
    let textureBottomRight = Vector2(0.0f, 1.0f)
 
    // Add the vertices for the FRONT face.
    _vertices.[0] <- VertexPositionNormalTexture(topLeftFront, normalFront, textureTopLeft)
    _vertices.[1] <- VertexPositionNormalTexture(btmLeftFront, normalFront, textureBottomLeft)
    _vertices.[2] <- VertexPositionNormalTexture(topRightFront, normalFront, textureTopRight)
    _vertices.[3] <- VertexPositionNormalTexture(btmLeftFront, normalFront, textureBottomLeft)
    _vertices.[4] <- VertexPositionNormalTexture(btmRightFront, normalFront, textureBottomRight)
    _vertices.[5] <- VertexPositionNormalTexture(topRightFront, normalFront, textureTopRight)
 
    // Add the vertices for the BACK face.
    _vertices.[6] <- VertexPositionNormalTexture(topLeftBack, normalBack, textureTopRight)
    _vertices.[7] <- VertexPositionNormalTexture(topRightBack, normalBack, textureTopLeft)
    _vertices.[8] <- VertexPositionNormalTexture(btmLeftBack, normalBack, textureBottomRight)
    _vertices.[9] <- VertexPositionNormalTexture(btmLeftBack, normalBack, textureBottomRight)
    _vertices.[10] <- VertexPositionNormalTexture(topRightBack, normalBack, textureTopLeft)
    _vertices.[11] <- VertexPositionNormalTexture(btmRightBack, normalBack, textureBottomLeft)
 
    // Add the vertices for the TOP face.
    _vertices.[12] <- VertexPositionNormalTexture(topLeftFront, normalTop, textureBottomLeft)
    _vertices.[13] <- VertexPositionNormalTexture(topRightBack, normalTop, textureTopRight)
    _vertices.[14] <- VertexPositionNormalTexture(topLeftBack, normalTop, textureTopLeft)
    _vertices.[15] <- VertexPositionNormalTexture(topLeftFront, normalTop, textureBottomLeft)
    _vertices.[16] <- VertexPositionNormalTexture(topRightFront, normalTop, textureBottomRight)
    _vertices.[17] <- VertexPositionNormalTexture(topRightBack, normalTop, textureTopRight)
 
    // Add the vertices for the BOTTOM face. 
    _vertices.[18] <- VertexPositionNormalTexture(btmLeftFront, normalBottom, textureTopLeft)
    _vertices.[19] <- VertexPositionNormalTexture(btmLeftBack, normalBottom, textureBottomLeft)
    _vertices.[20] <- VertexPositionNormalTexture(btmRightBack, normalBottom, textureBottomRight)
    _vertices.[21] <- VertexPositionNormalTexture(btmLeftFront, normalBottom, textureTopLeft)
    _vertices.[22] <- VertexPositionNormalTexture(btmRightBack, normalBottom, textureBottomRight)
    _vertices.[23] <- VertexPositionNormalTexture(btmRightFront, normalBottom, textureTopRight)
 
    // Add the vertices for the LEFT face.
    _vertices.[24] <- VertexPositionNormalTexture(topLeftFront, normalLeft, textureTopRight)
    _vertices.[25] <- VertexPositionNormalTexture(btmLeftBack, normalLeft, textureBottomLeft)
    _vertices.[26] <- VertexPositionNormalTexture(btmLeftFront, normalLeft, textureBottomRight)
    _vertices.[27] <- VertexPositionNormalTexture(topLeftBack, normalLeft, textureTopLeft)
    _vertices.[28] <- VertexPositionNormalTexture(btmLeftBack, normalLeft, textureBottomLeft)
    _vertices.[29] <- VertexPositionNormalTexture(topLeftFront, normalLeft, textureTopRight)
 
    // Add the vertices for the RIGHT face. 
    _vertices.[30] <- VertexPositionNormalTexture(topRightFront, normalRight, textureTopLeft)
    _vertices.[31] <- VertexPositionNormalTexture(btmRightFront, normalRight, textureBottomLeft)
    _vertices.[32] <- VertexPositionNormalTexture(btmRightBack, normalRight, textureBottomRight)
    _vertices.[33] <- VertexPositionNormalTexture(topRightBack, normalRight, textureTopRight)
    _vertices.[34] <- VertexPositionNormalTexture(topRightFront, normalRight, textureTopLeft)
    _vertices.[35] <- VertexPositionNormalTexture(btmRightBack, normalRight, textureBottomRight)

    _vertices

let stoneLength = 2.0f
let stoneWidth = 0.5f

let worldMatrixForStone stone =
    let centerToZero = Matrix.CreateTranslation(-0.5f, -0.5f, -0.5f)
    let stretch, yOffset =
        match stone.Orientation with
        | Flat -> Matrix.CreateScale(stoneLength, stoneWidth, stoneWidth), stoneLength + stoneWidth / 2.0f
        | Upright -> Matrix.CreateScale(stoneWidth, stoneLength, stoneWidth), stoneLength / 2.0f
    let shiftY = Matrix.CreateTranslation(0.0f, yOffset, 0.0f)

    let shiftZ = Matrix.CreateTranslation(0.0f, 0.0f, stone.Radius)
    let rotation = Matrix.CreateRotationY(stone.Rotation / MathHelper.TwoPi)

    let world = centerToZero * stretch * shiftY * shiftZ * rotation
    
    world