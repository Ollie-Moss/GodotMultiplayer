using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

[Tool]
public partial class SpawnerComponent : Node2D
{
    private List<SpawnPointComponent> _spawnPoints = new List<SpawnPointComponent>();

    public override void _Ready()
    {
        foreach (SpawnPointComponent spawnPoint in GetTree().GetNodesInGroup("SpawnPoint"))
        {
            _spawnPoints.Add(spawnPoint);
        }
    }

    public SpawnPointComponent FindNextSpawnPoint()
    {
        foreach (SpawnPointComponent spawnPoint in _spawnPoints)
        {
            if (spawnPoint.canSpawn)
            {
                spawnPoint.canSpawn = false;
                return spawnPoint;
            }
        }
        return null;
    }







    public override string[] _GetConfigurationWarnings()
    {
        if (GetChildCount() > 0)
        {
            foreach (var child in GetChildren())
            {
                if(child.GetGroups().Count() == 0) return new string[] { "Please ensure that only SpawnPointComponents are used" };
                foreach (string group in child.GetGroups())
                {
                    if (group != "SpawnPoint")
                    {
                        return new string[] { "Please ensure that only SpawnPointComponents are used" };
                    }
                }
                
            }
            
        }
        else
        {
            return new string[] { "Must Contain SpawnpointComponents" };
        }
        return Array.Empty<string>();
    }

}
