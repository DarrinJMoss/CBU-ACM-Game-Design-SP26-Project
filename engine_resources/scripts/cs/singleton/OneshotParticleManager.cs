using Godot;
using System;
using System.IO;
using System.Text.RegularExpressions;

/* SCRIPT INFORMATION
    CONTRIBUTORS:
        Darrin

    DESCRIPTION:
        Global script that can be accessed by any .CS or .GD Script.

        For GD Script, just use OsPMan.(. . .) in any part of your script.
        For CS, use OneshotParticleManager.i.(. . .)

        Spawns a set one-shot particles defined in a scene for whatever effects you need.

    CHANGELOG:
        2/21/2026:
            Script created
*/

public partial class OneshotParticleManager : Node
{
    static public OneshotParticleManager i;

    public enum ParticleTypes
    {
        BOOST_EXPLOSION,
        LAND_JUMP,
    }

    public override void _Ready()
    {
        if (OneshotParticleManager.i == null)
        {
            Global.Log("OneshotParticles._Ready()", "engine_resources/scripts/cs/singleton/OneshotParticles.cs", "Created instance of OsParticles singleton.");
            OneshotParticleManager.i = this; 
        }
        else
        {
            Global.LogWarning("OneshotParticles._Ready()", "engine_resources/scripts/cs/singleton/OneshotParticles.cs", "Instance of OsParticles singleton already exists.");
            this.QueueFree();
        }
    }

    public void SpawnParticlesAt(ParticleTypes particleType, Vector2 pos, float rotation = 0.0f, int zIdx = 0)
    {
        _SpawnParticles(particleType, null, pos, rotation, zIdx);
    }
    public void SpawnParticleAsChild(ParticleTypes particleType, Node2D parent, Vector2 positionOffset, float rotation, int zIdx)
    {
        _SpawnParticles(particleType, parent, positionOffset, rotation, zIdx);
    }

    private void _SpawnParticles(ParticleTypes particleType, Node2D parent, Vector2 positionOffset, float rotation, int zIdx) 
    {
        GpuParticles2D toSpawn = _GetParticlesNode(particleType);

        if (toSpawn == null)
        {
            GD.PrintErr("ERR in OneshotParticleManager.cs | _SpawnParticles() | Could not find particles. ID: " + particleType.ToString());
            return;
        }

        if (parent != null)
        {
            toSpawn.Call("Prepare", positionOffset, rotation, zIdx);
            parent.AddChild(toSpawn);
            toSpawn.Call("EmitAndFree");
        } 
        else
        {
            toSpawn.Call("Prepare", positionOffset, rotation, zIdx);
            this.AddChild(toSpawn);   
            toSpawn.Call("EmitAndFree");
        }
    }

    private GpuParticles2D _GetParticlesNode(ParticleTypes particleType)
    {
        string path = "";
        switch (particleType)
        {
            case ParticleTypes.BOOST_EXPLOSION:
                path = "res://engine_resources/scenes/oneshot_particles/blast_particles.tscn";
                break;
            case ParticleTypes.LAND_JUMP:
                path = "res://engine_resources/scenes/oneshot_particles/landing_particles.tscn";
                break;
        }
        if (path == "")
        {
            GD.PrintErr("ERR In OneshotParticlesManager.cs | _GetParticlesNode() | Invalid/Not implemented particle. ID: " + particleType.ToString());
            return null;
        }

        try
        {
            return GD.Load<PackedScene>(path).Instantiate<GpuParticles2D>();
        }
        catch (InvalidCastException e)
        {
            GD.PrintErr("Exception (InvalidCastException) In OneshotParticlesManager.cs | _GetParticlesNode() | Could not cast particles of ID: " + particleType.ToString() + " to GPUParticles 2D. Path: <" + path + ">. Exception Message: " + e.Message);
        }
        catch (FileNotFoundException e)
        {
            GD.PrintErr("Exception (FileNotFoundException) In OneshotParticlesManager.cs | _GetParticlesNode() | Could not find patch for particle. ID: " + particleType.ToString() + " Attempted path: <" + path + ">. Exception Message: " + e.Message);
        }
        return null;
    }

}
