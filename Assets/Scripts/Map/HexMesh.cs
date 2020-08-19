using System.Collections.Generic;
using UnityEngine;
public class HexMesh
{
    private class Node
    {
        public Vector3 position;
        public int vertexIndex = -1;
        public List<Node> connected = new List<Node>();
        public bool renderingDone = false;

        public Node(Vector3 _pos)
        {
            position = _pos;
        }

        public Node(float x, float y, float z)
        {
            position = new Vector3(x, y, z);
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
        float baseY = hex.Elevation;
        if (baseY > 0)
        {
            baseY = Mathf.Pow(hex.Elevation, 2f);
        }
        
        Node hexCenter = new Node(0, baseY, 0);

        AddNodeToMainList(hexCenter);

        List<List<Node>> rings = new List<List<Node>>();

        int ringCount = 1;
        float maxRingSize = hexTriangleSide;
        List<Node> previousRing = null;

        for(int ringIndex = 0; ringIndex < ringCount; ringIndex++) {
            List<Node> ring = new List<Node>();
            rings.Add(ring);
            int segments = (ringIndex + 1);

            AddHexRing(hex, ring, segments, (float)segments/(float)ringCount * maxRingSize);
            
            for(int nodeIndex = 0; nodeIndex < ring.Count; nodeIndex++) {
                Node node = ring[nodeIndex];
                
                int nextIndex = nodeIndex + 1;
                if (nextIndex == ring.Count)
                {
                    nextIndex = 0;
                }

                AddNodeToMainList(node);
                if (ringIndex == 0) {
                    ConnectNodes(hexCenter, node);
                } else {
                    float fuzzyIndex = nodeIndex * (float)(segments-1) / (float)segments;
                    int lowerIndex = (int) Mathf.Floor(fuzzyIndex) % previousRing.Count;
                    int upperIndex = (int) Mathf.Ceil(fuzzyIndex) % previousRing.Count;
                    ConnectNodes(node, previousRing[lowerIndex]);
                    ConnectNodes(node, previousRing[upperIndex]);
                }

                ConnectNodes(node, ring[nextIndex]);
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
        Vector3 currentPoint = lineStart;

        if (lineSegments < 1) {
            return;
        }

        Vector3 difference = lineEnd - lineStart;

        for(int segmentIndex = 0; segmentIndex < lineSegments; segmentIndex++) {
            Node node = AddVertex(vertices, currentPoint, mainHex, antiClockwiseHex, oppositeHex, clockwiseHex);
            currentPoint = lineStart + (float)segmentIndex/(float)lineSegments * difference;
        }
    }

    private Node AddVertex(List<Node> vertices, Vector3 baseVector, Hex mainHex, Hex antiClockwiseHex, Hex oppositeHex, Hex clockwiseHex) {
        Node node = new Node(  baseVector.x + GetWeightedFloatParam(Hex.HEX_FLOAT_PARAMS.XOffset, baseVector, false, mainHex, antiClockwiseHex, oppositeHex, clockwiseHex), 
                                baseVector.y + GetWeightedFloatParam(Hex.HEX_FLOAT_PARAMS.Elevation, baseVector, true, mainHex, antiClockwiseHex, oppositeHex, clockwiseHex),
                                baseVector.z + GetWeightedFloatParam(Hex.HEX_FLOAT_PARAMS.ZOffset, baseVector, false, mainHex, antiClockwiseHex, oppositeHex, clockwiseHex));
        vertices.Add(node);

        return node;
    }

    private float GetWeightedFloatParam(Hex.HEX_FLOAT_PARAMS param, Vector3 relativePosition, bool squared, Hex mainHex, Hex antiClockwiseHex, Hex oppositeHex, Hex clockwiseHex) {
        float total = 0;
        float totalWeight = 0;
        float weight = 0;

        Vector3 absolutePosition = mainHex.PositionFromCamera(mainHex.Position()) + relativePosition;
        
        weight = GetHexWeight(mainHex, mainHex, absolutePosition);
        total += weight * mainHex.floatParams[param];
        totalWeight += weight;

        if (antiClockwiseHex != null) {
            weight = GetHexWeight(antiClockwiseHex, mainHex, absolutePosition);

            total += weight * antiClockwiseHex.floatParams[param];
            totalWeight += weight;
        }

        if (oppositeHex != null) {
            weight = GetHexWeight(oppositeHex, mainHex, absolutePosition);

            total += weight * oppositeHex.floatParams[param];
            totalWeight += weight;
        }

        if (clockwiseHex != null) {
            weight = GetHexWeight(clockwiseHex, mainHex, absolutePosition);

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