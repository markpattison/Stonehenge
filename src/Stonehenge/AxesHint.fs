module Stonehenge.AxesHint

open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics

let vertices =
    [| VertexPositionColor(Vector3.Zero, Color.Red)
       VertexPositionColor(Vector3.UnitX, Color.Red)
       VertexPositionColor(Vector3.Zero, Color.Green)
       VertexPositionColor(Vector3.UnitY, Color.Green)
       VertexPositionColor(Vector3.Zero, Color.Blue)
       VertexPositionColor(Vector3.UnitZ, Color.Blue)
    |]
