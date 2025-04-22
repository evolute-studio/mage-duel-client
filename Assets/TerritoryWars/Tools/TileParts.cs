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
    public GameObject ContestedEnviroment;
    public PolygonCollider2D PolygonCollider2D;

    private int DefaultLayerMask = 0; // Default layer
    private int OutlineLayerMask = 31; // Outline layer
    private int GrayOutlineLayerMask = 32; // Gray layer


    public void Awake()
    {
        DefaultLayerMask = LayerMask.NameToLayer("Default");
        OutlineLayerMask = LayerMask.NameToLayer("Outline");
        GrayOutlineLayerMask = LayerMask.NameToLayer("GrayOutline");
        // BorderFences
        Transform borderFences = transform.Find("BorderFence");
        if (borderFences != null)
        {
            CloserToBorderFences = new List<CloserToBorderFence>();
            for (int i = 0; i < borderFences.childCount; i++)
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

    public void ChangeEnvironmentForContest()
    {
        if (ContestedEnviroment != null)
        {
            ContestedEnviroment.SetActive(true);
            if (HouseRenderers.Count == 4)
            {
                foreach (var house in HouseRenderers)
                {
                    house.gameObject.SetActive(false);
                }
            }
        }
    }

    public void CityOutline(bool isOutline, bool isGray = false)
    {
        int mask = isOutline ? OutlineLayerMask : DefaultLayerMask;
        mask = isGray ? GrayOutlineLayerMask : mask;
        
        if (HouseRenderers != null)
        {
            foreach (var house in HouseRenderers)
            {
                if (house != null) house.gameObject.layer = mask;
            }
        }

        if (ArcRenderers != null)
        {
            foreach (var arc in ArcRenderers)
            {
                if (arc != null) arc.gameObject.layer = mask;
            }
        }

        if (TileTerritoryFiller != null && TileTerritoryFiller.currentTerritory != null &&
            TileTerritoryFiller.currentTerritory._spriteRenderer != null)
        {
            TileTerritoryFiller.currentTerritory._spriteRenderer.gameObject.layer = mask;
        }

        if (WallPlacer != null)
        {
            foreach (var pillar in WallPlacer.GetPillars())
            {
                if (pillar != null) pillar.gameObject.layer = mask;
            }

            foreach (var wall in WallPlacer.GetWallSegments())
            {
                if (wall != null) wall.gameObject.layer = mask;
            }
        }

        if (CloserToBorderFences != null)
        {
            foreach (var border in CloserToBorderFences)
            {
                if (border != null && border.WallPlacer != null)
                {
                    foreach (var house in border.WallPlacer.GetPillars())
                    {
                        if (house != null) house.gameObject.layer = mask;
                    }

                    foreach (var wall in border.WallPlacer.GetWallSegments())
                    {
                        if (wall != null) wall.gameObject.layer = mask;
                    }
                }
            }
        }
    }

    public void RoadOutline(bool isOutline, Side side = Side.None, bool isGray = false)
    {
        int mask = isOutline ? OutlineLayerMask : DefaultLayerMask;
        mask = isGray ? GrayOutlineLayerMask : mask;
        for (int i = 0; i < RoadRenderers.Length; i++)
        {
            if (RoadRenderers[i] != null)
            {
                if (side == Side.None || side == (Side)i)
                {
                    RoadRenderers[i].gameObject.layer = mask;
                }
            }
        }

        if (Mill != null)
        {
            Mill.layer = mask;
        }

        if (ArcRenderers != null)
        {
            foreach (var arc in ArcRenderers)
            {
                if (arc != null) arc.gameObject.layer = mask;
            }
        }
    }

    public Side GetRoadSideByObject(GameObject go)
    {
        for (int i = 0; i < RoadRenderers.Length; i++)
        {
            if (RoadRenderers[i] != null && RoadRenderers[i].gameObject == go)
            {
                return (Side)i;
            }
        }

        return Side.None;
    }

    public int GetRoadCount()
    {
        int count = 0;
        for (int i = 0; i < RoadRenderers.Length; i++)
        {
            if (RoadRenderers[i] != null)
            {
                count++;
            }
        }

        return count;
    }

    public void DisableOutline()
    {
        CityOutline(false);
        RoadOutline(false);
    }

    [Serializable]
    public class CloserToBorderFence
    {
        public Side Side;
        public GameObject Fence;
        public WallPlacer WallPlacer;
    }
}
