using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField] public GameObject DebugReadout;
    [SerializeField] public string[] securityString;
    [SerializeField] public GameObject NodePrefab;
    [SerializeField] public Route WorkingRoute;

    static bool listeningForName = false;
    public TextAsset securityCSV;
    public static string nameSoFar = "";
    public int RowCount = 0;
    public float RowSize = 0.25f;

    private TextMeshProUGUI _debugText;
    private Vector3 _mouseDragStartPoint = Vector3.zero;
    private Vector3 _cameraDragStartPoint = Vector3.zero;
    private bool _cameraDraggingNow = false;
    private float _cameraClampPadding = 0f;

    /// <summary>
    /// Unity Start function
    /// </summary>
    void Start()
    {
        string csv = securityCSV.ToString();
        securityString = csv.Split(',');
        Debug.Log("There are " + securityString.Length + " lines loaded into the security string array.");

        int nodesManaged = 0;
        int stringsManaged = 0;
        // Load a saved route or create a fresh one
        if (File.Exists(Serializer.SavePath))
        {
            WorkingRoute = Serializer.ReadData(WorkingRoute);
            for (int i = 0; i < WorkingRoute.NodeList.Count; i++)
            {
                Vector2 pos = new Vector2(0f, -i * RowSize);
                GameObject frame = Instantiate(NodePrefab, pos, Quaternion.identity);
                frame.GetComponent<NodeFrame>().NodeName = WorkingRoute.NodeList[i].Name;
                frame.GetComponent<NodeFrame>().NodeTime = WorkingRoute.NodeList[i].Update;

                float secStatus = FetchSystemSecurity(WorkingRoute.NodeList[i].Name);
                Color newCol = new Color();
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
                frame.GetComponent<NodeFrame>().TmpNameComponent.color = newCol;

                RowCount++;
                nodesManaged++;
                stringsManaged += WorkingRoute.NodeList[i].SigList.Count;
            }
            Debug.Log("loading camera x: " + WorkingRoute.CameraY);
            Camera.main.transform.position += new Vector3(0f, WorkingRoute.CameraY);
        }
        else WorkingRoute = new Route();

        _debugText = DebugReadout.GetComponent<TextMeshProUGUI>();
        _debugText.text = "Managing " + nodesManaged + " systems";
        _debugText.text += " and " + stringsManaged + " signatures.";
    }

    /// <summary>
    /// Main loop, handles input and camera movement
    /// </summary>
    void Update()
    {
        InputHandler();
        if (_cameraDraggingNow) DragCamera();
        if (listeningForName) RecordKeystrokes();
        else if ((Input.GetKeyDown(KeyCode.Return)) && (nameSoFar != "")) FinishRecordingKeyStrokes();
    }

    /// <summary>
    /// Interprets input from the user
    /// </summary>
    void InputHandler()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
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

    /// <summary>
    /// Node management
    /// </summary>
    /// <param name="mode"></param>
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

    /// <summary>
    /// Gets the security status of a system
    /// </summary>
    /// <param name="systemName"></param>
    /// <returns></returns>
    float FetchSystemSecurity(string systemName)
    {
        float fetchedSecurity = 1f;
        Debug.Log("Searching for security status for system name: " + systemName);
        for (int i = 0; i < securityString.Length; i++)
        {
            if (securityString[i] == systemName)
            {
                Debug.Log("Found name match: " + systemName + " + " + securityString[i] + " (security: " + securityString[i + 1] + ")");
                fetchedSecurity = float.Parse(securityString[i + 1]);
                break;
            }
        }
        // Round to nearest tenth
        fetchedSecurity = Mathf.Round(fetchedSecurity * 10.0f) * 0.1f;
        Debug.Log("Security for " + systemName + ": " + fetchedSecurity);
        return fetchedSecurity;
    }

    /// <summary>
    /// Algorithm for the imported route
    /// </summary>
    void ImportRouteAlgorithm()
    {
        // Validate there is a route in the clipBoard
        string clipBoard = GUIUtility.systemCopyBuffer;
        if (clipBoard.Substring(0, 18) != "Current location: ") return;
        WorkingRoute.NodeList.Clear();

        // Trim "Current location: " from clipboard
        int terminationTrim = 0;
        for (int i = 0; i < clipBoard.Length; i++)
        {
            if (clipBoard[i] == '1')
            {
                terminationTrim = i;
                break;
            }
        }
        clipBoard = clipBoard.Substring(terminationTrim);

        // Parse each solar system into a working list
        string[] parsedList = clipBoard.Split('\n');
        int terminationCharacterLength = 0;
        for (int i = 0; i < parsedList.Length; i++)
        {
            // Search string for subsequent parenthesis
            for (int j = 0; j < parsedList[i].Length; j++)
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
        for (int i = 0; i < parsedList.Length; i++)
        {
            Node newNode = new Node(parsedList[i]);
            WorkingRoute.NodeList.Add(newNode);
        }

        Serializer.SaveData(WorkingRoute);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    /// <summary>
    /// Records keystrokes, used for naming systems
    /// </summary>
    void RecordKeystrokes()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) nameSoFar = "";
        else if ((nameSoFar != "") && (Input.GetKeyDown(KeyCode.Backspace))) nameSoFar = nameSoFar.Substring(0, nameSoFar.Length - 1);
        else nameSoFar += Input.inputString;
    }

    /// <summary>
    /// Finishes recording keystrokes, used for naming systems
    /// </summary>
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

    /// <summary>
    /// Scrolls the camera up or down
    /// </summary>
    /// <param name="dir"></param>
    void ScrollCamera(Vector3 dir)
    {
        Camera.main.transform.position += dir * RowSize;
        ClampCamera();
        Serializer.SaveData(WorkingRoute);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    /// <summary>
    /// Allows the camera to be dragged
    /// </summary>
    void BeginCameraDrag()
    {
        _cameraDraggingNow = true;
        _mouseDragStartPoint = Input.mousePosition;
        _cameraDragStartPoint = Camera.main.transform.position;
    }

    /// <summary>
    /// Drags the camera
    /// </summary>
    void DragCamera()
    {
        Vector3 newLoc = new Vector3(0f, (_mouseDragStartPoint.y - Input.mousePosition.y) / 100, 0f);
        Camera.main.transform.position = _cameraDragStartPoint + newLoc;
    }

    /// <summary>
    /// Finishes dragging the camera
    /// </summary>
    void FinishCameraDrag()
    {
        _cameraDraggingNow = false;
        float yOffset = Camera.main.transform.position.y % RowSize;
        if (yOffset != 0)
        {
            Camera.main.transform.position -= new Vector3(0f, yOffset, 0f);
        }
        ClampCamera();
        Serializer.SaveData(WorkingRoute);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    /// <summary>
    /// Keeps the camera from going out of bounds
    /// </summary>
    public void ClampCamera()
    {
        Vector3 camPos = Camera.main.transform.position;
        if (camPos.y > -2 * RowSize + _cameraClampPadding)
        {
            camPos = new Vector3(camPos.x, -2 * RowSize + _cameraClampPadding, camPos.z);
        }
        else if (camPos.y < -((RowCount - 3) * RowSize))
        {
            camPos = new Vector3(camPos.x, -((RowCount - 3) * RowSize) - _cameraClampPadding, camPos.z);
        }
        Camera.main.transform.position = camPos;
        WorkingRoute.CameraY = camPos.y;
    }
}