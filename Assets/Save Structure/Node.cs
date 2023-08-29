using System;
using System.Collections.Generic;

[Serializable]
public class Node
{
    public string Name;
    public DateTime Update;
    public List<string> SigList;

    public Node(string name)
    {
        Name = name;
        Update = DateTime.Now;
        SigList = new List<string>();
    }
}