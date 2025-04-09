using System;
using System.Collections.Generic;
using TerritoryWars.Tile;
using UnityEngine;

public class TileParts : MonoBehaviour
{
    public List<SpriteRenderer> HouseRenderers;
    public List<SpriteRenderer> ArcRenderers = new List<SpriteRenderer>();
    public TerritoryFiller TileTerritoryFiller;
    public WallPlacer WallPlacer;
    public SpriteRenderer[] RoadRenderers = new SpriteRenderer[4];
    public GameObject Mill;
    public List<CloserToBorderFence> CloserToBorderFences = new List<CloserToBorderFence>();
    public Transform[] PinsPositions;
    public GameObject[] Forest;
    public GameObject Enviroment;
    
    
    public void Awake()
    {
        // BorderFences
        Transform borderFences = transform.Find("BorderFence");
        if (borderFences != null)
        {
            CloserToBorderFences = new List<CloserToBorderFence>();
            for(int i = 0; i < borderFences.childCount; i++)
            {
                CloserToBorderFence sideFence = new CloserToBorderFence();
                sideFence.Side = (Side)i;
                sideFence.Fence = borderFences.GetChild(i).gameObject;
                sideFence.WallPlacer = sideFence.Fence.GetComponent<WallPlacer>();
                CloserToBorderFences.Add(sideFence);
            }
        }
        
        // WallPlacer
        Transform fence = transform.Find("Fence");
        if (fence != null)
        {
            WallPlacer = fence.GetComponent<WallPlacer>();
        }
    }
    public void ChangeRoadForContest()
    {
        
    }

    public void ChangeCityFenceForContest()
    {
        
    }

    [Serializable]
    public class CloserToBorderFence
    {
        public Side Side;
        public GameObject Fence;
        public WallPlacer WallPlacer;
    }
}
