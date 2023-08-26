using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.SceneManagement;

public class NodeFrame : MonoBehaviour
{
    [SerializeField] public TextMeshPro NameTextComponent;
    [SerializeField] public TextMeshPro TimerTextComponent;
    [SerializeField] public TextMeshPro ScanTextComponent;
    [SerializeField] public string Name;
    [SerializeField] public DateTime Time;
    private GameManager gameManager;
    void Start()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        UpdateUIText(Name);
    }
    void Update()
    {
        float nodeTime = Convert.ToSingle(Math.Round((DateTime.Now - Time).TotalMinutes));
        if (nodeTime < 60) TimerTextComponent.text = Mathf.Round(nodeTime) + "m";
        else if (nodeTime < 1440) TimerTextComponent.text = Mathf.Round(nodeTime / 60) + "h";
        else TimerTextComponent.text = Mathf.Round(nodeTime / 1440) + "d";
    }
    public void RemoveSigs()
    {
        foreach (Node node in gameManager.WorkingRoute.NodeList)
        {
            if (node.NodeName == Name)
            {
                node.NodeSigList.Clear();
                node.NodeUpdateTime = DateTime.Now;
                Serializer.SaveData(gameManager.WorkingRoute);
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                return;
            }
        }
    }
    public void UpdateSigs()
    {
        Debug.Log("updating sigs");
        // import sigs here and apply to list
        var NewStrings = new List<string>(GUIUtility.systemCopyBuffer.Split('\n'));
        if (NewStrings == null) return;
        if (NewStrings.Count == 0) return;
        if (NewStrings[0][3] != '-') return;
        foreach (Node node in GameObject.Find("GameManager").GetComponent<GameManager>().WorkingRoute.NodeList)
        {
            if (node.NodeName == Name)
            {
                Debug.Log("node name match");
                // Compare old sigs to new, update old list
                for (var i = 0; i < NewStrings.Count; i ++)
                {
                    for (var j = 0; j < node.NodeSigList.Count; j ++)
                    {
                        if (NewStrings[i].Substring(0, 7) == node.NodeSigList[j].Substring(0, 7))
                        {
                            Debug.Log("existing sig found");
                            if (NewStrings[i].Length < node.NodeSigList[j].Length)
                            {
                                Debug.Log("replacing new sig with further progressed old sig");
                                NewStrings[i] = node.NodeSigList[j];
                            }
                        }
                    }
                }
                Debug.Log("updated sigs");
                node.NodeSigList = NewStrings;
                node.NodeUpdateTime = DateTime.Now;
                gameManager.WorkingRoute.CameraY = transform.position.y;
                var camPos = Camera.main.transform.position;
                Camera.main.transform.position = new Vector3(camPos.x, transform.position.y, camPos.z);
                gameManager.ClampCamera();
                Serializer.SaveData(gameManager.WorkingRoute);
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                return;
            }
        }
    }
    void UpdateUIText(string nodeName)
    {
        List<string> strings = new List<string>();
        var nodeList = GameObject.Find("GameManager").GetComponent<GameManager>().WorkingRoute.NodeList;
        foreach (Node node in nodeList)
        {
            if (nodeName == node.NodeName)
            {
                strings = node.NodeSigList;
            }
        }

        NameTextComponent.text = Name;

        // Convert list of raw sigs to human-readable format
        var total = 0;
        var solved = 0;
        var unsolved = 0;
        var targets = 0;
        ScanTextComponent.text = "";
        var tempString = "";
        foreach (string sig in strings)
        {
            /*displayString += "• ";
            displayString += sig.Substring(0, 3) + " ";
            if (sig.Contains("Covert Research")) displayString += "→ GHOST";
            else if (sig.Contains("Sleeper Cache")) displayString += "→ SLEEPER";
            else if (sig.Contains("Nebula")) displayString += "→ NEBULA";
            else if (sig.Contains("Data Site")) displayString += "";
            else if (sig.Contains("Relic Site")) displayString += "";
            else if (sig.Contains("Wormhole")) displayString += "";
            else if (sig.Contains("Combat Site")) displayString += "";
            else displayString += "— ???";
            displayString += "\n";*/
            total ++;
            if (sig.Contains("Covert Research")) 
            {
                tempString += sig.Substring(0, 3) + " [GS], ";
                targets ++; solved ++;
            }
            else if (sig.Contains("Sleeper Cache"))
            {
                tempString += sig.Substring(0, 3) + " [SC], ";
                targets ++; solved ++;
                }
            else if (sig.Contains("Nebula")) {
                //{tempString+= sig.Substring(0, 3) + " [nebu], ";
                if (sig.Contains("Sister")) {tempString+= sig.Substring(0, 3) + " [LiM20], "; targets ++;}
                else if (sig.Contains("Helix")) {tempString+= sig.Substring(0, 3) + " [LiM60], "; targets ++;}
                else if (sig.Contains("Wild")) {tempString+= sig.Substring(0, 3) + " [MaM20], "; targets ++;}
                else if (sig.Contains("Blackeye")) {tempString+= sig.Substring(0, 3) + " [MaM60], "; targets ++;}
                else if (sig.Contains("Sunspark")) {tempString+= sig.Substring(0, 3) + " [AmM20], "; targets ++;}
                else if (sig.Contains("Diablo")) {tempString+= sig.Substring(0, 3) + " [AmM60], "; targets ++;}
                else if (sig.Contains("Smoking")) {tempString+= sig.Substring(0, 3) + " [GoM20], "; targets ++;}
                else if (sig.Contains("Ring")) {tempString+= sig.Substring(0, 3) + " [GoM60], "; targets ++;}
                else if (sig.Contains("Calabash")) {tempString+= sig.Substring(0, 3) + " [CeM20], "; targets ++;}
                else if (sig.Contains("Glass")) {tempString+= sig.Substring(0, 3) + " [CeM60], "; targets ++;}
                else if (sig.Contains("Bright")) {tempString+= sig.Substring(0, 3) + " [ViM20], "; targets ++;}
                else if (sig.Contains("Sparking")) {tempString+= sig.Substring(0, 3) + " [ViM60], "; targets ++;}
                else if (sig.Contains("Ghost")) {tempString+= sig.Substring(0, 3) + " [AzM20], "; targets ++;}
                else if (sig.Contains("Eagle")) {tempString+= sig.Substring(0, 3) + " [AzM60], "; targets ++;}
                else if (sig.Contains("Flame")) {tempString+= sig.Substring(0, 3) + " [VeM20], "; targets ++;}
                else if (sig.Contains("Pipe")) {tempString+= sig.Substring(0, 3) + " [VeM60], "; targets ++;}
                /*else if (sig.Contains("Emerald")) {tempString+= sig.Substring(0, 3) + " [LiC5], ";}
                else if (sig.Contains("Crimson")) {tempString+= sig.Substring(0, 3) + " [MaC5], ";}
                else if (sig.Contains("Bandit")) {tempString+= sig.Substring(0, 3) + " [AmC5], ";}
                else if (sig.Contains("Profiteer")) {tempString+= sig.Substring(0, 3) + " [GoC5], ";}
                else if (sig.Contains("Phoenix")) {tempString+= sig.Substring(0, 3) + " [CeC5], ";}
                else if (sig.Contains("Forgotten")) {tempString+= sig.Substring(0, 3) + " [ViC5], ";}
                else if (sig.Contains("Rapture")) {tempString+= sig.Substring(0, 3) + " [AzC5], ";}
                else if (sig.Contains("Saintly")) {tempString+= sig.Substring(0, 3) + " [VeC5], ";}*/
                solved ++;
                }
            else if (sig.Contains("Data Site")) solved ++;
            else if (sig.Contains("Relic Site")) solved ++;
            else if (sig.Contains("Wormhole")) solved ++;
            else if (sig.Contains("Combat Site")) solved ++;
            else if (sig.Contains("Gas Site")) solved ++;
            else
            {
                tempString += sig.Substring(0, 3) + ", ";
                unsolved ++;
            }
        }
        var filtered = solved - targets;
        if (filtered > 0)
        {
            ScanTextComponent.text += "" + filtered + " ";
            if ((unsolved > 0) || (targets > 0))
            {
                ScanTextComponent.text += "+ ";
            }
        }
        // clean up the last space and comma
        if (tempString.Length > 0)
        {
            ScanTextComponent.text += tempString.Substring(0, tempString.Length - 2);
        }
        //SigTextComponent.text = " (" + solved + "/" + total + ")";
        if (targets > 0) ScanTextComponent.color = Color.green;
        else ScanTextComponent.color = Color.white;
        //else if (unsolved == 0) NameTextComponent.color = Color.green;
        //else if (solved > 1) NameTextComponent.color = Color.yellow;
        //else if ((DateTime.Now - Time).TotalMinutes < 60) TimerTextComponent.color = Color.Lerp(Color.white, Color.yellow, ((float)(DateTime.Now - Time).TotalMinutes) / 60);
        //else if ((DateTime.Now - Time).TotalMinutes >= 60) TimerTextComponent.color = Color.Lerp(Color.yellow, Color.red, (((float)(DateTime.Now - Time).TotalMinutes) - 60) / 540);
    }
}
