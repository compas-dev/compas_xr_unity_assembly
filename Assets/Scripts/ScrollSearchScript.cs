using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Helpers;
using UnityEngine.UI;
using TMPro;

public class ScrollSearchScript : MonoBehaviour
{
    //Other Scripts for inuse objects
    public DatabaseManager databaseManager;
    public UIFunctionalities uIFunctionalities;
    public GameObject cellPrefabAsset;
    public GameObject cellsParent;

    //Public Variables
    public RectTransform scrollablePanel;
    public RectTransform container;
    // public GameObject cellsParent;
    public List<GameObject> cells;
    public RectTransform center;
    public GameObject ScrollSearchObjects;
    public GameObject cellPrefab;

    //Private Variables
    public float[] cellDistances;
    private bool dragging = false;
    private int cellSpacing = -20;
    private int closestCellIndex;
    
    // Start is called before the first frame update
    void Start()
    {
        //Get the database manager
        databaseManager = GameObject.Find("DatabaseManager").GetComponent<DatabaseManager>();
        GameObject Canvas = GameObject.Find("Canvas");
        ScrollSearchObjects = Canvas.FindObject("ScrollSearch");
        cellPrefab = ScrollSearchObjects.FindObject("CellPrefab");
        cellsParent = ScrollSearchObjects.FindObject("Container");


        //Get the cell spacing
        // cellSpacing = (int)cells[1].GetComponent<RectTransform>().anchoredPosition.y - (int)cells[0].GetComponent<RectTransform>().anchoredPosition.y;
        // Debug.Log("Ancored Position = " + cells[1].GetComponent<RectTransform>().anchoredPosition);
        // Debug.Log("Cell Spacing = " + cellSpacing);

        //Get the center cell
        // int cellLength = cells.Count;
        // cellDistances = new float[cellLength];
    }

    // Update is called once per frame
    //TODO: Convert to method.
    //TODO: MOVE TO UIFUNCTIONALITIES
    void Update()
    {
        if (ScrollSearchObjects.activeSelf)
        {
            //create cells from prefab
            if (cells.Count != databaseManager.BuildingPlanDataItem.steps.Count)
            {
                CreateCellsFromPrefab(ref cellPrefab, cellSpacing, cellsParent, databaseManager.BuildingPlanDataItem.steps.Count);
            }

            // Debug.Log("I EXIT HERE.");

            //Loop through the cells and get the closest cell
            if (cells.Count == databaseManager.BuildingPlanDataItem.steps.Count)
            {
                //Calculate the distance between the center and the cells
                for (int i = 0; i < cells.Count; i++)
                {
                    cellDistances[i] = Mathf.Abs(center.transform.position.y - cells[i].transform.position.y);
                }

                //Get the minimum distance
                float minDistance = Mathf.Min(cellDistances);
                // Debug.Log("MINDISTANCE = " + minDistance);

                //Get the index of the closest cell
                for (int a = 0; a < cells.Count; a++)
                {
                    if (minDistance == cellDistances[a])
                    {
                        closestCellIndex = a;
                        Debug.Log("ClosestCellIndex = " + closestCellIndex);
                    }
                }

                if (!dragging)
                {
                    LerpToCell(closestCellIndex * - cellSpacing);
                }
            }
        }
        else
        {
            cells.Clear();
            cellDistances = new float[0];
        }
    }
    
    void CreateCellsFromPrefab(ref GameObject prefabAsset, int cellSpacing, GameObject cellsParent, int cellCount)
    {
        if(prefabAsset == null)
        {
            Debug.Log("Prefab Asset is null");
            return;
        }
        if(cellsParent == null)
        {
            Debug.Log("Cells Parent is null");
            return;
        }
        if(cellCount == 0)
        {
            Debug.Log("Cell Count is 0");
            return;
        }

        Debug.Log("CELL COUNT " + cellCount);

        //Create cell distance array from the cell count
        cellDistances = new float[cellCount];

        //Create cells from prefab
        for (int i = 0; i < cellCount; i++)
        {
            GameObject cell = Instantiate(prefabAsset, cellsParent.transform);
            cell.SetActive(true);
            Vector2 newCellPosition = new Vector2(0 , i * cellSpacing);
            cell.GetComponent<RectTransform>().anchoredPosition = newCellPosition;
            cell.GetComponentInChildren<TMP_Text>().text = i.ToString();
            cell.name = $"Cell {i}";

            cells.Add(cell);
        }
    }

    void LerpToCell(int position)
    {
        float newY = Mathf.Lerp(container.anchoredPosition.y, position, Time.deltaTime * 10f);
        Vector2 newPosition = new Vector2(container.anchoredPosition.x, newY);

        container.anchoredPosition = newPosition;

        Debug.Log("LerpToCell");
    }

    public void StartDrag()
    {
        Debug.Log("Start Drag");
        dragging = true;
    }

    public void EndDrag()
    {
        Debug.Log("End Drag");
        dragging = false;
    }
}
