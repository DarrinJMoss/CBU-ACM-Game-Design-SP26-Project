class_name OneshotParticles
extends GPUParticles2D

# SCRIPT INFORMATION
#    CONTRIBUTORS:
#        Darrin
#
#    DESCRIPTION:
#        Script to handle particles that only go once.
#        They can be finniky to work with directly, so I'm leaving it to a system that spawns and despanws them when we need
#
#    CHANGELOG:
#        2/19/2026
#            Script made.

@export var particleTime : float = 1.0

func _init() -> void:
	self.emitting = false

func Prepare(pos : Vector2, rot : float = 0.0, zIdx : int = 0) -> void:
	self.global_position = pos
	self.z_index = zIdx
	self.z_as_relative = true
	self.emitting = false
	self.rotation = rot
	
	print("PARTICLES: " + str(self.global_position))

func EmitAndFree() -> void:
	self.emitting = true
	await get_tree().create_timer(particleTime).timeout
	self.queue_free()
