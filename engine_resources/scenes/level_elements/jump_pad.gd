extends Node2D
class_name BouncePads



@export var bounceAmount : Vector2 = Vector2.ZERO



func _on_area_2d_body_entered(body: Node2D) -> void:
	if body.has_method("ApplyBounceImpulse"):
		body.call("ApplyBounceImpulse", bounceAmount)
