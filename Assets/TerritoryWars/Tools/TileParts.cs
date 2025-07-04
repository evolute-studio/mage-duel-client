﻿using System;
using System.Collections.Generic;
using TerritoryWars.General;
using TerritoryWars.Tile;
using TerritoryWars.Tools;
using TerritoryWars.UI;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;

public class TileParts : MonoBehaviour
{
    public List<HouseGameObject> Houses { get; private set; } = new List<HouseGameObject>();
    public List<SpriteRenderer> DecorationsRenderers { get; private set; } = new List<SpriteRenderer>();
    public List<SpriteRenderer> ArcRenderers = new List<SpriteRenderer>();
    public TerritoryFiller TileTerritoryFiller;
    public List<Area> Areas { get; private set; } = new List<Area>();
    public WallPlacer WallPlacer;
    public SpriteRenderer[] RoadRenderers = new SpriteRenderer[4];
    public GameObject Mill;
    public List<CloserToBorderFence> CloserToBorderFences = new List<CloserToBorderFence>();
    public Transform[] PinsPositions;
    public GameObject[] Forest;
    public GameObject Enviroment;
    public GameObject ContestedEnviroment;
    public PolygonCollider2D PolygonCollider2D;
    public FlagsOnWall FlagsOnWalls;
    public SpriteRenderer HangingGrass;
    public Material HangingGrassMaterial;
    public GameObject[] ArcsGameObjects;

    private GameObject WallParent;
    public GameObject[] CompletedBorderWalls;
    public List<GameObject> CompletedWalls;

    public SpriteRenderer GrassSpriteRenderer;

    public GameObject[] TransitionGrass;
    

    private int DefaultLayerMask = 0; // Default layer
    private int OutlineLayerMask = 31; // Outline layer
    private int GrayOutlineLayerMask = 32; // Gray layer


    public void Awake()
    {
        DefaultLayerMask = LayerMask.NameToLayer("Default");
        OutlineLayerMask = LayerMask.NameToLayer("Outline");
        GrayOutlineLayerMask = LayerMask.NameToLayer("GrayOutline");

        FlagsOnWalls = gameObject.GetComponentInChildren<FlagsOnWall>();
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
        
        // ContestedWalls
        Transform walls = transform.Find("Walls");
        if (walls == null)
        {
            return;
        }
        WallParent = walls.gameObject;
        CompletedBorderWalls = new GameObject[4];
        for (int i = 0; i < walls.childCount; i++)
        {
            GameObject child = walls.GetChild(i).gameObject;
            if (child.name == "BorderWalls")
            {
                for (int j = 0; j < child.transform.childCount; j++)
                {
                    GameObject borderWall = child.transform.GetChild(j).gameObject;
                    if (borderWall != null)
                    {
                        CompletedBorderWalls[j] = borderWall;
                        CompletedBorderWalls[j].SetActive(false);
                    } 
                }
            }
            else
            {
                CompletedWalls.Add(child);
            }
            
            
        }
        
        // Houses
        Transform houses = transform.Find("Houses");
        if (houses != null)
        {
            for (int i = 0; i < houses.childCount; i++)
            {
                GameObject house = houses.GetChild(i).gameObject;
                HouseGameObject houseGameObject = new HouseGameObject(house);
                Houses.Add(houseGameObject);
                house.GetComponent<SpriteRenderer>().enabled = false;
            }
        }
        
        // Decorations
        Transform decorations = transform.Find("Decorations");
        if (decorations != null)
        {
            for (int i = 0; i < decorations.childCount; i++)
            {
                SpriteRenderer decoration = decorations.GetChild(i).GetComponent<SpriteRenderer>();
                if (decoration != null)
                {
                    DecorationsRenderers.Add(decoration);
                }
            }
        }
        
        // ForestAreas
        ForestArea[] forestAreas = transform.GetComponentsInChildren<ForestArea>();
        if (forestAreas != null)
        {
            foreach (var forestArea in forestAreas)
            {
                Areas.Add(forestArea);
            }
        }
    }

    public void ClearAllHouses()
    {
        for(int i = 0; i < Houses.Count; i++)
        {
            Destroy(Houses[i].Parent);
        }
    }

    public void SetAllRenderersLayer(string layerName)
    {
        foreach (var house in Houses)
        {
            house.FlagSpriteRenderer.sortingLayerName = layerName;
            house.HouseSpriteRenderer.sortingLayerName = layerName;
        }
        foreach (var decoration in DecorationsRenderers)
        {
            decoration.sortingLayerName = layerName;
        }
        foreach (var arc in ArcRenderers)
        {
            arc.sortingLayerName = layerName;
        }
        if (TileTerritoryFiller != null && TileTerritoryFiller.currentTerritory != null &&
            TileTerritoryFiller.currentTerritory._spriteRenderer != null)
        {
            TileTerritoryFiller.currentTerritory._spriteRenderer.sortingLayerName = layerName;
        }
        
    }

    public void AddHouse(GameObject house)
    {
        if (house == null) return;
        HouseGameObject houseGameObject = new HouseGameObject(house);
            
        Houses.Add(houseGameObject);
    }

    public void SpawnTileObjects(bool isBoarderTile = false)
    {
        if (HangingGrass.sprite == null && HangingGrass != null)
        {
            HangingGrass.sprite = PrefabsManager.Instance.TileAssetsObject.GetHangingGrass();
            HangingGrass.material = HangingGrassMaterial;
        }
    }

    public void PlaceFlags(int rotation, int winner)
    {
        if (FlagsOnWalls == null) return;
        
        FlagsOnWalls.gameObject.SetActive(true);
        for (int i = 0; i < FlagsOnWalls.FlagsGO.Length; i++)
        {
            if (FlagsOnWalls.FlagsGO[i] == null)
            {
                continue;
            }
            FlagsOnWalls.FlagsGO[i].SetActive(i == rotation);
            SpriteRenderer[] flags = FlagsOnWalls.FlagsGO[i].GetComponentsInChildren<SpriteRenderer>();
            if (winner != 3)
            {
                foreach (var flag in flags)
                {
                    if (flag != null)
                    {
                        flag.sprite = PrefabsManager.Instance.TileAssetsObject
                            .GetFlagByReference(SetLocalPlayerData.GetLocalIndex(winner), flag.sprite);
                    }
                }
            }
        }
    }

    public void SetActiveWoodenBorderWall(bool isActive)
    {
        Transform borderFences = transform.Find("BorderFence");
        borderFences.gameObject.SetActive(isActive);
    }

    public void SetActiveWoodenArcs(bool isActive)
    {
        if (ArcsGameObjects != null)
        {
            foreach (var arc in ArcsGameObjects)
            {
                arc.SetActive(isActive);
            }
        }
    }

    public void PlaceContestedWalls(int rotation)
    {
        WallParent.SetActive(true);
        WallPlacer?.gameObject.SetActive(false);

        for (int i = 0; i < CompletedWalls.Count; i++)
        {
            if (CompletedWalls[i] == null)
            {
                continue;
            }
            CompletedWalls[i].SetActive(i == rotation);
        }

        PlaceBorderContestedWalls();
    }

    public void SetContestedBorderWalls(List<Side> sides)
    {
        foreach (var side in sides)
        {
            CompletedBorderWalls[(int)side].SetActive(true);
        }
        
    }
    
    private void PlaceBorderContestedWalls()
    {
        WallParent.transform.Find("BorderWalls").gameObject.SetActive(true);
    }
    
    public void HideForestAreas()
    {
        foreach (var area in Areas)
        {
            if (area is ForestArea forestArea)
            {
                forestArea.gameObject.SetActive(false);
            }
        }
    }
    

    public void CityOutline(bool isOutline, bool isGray = false)
    {
        int mask = isOutline ? OutlineLayerMask : DefaultLayerMask;
        mask = isGray ? GrayOutlineLayerMask : mask;
        
        if (Houses != null)
        {
            foreach (var house in Houses)
            {
                if (house != null) house.HouseSpriteRenderer.gameObject.layer = mask;
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
        if (CompletedWalls != null)
        {
            foreach (var wall in CompletedWalls)
            {
                if (wall != null) wall.layer = mask;
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

    [Serializable]
    public class HouseGameObject
    {
        public SpriteRenderer HouseSpriteRenderer { get; private set; }
        public SpriteRenderer FlagSpriteRenderer { get; private set; }
        public SpriteAnimator FlagAnimator{ get; private set; }

        public GameObject Parent;

        public HouseGameObject(GameObject house)
        {
            Parent = house;
        }

        public void SetData(GameObject house)
        {
            HouseSpriteRenderer = house.transform.Find("House").GetComponent<SpriteRenderer>();
            FlagSpriteRenderer = house.transform.Find("Flag").GetComponent<SpriteRenderer>();
            FlagAnimator = house.transform.Find("Flag").GetComponent<SpriteAnimator>();
        }
    }
}
