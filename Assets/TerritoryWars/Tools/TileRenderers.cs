using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using TerritoryWars.General;
using TerritoryWars.Tile;
using UnityEngine;

public class TileRenderers : MonoBehaviour
{
    public List<SpriteRenderer> HouseRenderers;
    public List<SpriteRenderer> ArcRenderers;
    public TerritoryFiller TileTerritoryFiller;
    public FencePlacer TileFencePlacer;
    public FencePlacer TileFencePlacerContested;
    public SpriteRenderer RoadRenderers;
    public SpriteRenderer RoadContestRenderer;
    public GameObject Mill;
    public List<CloserToBorderFence> CloserToBorderFences;

    public Transform[] PinsPositions;
    public GameObject[] Forest;
    public GameObject Enviroment;

    public void ChangeRoadForContest()
    {
        
    }

    public void ChangeCityFenceForContest()
    {
        // TileFencePlacer.gameObject.SetActive(false);
        // TileFencePlacerContested.gameObject.SetActive(true);
        // TileFencePlacerContested.PlaceFence();
        //
    }

    [Serializable]
    public class CloserToBorderFence
    {
        public Side Side;
        public GameObject Fence;
    }
}
