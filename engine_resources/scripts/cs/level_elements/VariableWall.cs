using Godot;
using System;

/// <summary>
/// A class that handles a variable sized collision shape based on the shape of the root TextureRect Node (configured in editor). 
/// Meant to be used by child classes rather than by itself.
/// </summary>
public partial class VariableWall : TextureRect
{

    [Export] protected CollisionShape2D  Hitbox;
    [Export] protected CollisionObject2D Body;
    [Export(PropertyHint.Layers2DPhysics)] uint mask  = 0b001;
    [Export(PropertyHint.Layers2DPhysics)] uint layer = 0b101;

    public override void _Ready()
    {
        Hitbox.Shape = GetSizedShape();
        Hitbox.Position = this.Size / 2.0f;
        Body.CollisionLayer = layer;
        Body.CollisionMask  = mask;
    }


    protected RectangleShape2D GetSizedShape()
    {
        RectangleShape2D shape = new RectangleShape2D();
        shape.Size = this.Size;
        return shape;
    }


}
