extends CanvasLayer


# Called when the node enters the scene tree for the first time.
func _ready() -> void:
    
    var anim : AnimationPlayer = $AnimationPlayer

    anim.play("DoIt")

    await anim.animation_finished

    get_tree().change_scene_to_file("res://engine_resources/scenes/title.tscn")

func _input(_event: InputEvent) -> void:
    if Input.is_action_just_pressed("SkipIntro"):
        get_tree().change_scene_to_file("res://engine_resources/scenes/title.tscn")
