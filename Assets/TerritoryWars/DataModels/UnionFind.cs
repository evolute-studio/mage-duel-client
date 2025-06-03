using System;
using System.Collections.Generic;
using TerritoryWars.Tools;
using UnityEngine;

namespace TerritoryWars.DataModels
{
    [Serializable]
    public struct UnionFind
    {
        public bool IsNull => string.IsNullOrEmpty(BoardId);
        public string BoardId;
        public Dictionary<(Vector2Int, Side), Structure> Structures;

        public UnionFind SetData(evolute_duel_UnionFind data)
        {
            BoardId = data.board_id.Hex();
            Structures = new Dictionary<(Vector2Int, Side), Structure>();

            List<StructureNode> structureNodes = GetStructureNodes(data);
            Structures = GetStructure(structureNodes);

            return this;

        }

        private List<StructureNode> GetStructureNodes(evolute_duel_UnionFind data)
        {
            List<StructureNode> structureNodes = new List<StructureNode>();
            for (int i = 0; i < data.nodes_parents.Length; i++)
            {
                var (parentPosition, parentSide) = GameConfiguration.GetPositionAndSide(data.nodes_parents[i]);
                var (position, side) = GameConfiguration.GetPositionAndSide((byte)i);
                StructureNode node = new StructureNode
                {
                    ParentPosition = parentPosition,
                    ParentSide = parentSide,
                    Position = position,
                    Side = side,
                    Points = new[] { data.nodes_blue_points[i], data.nodes_red_points[i] },
                    OpenEdges = data.nodes_open_edges[i],
                    Contested = data.nodes_contested[i],
                    Type = (StructureType)data.nodes_types[i]
                };
                structureNodes.Add(node);
            }
            return structureNodes;
        }

        private Dictionary<(Vector2Int, Side), Structure> GetStructure(List<StructureNode> nodes)
        {
            Dictionary<(Vector2Int, Side), Structure> structures = new Dictionary<(Vector2Int, Side), Structure>();
            foreach (var node in nodes)
            {
                var root = GetNodeRoot(nodes, node);
                if (!structures.ContainsKey((root.Position, root.Side)))
                {
                    Structure structure = new Structure(root.Type);
                    structure.SetParent(root);
                    structures.Add((root.Position, root.Side), structure);
                }
                structures[(root.Position, root.Side)].AddNode(node);
            }
            return structures;
        }

        private StructureNode 
            GetNodeRoot(List<StructureNode> nodes, StructureNode currentNode)
        {
            if (currentNode.Position == currentNode.ParentPosition && currentNode.Side == currentNode.ParentSide)
            {
                return currentNode;
            }

            var parentPosition = currentNode.ParentPosition;
            var parentSide = currentNode.ParentSide;
            foreach (var node in nodes)
            {
                if (node.Position == parentPosition && node.Side == parentSide)
                {
                    return GetNodeRoot(nodes, node);
                }
            }

            return currentNode;
        }

        public Structure GetStructureByNode(Vector2Int position, Side side)
        {
            foreach (var structure in Structures)
            {
                if(structure.Value.ContainsNode(position, side))
                {
                    return structure.Value;
                }
            }
            CustomLogger.LogError($"No structure found for position {position} and side {side}");
            return new Structure(StructureType.City) { Parent = new StructureNode { Position = position, Side = side } };
        }
        
        public List<Structure> GetStructures()
        {
            List<Structure> result = new List<Structure>();
            foreach (var structure in Structures.Values)
            {
                result.Add(structure);
            }
            return result;
        }
    }



    [Serializable]
    public struct Structure
    {
        public StructureType Type;
        public StructureNode Parent;

        public Vector2Int Position => Parent.Position;
        public Side Side => Parent.Side;
        public ushort[] Points => Parent.Points;
        public byte OpenEdges => Parent.OpenEdges;
        public bool Contested => Parent.Contested;

        public List<StructureNode> Nodes;

        public Structure(StructureType type)
        {
            Type = type;
            Parent = new StructureNode();
            Nodes = new List<StructureNode>();
        }

        public void SetParent(StructureNode parent)
        {
            Parent = parent;
        }

        public void AddNode(StructureNode node)
        {
            Nodes.Add(node);
        }

        public bool ContainsNode(Vector2Int position, Side side)
        {
            foreach (var node in Nodes)
            {
                if (node.Position == position && node.Side == side)
                {
                    return true;
                }
            }
            return false;
        }
    }

    [Serializable]
    public struct StructureNode
    {
        public Vector2Int ParentPosition;
        public Side ParentSide;
        public Vector2Int Position;
        public Side Side;
        public ushort[] Points;
        public byte OpenEdges;
        public bool Contested;
        public StructureType Type;

    }

    public enum StructureType
    {
        None = 0,
        City = 1,
        Road = 2,
    }
}
