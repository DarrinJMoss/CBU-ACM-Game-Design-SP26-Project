extends Node2D
class_name BouncePads

@export var bounceAmount : Vector2 = Vector2.ZERO

const ANGLE_OFFSET : float = PI / 2.0

func _ready() -> void:
	bounceAmount = bounceAmount.abs()
	
	# Manually overriding the signs of the bounce velocities based on
	# the node's orientation due to some in the level not matching up.
	# Easier than manually editing each platform.
	bounceAmount.x *= sign(cos(self.rotation - ANGLE_OFFSET))
	bounceAmount.y *= sign(sin(self.rotation - ANGLE_OFFSET))
	
	$Label.text = str(bounceAmount)


func _on_area_2d_body_entered(body: Node2D) -> void:
	if body.has_method("ApplyBounceImpulse"):
		body.call("ApplyBounceImpulse", bounceAmount)
