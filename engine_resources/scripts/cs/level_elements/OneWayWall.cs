using Godot;
using System;

public partial class OneWayWall : VariableWall
{

    const int ARROW_SPACING = 16;

    public enum OneWayDirections
    {
        TOP,
        BOTTOM,
        LEFT,
        RIGHT
    }

    [Export] Texture2D arrowTx;

    [Export] public OneWayDirections blockedSide = OneWayDirections.TOP;

    public override void _Ready()
    {
        base._Ready();
        if (blockedSide == OneWayDirections.RIGHT)
        {
            Hitbox.Rotation = Mathf.DegToRad(90);
            
            RectangleShape2D flippedShape = new RectangleShape2D();
            flippedShape.Size = new Vector2(this.Size.Y, this.Size.X);
            Hitbox.Shape = flippedShape;
        }
        else if (blockedSide == OneWayDirections.BOTTOM)
        {
            Hitbox.Rotation = Mathf.DegToRad(180);
        }
        else if (blockedSide == OneWayDirections.LEFT)
        {
            Hitbox.Rotation = Mathf.DegToRad(270);

            RectangleShape2D flippedShape = new RectangleShape2D();
            flippedShape.Size = new Vector2(this.Size.Y, this.Size.X);
            Hitbox.Shape = flippedShape;
        }

        Vector2I arrowAmnt = new Vector2I(Mathf.Max(1, (int)this.Size.X / ARROW_SPACING), Mathf.Max(1, (int)this.Size.Y / ARROW_SPACING));
        Vector2 cursorPos;
        Vector2 initOff = Vector2.Zero;
        if (arrowAmnt.X % 2 == 1)
        {
            initOff.X = -ARROW_SPACING / 2;
        }
        if (arrowAmnt.Y % 2 == 1)
        {
            initOff.Y = -ARROW_SPACING / 2;
        }

        cursorPos = initOff - (ARROW_SPACING * (arrowAmnt / 2)) + this.Size / 2.0f + (Vector2.One * ARROW_SPACING / 2.0f);
        Vector2 basePos = cursorPos;

        GD.Print(arrowAmnt);

        for (int x = 0; x < arrowAmnt.X; x++)
        {
            cursorPos.Y = basePos.Y;
            for (int y = 0; y < arrowAmnt.Y; y++)
            {
                Sprite2D arrowSprite = new Sprite2D();
                arrowSprite.Texture = arrowTx;
                arrowSprite.Rotation = Hitbox.Rotation;
                arrowSprite.Position = cursorPos;
                arrowSprite.ShowBehindParent = false;
                this.AddChild(arrowSprite);

                cursorPos.Y += ARROW_SPACING;
            }
            cursorPos.X += ARROW_SPACING;
        }

    }



}
