using Godot;

[GlobalClass]
public partial class TextButtonBehavior : Resource
{

    /// <summary>
    /// Reference to the associated TextButton node.
    /// </summary>
    protected TextButton ParentButtonRef;

    /// <summary>
    /// Establish a two-way connection with the button from the behavior to allow the behavior
    /// to directly access the player button node.
    /// </summary>
    /// <param name="parentButton"></param>
    public void Register(TextButton parentButton)
    {
        ParentButtonRef = parentButton;
    }
    /// <summary>
    /// Button behavior when the associated TextButton is pressed.
    /// </summary>
    public virtual void Pressed()
    {
        GD.PrintErr("ERR in TextButton.cs | TextButtonBehavior::Pressed() | Ui button Pressed() method not overrided OR TextButton given base TextButtonBehavior resource. Please use a specialized resource insted.");
    }
    /// <summary>
    /// Button behavior when the associated TextButton has the mouse enter it
    /// </summary>
    public virtual void Hovered()
    {
        GD.PrintErr("ERR in TextButton.cs | TextButtonBehavior::Hovered() | Ui button Hovered() method not overrided OR TextButton given base TextButtonBehavior resource. Please use a specialized resource insted.");
    }
    /// <summary>
    /// Button behavior when the associated TextButton has the mouse exit it
    /// </summary>
    public virtual void UnHovered()
    {
        GD.PrintErr("ERR in TextButton.cs | TextButtonBehavior::UnHovered() | Ui button UnHovered() method not overrided OR TextButton given base TextButtonBehavior resource. Please use a specialized resource insted.");
    }
    /// <summary>
    /// Use this to get a node since resources cannot have direct access to nodes.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="path"></param>
    /// <returns></returns>
    protected T GetNodeFromPath<T>(NodePath path) where T : Node
    {
        return ParentButtonRef.GetNode<T>(path);
    }
    
}
