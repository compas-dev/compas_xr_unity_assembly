using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Helpers;

public class ScrollSearchScript : MonoBehaviour
{
    //Other Scripts for inuse objects
    public DatabaseManager databaseManager;
    public UIFunctionalities uIFunctionalities;
    
    //Public Variables
    public RectTransform scrollablePanel;
    public RectTransform container;
    // public GameObject cellsParent;
    public List<GameObject> cells;
    public RectTransform center;

    //Private Variables
    public float[] cellDistances;
    private bool dragging = false;
    private int cellSpacing;
    private int closestCellIndex;
    
    // Start is called before the first frame update
    void Start()
    {
        //Get the database manager
        databaseManager = GameObject.Find("DatabaseManager").GetComponent<DatabaseManager>();
        uIFunctionalities = GameObject.Find("UIFunctionalities").GetComponent<UIFunctionalities>();
        
        //Get the cell spacing
        cellSpacing = (int)cells[1].GetComponent<RectTransform>().anchoredPosition.y - (int)cells[0].GetComponent<RectTransform>().anchoredPosition.y;
        Debug.Log("Ancored Position = " + cells[1].GetComponent<RectTransform>().anchoredPosition);
        Debug.Log("Cell Spacing = " + cellSpacing);

        //Get the center cell
        int cellLength = cells.Count;
        cellDistances = new float[cellLength];
    }

    // Update is called once per frame
    void Update()
    {
        //Calculate the distance between the center and the cells
        for (int i = 0; i < cells.Count; i++)
        {
            cellDistances[i] = Mathf.Abs(center.transform.position.y - cells[i].transform.position.y);
        }

        //Get the minimum distance
        float minDistance = Mathf.Min(cellDistances);
        Debug.Log("MINDISTANCE = " + minDistance);

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
