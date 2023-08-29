using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using TMPro;
public class GameManager : MonoBehaviour
{
    public TextAsset securityCSV;
    [SerializeField] public GameObject DebugReadout;
    private TextMeshProUGUI debugText;
    //private Dictionary<string, float> securityDictionary = new Dictionary<string, float>();
    [SerializeField] public string[] securityString;
    [SerializeField] public GameObject NodePrefab;
    [SerializeField] public Route WorkingRoute;
    static bool listeningForName = false;
    public static string nameSoFar = "";
    public int RowCount = 0;
    public float RowSize = 0.25f;
    private Vector3 mouseDragStartPoint = Vector3.zero;
    private Vector3 cameraDragStartPoint = Vector3.zero;
    private bool cameraDraggingNow = false;
    private float cameraClampPadding = 0f;//0.05f;
    void Start()
    {
        var csv = securityCSV.ToString();
        securityString = csv.Split(',');
        Debug.Log("There are "+securityString.Length+" lines loaded into the security string array.");

        var nodesManaged = 0;
        var stringsManaged = 0;
        // Load a saved route or create a fresh one
        if (File.Exists(Serializer.SavePath))
        {
            WorkingRoute = Serializer.ReadData(WorkingRoute);
            for (var i = 0; i < WorkingRoute.NodeList.Count; i ++)
            {
                var pos = new Vector2(0f, -i * RowSize);
                var frame = Instantiate(NodePrefab, pos, Quaternion.identity);
                frame.GetComponent<NodeFrame>().Name = WorkingRoute.NodeList[i].Name;
                frame.GetComponent<NodeFrame>().Time = WorkingRoute.NodeList[i].Update;
                
                var secStatus = FetchSystemSecurity(WorkingRoute.NodeList[i].Name);
                var newCol = new Color();
                if (secStatus >= 0.75f)
                {
                    newCol = Color.Lerp(Color.green, Color.cyan, (secStatus - 0.5f) / 0.5f);
                }
                else if (secStatus >= 0.5f)
                {
                    newCol = Color.Lerp(Color.yellow, Color.green, (secStatus - 0.5f) / 0.5f);
                }
                else if (secStatus < 0.5f)
                {
                    newCol = Color.Lerp(Color.red, new Color(1.0f, 0.64f, 0.0f), secStatus / 0.5f);
                }
                frame.GetComponent<NodeFrame>().NameTextComponent.color = newCol;

                RowCount++;
                nodesManaged++;
                stringsManaged += WorkingRoute.NodeList[i].SigList.Count;
            }
            Debug.Log("loading camera x: " + WorkingRoute.CameraY);
            Camera.main.transform.position += new Vector3(0f, WorkingRoute.CameraY);
        }
        else WorkingRoute = new Route();

        debugText = DebugReadout.GetComponent<TextMeshProUGUI>();
        debugText.text = "Managing " + nodesManaged + " systems";
        debugText.text += " and " + stringsManaged + " signatures.";
    }
    void Update()
    {
        // Main loop
        InputHandler();
        if (cameraDraggingNow) DragCamera();
        if (listeningForName) RecordKeystrokes();
        else if ((Input.GetKeyDown(KeyCode.Return)) && (nameSoFar != "")) FinishRecordingKeyStrokes();
    }
    void InputHandler()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            //ManageDisk("wipe");
            //ManageDisk("restart");
            Serializer.ClearData(WorkingRoute);
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
        if (Input.GetKeyDown(KeyCode.Delete)) ManageNode("backspace");
        if (Input.GetKeyDown(KeyCode.Return)) ManageNode("record");
        if (Input.GetKeyDown(KeyCode.F12)) ImportRouteAlgorithm();
        if (Input.GetAxis("Mouse ScrollWheel") > 0f) ScrollCamera(Vector3.up);
        if (Input.GetAxis("Mouse ScrollWheel") < 0f) ScrollCamera(Vector3.down);
        if (Input.GetMouseButtonDown(2)) BeginCameraDrag();
        if (Input.GetMouseButtonUp(2)) FinishCameraDrag();
    }
    /* NODE MANAGEMENT */
    void ManageNode(string mode)
    {
        switch (mode)
        {
            case "backspace":
                if (WorkingRoute.NodeList.Count < 1) break;
                WorkingRoute.NodeList.Remove(WorkingRoute.NodeList[WorkingRoute.NodeList.Count - 1]);
                Serializer.SaveData(WorkingRoute);
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                break;
            case "record":
                Debug.Log("Listening for keystrokes");
                listeningForName = !listeningForName;
                break;
        }
    }
    float FetchSystemSecurity(string systemName)
    {
        var fetchedSecurity = 1f;
        Debug.Log("Searching for security status for system name: "+systemName);
        for (var i = 0; i < securityString.Length; i ++)
        {
            if (securityString[i] == systemName)
            {
                Debug.Log("Found name match: "+systemName+" + "+securityString[i]+" (security: "+securityString[i + 1]+")");
                fetchedSecurity = float.Parse(securityString[i + 1]);
                break;
            }
        }
        // Round to nearest tenth
        fetchedSecurity = Mathf.Round(fetchedSecurity * 10.0f) * 0.1f;
        Debug.Log("Security for "+systemName+": "+fetchedSecurity);
        return(fetchedSecurity);
    }
    void ImportRouteAlgorithm()
    {
        // Validate there is a route in the clipBoard
        var clipBoard = GUIUtility.systemCopyBuffer;
        if (clipBoard.Substring(0, 18) != "Current location: ") return;
        WorkingRoute.NodeList.Clear();

        // Trim "Current location: " from clipboard
        var terminationTrim = 0;
        for (var i = 0; i < clipBoard.Length; i ++)
        {
            if (clipBoard[i] == '1')
            {
                terminationTrim = i;
                break;
            }
        }
        clipBoard = clipBoard.Substring(terminationTrim);

        // Parse each solar system into a working list
        var parsedList = clipBoard.Split('\n');
        var terminationCharacterLength = 0;
        for (var i = 0; i < parsedList.Length; i ++)
        {
            // Search string for subsequent parenthesis
            for (var j = 0; j < parsedList[i].Length; j ++)
            {
                if (parsedList[i][j] == '(')
                {
                    terminationCharacterLength = j - 3;
                    break;
                }
            }
            // Cut string to that parenthesis
            parsedList[i] = parsedList[i].Substring(3, terminationCharacterLength).Trim();
        }

        // Create new nodes and add them to working route
        for (var i = 0; i < parsedList.Length; i ++)
        {
            Node newNode = new Node(parsedList[i]);
            WorkingRoute.NodeList.Add(newNode);
        }

        Serializer.SaveData(WorkingRoute);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    void RecordKeystrokes()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) nameSoFar = "";
        else if ((nameSoFar != "") && (Input.GetKeyDown(KeyCode.Backspace))) nameSoFar = nameSoFar.Substring(0, nameSoFar.Length - 1);
        else nameSoFar += Input.inputString;
    }
    void FinishRecordingKeyStrokes()
    {
        nameSoFar = nameSoFar.Trim();
        //Debug.Log("Adding new system: " + nameSoFar);
        Node newNode = new Node(nameSoFar);
        nameSoFar = "";
        WorkingRoute.NodeList.Add(newNode);
        Serializer.SaveData(WorkingRoute);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    /* CAMERA MANAGEMENT */
    void ScrollCamera(Vector3 dir)
    {
        Camera.main.transform.position += dir * RowSize;
        ClampCamera();
        Serializer.SaveData(WorkingRoute);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    void BeginCameraDrag()
    {
        cameraDraggingNow = true;
        mouseDragStartPoint = Input.mousePosition;
        cameraDragStartPoint = Camera.main.transform.position;
    }
    void DragCamera()
    {
        var newLoc = new Vector3(0f, (mouseDragStartPoint.y - Input.mousePosition.y) / 100,0f);
        Camera.main.transform.position = cameraDragStartPoint + newLoc;
    }
    void FinishCameraDrag()
    {
        cameraDraggingNow = false;
        var yOffset = Camera.main.transform.position.y % RowSize;
        if (yOffset != 0)
        {
            Camera.main.transform.position -= new Vector3(0f, yOffset, 0f);
        }
        ClampCamera();
        Serializer.SaveData(WorkingRoute);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    public void ClampCamera()
    {
        var camPos = Camera.main.transform.position;
        if (camPos.y > -2 * RowSize + cameraClampPadding)
        {
            camPos = new Vector3(camPos.x, -2 * RowSize + cameraClampPadding, camPos.z);
        }
        else if (camPos.y < -((RowCount - 3) * RowSize))
        {
            camPos = new Vector3(camPos.x, -((RowCount - 3) * RowSize) - cameraClampPadding, camPos.z);
        }
        Camera.main.transform.position = camPos;
        WorkingRoute.CameraY = camPos.y;
    }
}