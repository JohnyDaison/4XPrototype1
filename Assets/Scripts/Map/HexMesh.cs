using System;
using System.Collections.Generic;
using UnityEngine;
public class HexMesh
{
    private class Node : IComparable
    {
        public Vector3 position;
        public int vertexIndex = -1;
        public List<Node> connected = new List<Node>();
        public bool renderingDone = false;

        public float sortingAngle;

        public Node(Vector3 _pos)
        {
            position = _pos;
        }

        public Node(float x, float y, float z)
        {
            position = new Vector3(x, y, z);
        }

        public int CompareTo(System.Object obj)
        {
            if (obj == null) return 1;

            Node otherNode = obj as Node;

            if (otherNode != null) {
                return this.sortingAngle.CompareTo(otherNode.sortingAngle);
            } else {
                throw new ArgumentException("Object is not a Node");
            }
        }
    }

    private List<Node> vertices = new List<Node>();
    private List<int> triangles = new List<int>();


    public static float hexTriangleSide = 1f;
    public static float hexTriangleHeight = hexTriangleSide * Mathf.Sqrt(3) / 2f;

    private Vector3 topVertex = new Vector3(0, 0, 1f);
    private Vector3 topRightVertex = new Vector3(hexTriangleHeight, 0, 0.5f);
    private Vector3 bottomRightVertex = new Vector3(hexTriangleHeight, 0, -0.5f);
    private Vector3 bottomVertex = new Vector3(0, 0, -1f);
    private Vector3 bottomLeftVertex = new Vector3(-hexTriangleHeight, 0, -0.5f);
    private Vector3 topLeftVertex = new Vector3(-hexTriangleHeight, 0, 0.5f);


    public Mesh GenerateHexMesh(Hex hex)
    {
        Mesh mesh = new Mesh();

        Clear();

        CalculateHexNodes(hex);
        CreateMeshTriangles();

        mesh.vertices = GetVerticesForMesh();
        mesh.triangles = triangles.ToArray();

        mesh.RecalculateNormals();

        return mesh;
    }

    private void CreateMeshTriangles()
    {
        for (int vertexIndex = 0; vertexIndex < vertices.Count; vertexIndex++)
        {
            Node vertex = vertices[vertexIndex];
            List<Node> connected = vertex.connected;

            SortConnectedList(vertex, connected);

            for (int connectedIndex = 0; connectedIndex < connected.Count; connectedIndex++)
            {
                int nextIndex = connectedIndex + 1;
                if (nextIndex == connected.Count)
                {
                    nextIndex = 0;
                }

                if (!connected[connectedIndex].renderingDone && !connected[nextIndex].renderingDone)
                {
                    if (connected[connectedIndex].connected.Contains(connected[nextIndex])) {
                        AddTriangle(vertex, connected[connectedIndex], connected[nextIndex]);
                    }
                }
            }

            vertex.renderingDone = true;
        }
    }

    private void SortConnectedList(Node mainNode, List<Node> list) {
        list.ForEach((Node node) => {
            node.sortingAngle = GetSignedAngle(mainNode.position, node.position);
            if (node.sortingAngle < 0) {
                node.sortingAngle += 360;
            }
        });
        list.Sort();
    }

    private float GetSignedAngle(Vector3 main, Vector3 other) {
        Vector2 nodePos = new Vector2(other.x, other.z);
        Vector2 mainNodePos = new Vector2(main.x, main.z);
        return Vector2.SignedAngle(nodePos - mainNodePos, Vector2.up);
    }

    private void Clear()
    {
        vertices.Clear();
        triangles.Clear();
    }

    private void AddNodeToMainList(Node node) {
        node.vertexIndex = vertices.Count;
        vertices.Add(node);

    }

    private Vector3[] GetVerticesForMesh() {
        List<Vector3> result = new List<Vector3>();
        vertices.ForEach((Node node) => result.Add(node.position));

        return result.ToArray();
    }

    private void CalculateHexNodes(Hex hex)
    {
        HexMap map = hex.HexMap;
        Hex topLeft = map.GetHexAt(hex.Q - 1, hex.R + 1);
        Hex topRight = map.GetHexAt(hex.Q + 0, hex.R + 1);
        Hex right = map.GetHexAt(hex.Q + 1, hex.R);

        Node hexCenter = MakeWeightedNode(Vector3.zero, hex, topLeft, topRight, right);

        AddNodeToMainList(hexCenter);

        List<List<Node>> rings = new List<List<Node>>();

        int ringCount = 4;
        float maxRingSize = hexTriangleSide;
        List<Node> previousRing = null;

        for(int ringIndex = 0; ringIndex < ringCount; ringIndex++) {
            List<Node> ring = new List<Node>();
            rings.Add(ring);
            int segments = (ringIndex + 1);

            AddHexRing(hex, ring, segments, (float)segments/(float)ringCount * maxRingSize);
            
            for(int nodeIndex = 0; nodeIndex < ring.Count; nodeIndex++) {
                Node node = ring[nodeIndex];
                
                int prevIndex = nodeIndex - 1;
                if (prevIndex < 0)
                {
                    prevIndex = ring.Count - 1;
                }

                AddNodeToMainList(node);
                if (ringIndex == 0) {
                    ConnectNodes(hexCenter, node);
                } else {
                    float fuzzyIndex = (float)nodeIndex * (float)(segments-1) / (float)segments;
                    int lowerIndex = (int) Mathf.Floor(fuzzyIndex) % previousRing.Count;
                    int upperIndex = (int) Mathf.Ceil(fuzzyIndex) % previousRing.Count;
                    ConnectNodes(node, previousRing[lowerIndex]);
                    ConnectNodes(node, previousRing[upperIndex]);
                }

                ConnectNodes(ring[prevIndex], node);
            };

            previousRing = ring;
        }
    }

    private void ConnectNodes(Node node1, Node node2) {
        if (node1 == node2) {
            return;
        }

        if (!node1.connected.Contains(node2)) {
            node1.connected.Add(node2);
        }

        if (!node2.connected.Contains(node1)) {
            node2.connected.Add(node1);
        }
    }

    private void AddHexRing(Hex hex, List<Node> ring, int segments, float ringSize)
    {
        HexMap map = hex.HexMap;
        Hex topLeft = map.GetHexAt(hex.Q - 1, hex.R + 1);
        Hex topRight = map.GetHexAt(hex.Q + 0, hex.R + 1);
        Hex left = map.GetHexAt(hex.Q - 1, hex.R);
        Hex right = map.GetHexAt(hex.Q + 1, hex.R);
        Hex bottomLeft = map.GetHexAt(hex.Q + 0, hex.R - 1);
        Hex bottomRight = map.GetHexAt(hex.Q + 1, hex.R - 1);

        AddHexLine(ring, ringSize * topVertex,          ringSize * topRightVertex,      segments, hex, topLeft, topRight, right);
        AddHexLine(ring, ringSize * topRightVertex,     ringSize * bottomRightVertex,   segments, hex, topRight, right, bottomRight);
        AddHexLine(ring, ringSize * bottomRightVertex,  ringSize * bottomVertex,        segments, hex, right, bottomRight, bottomLeft);
        AddHexLine(ring, ringSize * bottomVertex,       ringSize * bottomLeftVertex,    segments, hex, bottomRight, bottomLeft, left);
        AddHexLine(ring, ringSize * bottomLeftVertex,   ringSize * topLeftVertex,       segments, hex, bottomLeft, left, topLeft);
        AddHexLine(ring, ringSize * topLeftVertex,      ringSize * topVertex,           segments, hex, left, topLeft, topRight);
    }

    private void AddHexLine(List<Node> vertices, Vector3 lineStart, Vector3 lineEnd, int lineSegments, Hex mainHex, Hex antiClockwiseHex, Hex oppositeHex, Hex clockwiseHex) {
        Vector3 currentPoint;

        if (lineSegments < 1) {
            return;
        }

        Vector3 difference = lineEnd - lineStart;

        for(int segmentIndex = 0; segmentIndex < lineSegments; segmentIndex++) {
            float lineProgression = (float)segmentIndex / (float)lineSegments;
            currentPoint = lineStart + lineProgression * difference;
            Node node = AddVertex(vertices, currentPoint, mainHex, antiClockwiseHex, oppositeHex, clockwiseHex);
        }
    }

    private Node AddVertex(List<Node> vertices, Vector3 baseVector, Hex mainHex, Hex antiClockwiseHex, Hex oppositeHex, Hex clockwiseHex) {
        Node node = MakeWeightedNode(baseVector, mainHex, antiClockwiseHex, oppositeHex, clockwiseHex);
        vertices.Add(node);

        return node;
    }

    private Node MakeWeightedNode(Vector3 baseVector, Hex mainHex, Hex antiClockwiseHex, Hex oppositeHex, Hex clockwiseHex) {

        return new Node(baseVector.x + GetNoiseSampleAtPosition(3, mainHex, baseVector),
                        baseVector.y + GetWeightedFloatParam(Hex.HEX_FLOAT_PARAMS.Elevation, baseVector, false, mainHex, antiClockwiseHex, oppositeHex, clockwiseHex),
                        baseVector.z + GetNoiseSampleAtPosition(4, mainHex, baseVector) );
    }

    private float GetNoiseSampleAtPosition(int noiseIndex, Hex hex, Vector3 baseVector) {
        Vector3 absolutePosition = hex.Position() + baseVector;

        HexMap_Continent hexMap = hex.HexMap as HexMap_Continent;

        return hexMap.SampleNoiseType(hexMap.hexNoiseTypes[noiseIndex], absolutePosition.x, absolutePosition.z);
    }

    private float GetWeightedFloatParam(Hex.HEX_FLOAT_PARAMS param, Vector3 baseVector, bool squared, Hex mainHex, Hex antiClockwiseHex, Hex oppositeHex, Hex clockwiseHex) {
        float total = 0;
        float totalWeight = 0;
        float weight = 0;

        Vector3 relativePosition = mainHex.PositionFromCamera(mainHex.Position()) + baseVector;
        
        weight = GetHexWeight(mainHex, mainHex, relativePosition);
        total += weight * mainHex.floatParams[param];
        totalWeight += weight;

        if (antiClockwiseHex != null) {
            weight = GetHexWeight(antiClockwiseHex, mainHex, relativePosition);

            total += weight * antiClockwiseHex.floatParams[param];
            totalWeight += weight;
        }

        if (oppositeHex != null) {
            weight = GetHexWeight(oppositeHex, mainHex, relativePosition);

            total += weight * oppositeHex.floatParams[param];
            totalWeight += weight;
        }

        if (clockwiseHex != null) {
            weight = GetHexWeight(clockwiseHex, mainHex, relativePosition);

            total += weight * clockwiseHex.floatParams[param];
            totalWeight += weight;
        }
        

        float result = total / totalWeight;

        if (squared && result > 0) {
            result = Mathf.Pow(result, 2f);
        }

        return result;
    }

    private float GetHexWeight(Hex hex, Hex mainHex, Vector3 position) {
        float distance = (hex.PositionFromCamera(mainHex.Position()) - position).magnitude;
        float weight = Mathf.Clamp(2f - distance / hexTriangleSide, 0, 1f);

        return weight;
    }

    private void AddTriangle(Node node1, Node node2, Node node3) {
        triangles.Add(node1.vertexIndex);
        triangles.Add(node2.vertexIndex);
        triangles.Add(node3.vertexIndex);
    }
}