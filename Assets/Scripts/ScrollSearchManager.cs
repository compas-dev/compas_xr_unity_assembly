using System.Collections.Generic;
using UnityEngine;
using Helpers;
using UnityEngine.UI;
using TMPro;
using CompasXR.Core;
using CompasXR.Core.Data;

namespace CompasXR.UI
{
    public class ScrollSearchManager : MonoBehaviour
    {
        //Other Scripts and global Objects for in script use
        public DatabaseManager databaseManager;
        public InstantiateObjects instantiateObjects;
        public UIFunctionalities uiFunctionalites;
        public GameObject Elements;

        //Public Variables
        public RectTransform scrollablePanel;
        public RectTransform container;
        public GameObject cellsParent;
        public List<GameObject> cells;
        public RectTransform center;
        public GameObject ScrollSearchObjects;
        public GameObject cellPrefab;

        //Private Variables
        public bool cellsExist = false;
        public float[] cellDistances;
        private bool dragging = false;
        public int cellSpacing = -20;
        private int closestCellIndex;
        private int? selectedCellIndex = null;
        public string selectedCellStepIndex;
        
        void Start()
        {
            //Find initial objects
            OnStartInitilization();
        }

        void Update()
        {
            //Create Scroll Search Objects based on the active state
            ScrollSearchControler(ref cellsExist);
        }

        /////////////////////////////////////////////// Initilization and Control Methods ///////////////////////////////////////////////       
        private void OnStartInitilization()
        {
            //Get the database manager
            databaseManager = GameObject.Find("DatabaseManager").GetComponent<DatabaseManager>();
            instantiateObjects = GameObject.Find("Instantiate").GetComponent<InstantiateObjects>();
            uiFunctionalites = GameObject.Find("UIFunctionalities").GetComponent<UIFunctionalities>();
            Elements = GameObject.Find("Elements");

            //Find OnScreen Objects
            GameObject Canvas = GameObject.Find("Canvas");
            GameObject VisiblityEditor = Canvas.FindObject("Visibility_Editor");
            GameObject ScrollSearchToggle = VisiblityEditor.FindObject("ScrollSearchToggle");
            ScrollSearchObjects = ScrollSearchToggle.FindObject("ScrollSearchObjects");
            cellPrefab = ScrollSearchObjects.FindObject("CellPrefab");
            cellsParent = ScrollSearchObjects.FindObject("Container");

        }
        public void ScrollSearchControler(ref bool cellsExist)
        {
            //If cells exist loop through the cells and get the closest cell
            if (cellsExist)
            {
                //Calculate the distance between the center and the cells
                for (int i = 0; i < cells.Count; i++)
                {
                    cellDistances[i] = Mathf.Abs(center.transform.position.y - cells[i].transform.position.y);
                }

                //Get the minimum distance
                float minDistance = Mathf.Min(cellDistances);

                //Get the index of the closest cell
                for (int a = 0; a < cells.Count; a++)
                {
                    if (minDistance == cellDistances[a])
                    {
                        closestCellIndex = a;
                    }
                }

                //If the dragging event has ended then lerp to the closest cell and color the item from the cell
                if (!dragging)
                {
                    //Adjust pannel to the closest cell
                    LerpToCell(closestCellIndex * - cellSpacing);

                    //Search for the step
                    ScrollSearchObjectColor(ref closestCellIndex, ref selectedCellIndex, ref selectedCellStepIndex, ref cells);
                }
            }
        }

        /////////////////////////////////////////////// OnScreen Object Management /////////////////////////////////////////////////////   
        public void CreateCellsFromPrefab(ref GameObject prefabAsset, int cellSpacing, GameObject cellsParent, int cellCount, ref bool cellsExist)
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

            //Change the reference bool
            cellsExist = true;
        }
        public void ResetScrollSearch(ref bool cellsExist)
        {
            //Set bool to false
            cellsExist = false;

            //Destroy all cells
            DestroyCellInfo(ref cells, ref cellDistances);

            //Color the last Selected Item based on the current application mode
            if (selectedCellStepIndex != null)
            {
                Step step = databaseManager.BuildingPlanDataItem.steps[selectedCellStepIndex];
                GameObject objectToColor = Elements.FindObject(selectedCellStepIndex).FindObject(step.data.element_ids[0] + " Geometry");

                if (objectToColor != null)
                {
                    instantiateObjects.ObjectColorandTouchEvaluater(
                        instantiateObjects.visulizationController.VisulizationMode,
                        instantiateObjects.visulizationController.TouchMode,
                        step, selectedCellStepIndex, objectToColor);
                }

                //If Priority Viewer toggle is on then color the add additional color based on priority
                if (uiFunctionalites.PriorityViewerToggleObject.GetComponent<Toggle>().isOn)
                {
                    instantiateObjects.ColorObjectByPriority(uiFunctionalites.SelectedPriority, step.data.priority.ToString(), selectedCellStepIndex, objectToColor);
                }
            }

            //Reset the selected cell index
            selectedCellIndex = null;
        }
        public void DestroyCellInfo(ref List<GameObject> cells, ref float[] cellDistances)
        {
            //Destroy all cells
            foreach (GameObject cell in cells)
            {
                Destroy(cell);
            }

            //Clear the cells list and cell distances array
            cells.Clear();
            cellDistances = new float[0];
        }
        void LerpToCell(int position)
        {
            float newY = Mathf.Lerp(container.anchoredPosition.y, position, Time.deltaTime * 10f);
            Vector2 newPosition = new Vector2(container.anchoredPosition.x, newY);

            container.anchoredPosition = newPosition;
        }

        /////////////////////////////////////////////// Spatial Object Management /////////////////////////////////////////////////////
        public void ScrollSearchObjectColor(ref int closestCellIndex, ref int? selectedCellIndex, ref string selectedCellStepIndex, ref List<GameObject> cells)
        {
            if (closestCellIndex != selectedCellIndex)
            {
                //Color the previous item based on current application mode settings
                if (selectedCellIndex != null && selectedCellStepIndex != null)
                {
                    //Get information for the specific object
                    Step step = databaseManager.BuildingPlanDataItem.steps[selectedCellStepIndex];
                    GameObject objectToColor = Elements.FindObject(selectedCellStepIndex).FindObject(step.data.element_ids[0] + " Geometry");

                    if(objectToColor == null)
                    {
                        Debug.Log("ScrollSearchController: ObjectToColor is null.");
                    }

                    if (selectedCellStepIndex != uiFunctionalites.CurrentStep)
                    {
                        //Color the object based on current application information
                        instantiateObjects.ObjectColorandTouchEvaluater(
                            instantiateObjects.visulizationController.VisulizationMode,
                            instantiateObjects.visulizationController.TouchMode,
                            step, selectedCellStepIndex, objectToColor);

                        //If Priority Viewer toggle is on then color the add additional color based on priority
                        if (uiFunctionalites.PriorityViewerToggleObject.GetComponent<Toggle>().isOn)
                        {
                            instantiateObjects.ColorObjectByPriority(uiFunctionalites.SelectedPriority, step.data.priority.ToString(), selectedCellStepIndex, objectToColor);
                        }
                    }
                    else
                    {
                        //Color the object based on human or robot
                        instantiateObjects.ColorHumanOrRobot(step.data.actor, step.data.is_built, objectToColor);
                    }
                    
                }

                //Set new selected cell index
                selectedCellIndex = closestCellIndex;
                selectedCellStepIndex = GetTextItemFromGameObject(cells[closestCellIndex]);
                Step newStep = databaseManager.BuildingPlanDataItem.steps[selectedCellStepIndex];

                //Find the gameObject associated with the cells text value
                GameObject newObjectToColor = Elements.FindObject(selectedCellStepIndex).FindObject(newStep.data.element_ids[0] + " Geometry");

                //if the search Object is not null then color it, but if it is null then display a warning message
                if (newObjectToColor != null)
                {
                    Debug.Log($"ScrollSearchController: Coloring Object {selectedCellStepIndex} by searched color.");
                    instantiateObjects.ColorObjectbyInputMaterial(newObjectToColor, instantiateObjects.SearchedObjectMaterial);
                }
                else
                {
                    string message = $"WARNING: The item {selectedCellStepIndex} could not be found. Please retype information and try search again.";
                    uiFunctionalites.SignalOnScreenMessageFromPrefab(ref uiFunctionalites.OnScreenErrorMessagePrefab, ref uiFunctionalites.SearchItemNotFoundWarningMessageObject, "SearchItemNotFoundWarningMessage", uiFunctionalites.MessagesParent, message, "ScrollSearchController: Could not find searched item.");
                }
            }
        }
        public string GetTextItemFromGameObject(GameObject gameObject)
        {
            return gameObject.GetComponentInChildren<TMP_Text>().text;
        }

        /////////////////////////////////////////////// Scrolling Object Event Methods  ////////////////////////////////////////////////
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
}
