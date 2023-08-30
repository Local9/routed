using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NodeFrame : MonoBehaviour
{
    [SerializeField] public TextMeshPro TmpNameComponent;
    [SerializeField] public TextMeshPro TmpTimerComponent;
    [SerializeField] public TextMeshPro TmpScanComponent;
    [SerializeField] public string NodeName;
    [SerializeField] public DateTime NodeTime;
    private GameManager _gameManager;

    void Start()
    {
        _gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        UpdateUIText(NodeName);
    }

    /// <summary>
    /// Update the UI text for the node
    /// </summary>
    void Update()
    {
        int minutesSinceScan = Convert.ToInt32(Math.Round((DateTime.Now - NodeTime).TotalMinutes));

        if (TmpTimerComponent is null) return;

        if (minutesSinceScan < 60) TmpTimerComponent.text = Mathf.Round(minutesSinceScan) + "m";
        else if (minutesSinceScan < 1440) TmpTimerComponent.text = Mathf.Round(minutesSinceScan / 60) + "h";
        else TmpTimerComponent.text = Mathf.Round(minutesSinceScan / 1440) + "d";
    }

    /// <summary>
    /// Remove sigs from the node's sig list
    /// </summary>
    public void RemoveSigs()
    {
        foreach (Node node in _gameManager.WorkingRoute.NodeList)
        {
            if (node.Name == NodeName)
            {
                node.SigList.Clear();
                node.Update = DateTime.Now;
                Serializer.SaveData(_gameManager.WorkingRoute);
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                return;
            }
        }
    }

    /// <summary>
    /// Import sigs from clipboard and update the node's sig list
    /// </summary>
    public void UpdateSigs()
    {
        Debug.Log("updating sigs");
        List<string> NewStrings = new List<string>(GUIUtility.systemCopyBuffer.Split('\n'));

        if (NewStrings == null) return;
        if (NewStrings.Count == 0) return;
        if (NewStrings[0][3] != '-') return;

        foreach (Node node in GameObject.Find("GameManager").GetComponent<GameManager>().WorkingRoute.NodeList)
        {
            if (node.Name == NodeName)
            {
                Debug.Log("node name match");
                // Compare old sigs to new, update old list
                for (int i = 0; i < NewStrings.Count; i++)
                {
                    for (int j = 0; j < node.SigList.Count; j++)
                    {
                        if (NewStrings[i].Substring(0, 7) == node.SigList[j].Substring(0, 7))
                        {
                            Debug.Log("existing sig found");
                            if (NewStrings[i].Length < node.SigList[j].Length)
                            {
                                Debug.Log("replacing new sig with further progressed old sig");
                                NewStrings[i] = node.SigList[j];
                            }
                        }
                    }
                }

                Debug.Log("updated sigs");
                node.SigList = NewStrings;
                node.Update = DateTime.Now;

                _gameManager.WorkingRoute.CameraY = transform.position.y;
                Vector3 camPos = Camera.main.transform.position;
                Camera.main.transform.position = new Vector3(camPos.x, transform.position.y, camPos.z);
                _gameManager.ClampCamera();

                Serializer.SaveData(_gameManager.WorkingRoute);
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                return;
            }
        }
    }

    /// <summary>
    /// Update the UI text for the node based on the node's sig list
    /// </summary>
    /// <param name="nodeName"></param>
    void UpdateUIText(string nodeName)
    {
        List<string> lstSignatures = new List<string>();
        List<Node> nodeList = GameObject.Find("GameManager").GetComponent<GameManager>().WorkingRoute.NodeList;

        foreach (Node node in nodeList)
        {
            if (nodeName == node.Name)
            {
                lstSignatures = node.SigList;
            }
        }

        TmpNameComponent.text = NodeName;

        // Convert list of raw sigs to human-readable format
        int total = lstSignatures.Count;
        int solved = 0;
        int unsolved = 0;
        TmpScanComponent.text = "";
        List<string> tempString = new List<string>(); // change to a list of strings

        foreach (string sig in lstSignatures)
        {
            if (sig.Contains("Covert Research"))
            {
                tempString.Add($"{sig.Substring(0, 3)} [GS]");
                solved++;
            }
            else if (sig.Contains("Sleeper Cache"))
            {
                tempString.Add($"{sig.Substring(0, 3)} [SC]");
                solved++;
            }
            else if (sig.Contains("Nebula"))
            {
                if (sig.Contains("Sister")) { tempString.Add($"{sig.Substring(0, 3)} [LiM20]"); }
                else if (sig.Contains("Helix")) { tempString.Add($"{sig.Substring(0, 3)} [LiM60]"); }
                else if (sig.Contains("Wild")) { tempString.Add($"{sig.Substring(0, 3)} [MaM20]"); }
                else if (sig.Contains("Blackeye")) { tempString.Add($"{sig.Substring(0, 3)} [MaM60]"); }
                else if (sig.Contains("Sunspark")) { tempString.Add($"{sig.Substring(0, 3)} [AmM20]"); }
                else if (sig.Contains("Diablo")) { tempString.Add($"{sig.Substring(0, 3)} [AmM60]"); }
                else if (sig.Contains("Smoking")) { tempString.Add($"{sig.Substring(0, 3)} [GoM20]"); }
                else if (sig.Contains("Ring")) { tempString.Add($"{sig.Substring(0, 3)} [GoM60]"); }
                else if (sig.Contains("Calabash")) { tempString.Add($"{sig.Substring(0, 3)} [CeM20]"); }
                else if (sig.Contains("Glass")) { tempString.Add($"{sig.Substring(0, 3)} [CeM60]"); }
                else if (sig.Contains("Bright")) { tempString.Add($"{sig.Substring(0, 3)} [ViM20]"); }
                else if (sig.Contains("Sparking")) { tempString.Add($"{sig.Substring(0, 3)} [ViM60]"); }
                else if (sig.Contains("Ghost")) { tempString.Add($"{sig.Substring(0, 3)} [AzM20]"); }
                else if (sig.Contains("Eagle")) { tempString.Add($"{sig.Substring(0, 3)} [AzM60]"); }
                else if (sig.Contains("Flame")) { tempString.Add($"{sig.Substring(0, 3)} [VeM20]"); }
                else if (sig.Contains("Pipe")) { tempString.Add($"{sig.Substring(0, 3)} [VeM60]"); }
                solved++;
            }
            else if (sig.Contains("Data Site")) solved++;
            else if (sig.Contains("Relic Site")) solved++;
            else if (sig.Contains("Wormhole")) solved++;
            else if (sig.Contains("Combat Site")) solved++;
            else if (sig.Contains("Gas Site")) solved++;
            else
            {
                tempString.Add(sig.Substring(0, 3));
                unsolved++;
            }
        }

        int filtered = solved - tempString.Count;
        if (filtered > 0)
        {
            TmpScanComponent.text += "" + filtered + " ";
            if ((unsolved > 0) || (tempString.Count > 0))
            {
                TmpScanComponent.text += "+ ";
            }
        }

        if (tempString.Count > 0) TmpScanComponent.text = String.Join(", ", tempString);

        if (tempString.Count > 0) TmpScanComponent.color = Color.green;
        else TmpScanComponent.color = Color.white;
    }
}
