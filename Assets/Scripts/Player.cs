using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


public class Player
{
    public Player( string name )
    {
        PlayerName = name; 

        units = new HashSet<Unit>();
        cities = new HashSet<City>();
        structures = new HashSet<SurfaceStructure>();
    }

    public string PlayerName;

    public enum PlayerType { LOCAL, AI, REMOTE };
    public PlayerType Type = PlayerType.LOCAL;

    private HashSet<Unit> units;
    private HashSet<City> cities;
    private HashSet<SurfaceStructure> structures;

    public Unit[] Units {
        get { return units.ToArray(); }
    }
    public City[] Cities {
        get { return cities.ToArray(); }
    }
    public SurfaceStructure[] Structures {
        get { return structures.ToArray(); }
    }

    public void AddUnit( Unit u )
    {
        units.Add(u);
        u.OnObjectDestroyed += OnUnitDestroyed;
    }

    public void OnUnitDestroyed( MapObject mo )
    {
        units.Remove( (Unit)mo );
    }

    public void AddCity ( City c )
    {
        cities.Add(c);
        c.OnObjectDestroyed += OnCityDestroyed;
    }

    public void OnCityDestroyed( MapObject mo )
    {
        cities.Remove( (City)mo );
    }

    public void AddStructure ( SurfaceStructure c )
    {
        structures.Add(c);
        c.OnObjectDestroyed += OnStructureDestroyed;
    }

    public void OnStructureDestroyed( MapObject mo )
    {
        structures.Remove( (SurfaceStructure)mo );
    }

}

