using System;
using System.Collections.Generic;

[Serializable]
public class Route
{
    public List<Node> NodeList;
    public float CameraY;

    public Route()
    {
        NodeList = new List<Node>();
        CameraY = 0;
    }
}