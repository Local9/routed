using System;
using System.Collections.Generic;
[System.Serializable]
public class Node
{
    public string NodeName;
    public DateTime NodeUpdateTime;
    public List<string> NodeSigList;
    public Node(string name)
    {
        NodeName = name;
        NodeUpdateTime = DateTime.Now;
        NodeSigList = new List<string>();
    }
}