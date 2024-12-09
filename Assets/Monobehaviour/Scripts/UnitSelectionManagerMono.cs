using UnityEngine;
using System;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
public class UnitSelectionManagerMono : MonoBehaviour
{
    public static UnitSelectionManagerMono Instance { get; private set; }

    public event EventHandler OnSelectionAreaStart;
    public event EventHandler OnSelectionAreaEnd;

    private Vector2 selectionStartMousePosition;
    public List<GameObject> units = new List<GameObject>();

    private List<SoldierMono> soldiers = new List<SoldierMono>();
    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            selectionStartMousePosition = Input.mousePosition;

            OnSelectionAreaStart?.Invoke(this, EventArgs.Empty);
        }

        if (Input.GetMouseButtonUp(0))
        {
            ResetSelectedUnit();

            Rect selectionAreaRect = GetSelectionAreaRect();
            float selectionAreaSize = selectionAreaRect.width + selectionAreaRect.height;
            float multipleSelectionSizeMin = 40f;
            bool isMultipleSelection = selectionAreaSize > multipleSelectionSizeMin;
            
            if (isMultipleSelection)
            {
                // Multi select
                foreach (GameObject unit in units)
                {
                    Vector2 unitScreenPosition = Camera.main.WorldToScreenPoint(unit.transform.position);

                    if (selectionAreaRect.Contains(unitScreenPosition))
                    {
                        SoldierMono _soldier = unit.GetComponent<SoldierMono>();
                        _soldier.Selected();
                        soldiers.Add(_soldier);

                        Debug.Log(unit.name + " nằm trong vùng Rect");
                    }
                }
            }
            else
            {
                // Single select

                Vector3 mousePosition = Input.mousePosition;
                Ray cameraRay = Camera.main.ScreenPointToRay(mousePosition);

                if (Physics.Raycast(cameraRay, out RaycastHit hitInfo))
                {
                    if (hitInfo.collider.CompareTag("Selectable"))
                    {
                        SoldierMono _soldier = hitInfo.collider.gameObject.GetComponent<SoldierMono>();
                        _soldier.Selected();
                        soldiers.Add(_soldier);

                        Debug.Log("Trúng đối tượng: " + hitInfo.collider.gameObject.name);
                    }
                }
            }

            OnSelectionAreaEnd?.Invoke(this, EventArgs.Empty);
        }

        if (Input.GetMouseButtonDown(1))
        {
            Vector3 mouseWorldPosition = MouseWorldPosition.Instance.GetPosition();
            Vector3[] movePositionArray = GenerateMovePositionArray(mouseWorldPosition, soldiers.Count);

            for (int i = 0; i < soldiers.Count; i++) 
            {
                soldiers[i].SetMoveTarget(movePositionArray[i]);
            }
        }
    }
    private void ResetSelectedUnit()
    {
        soldiers.Clear();
        units.Clear();
        units = GameObject.FindGameObjectsWithTag("Selectable").ToList();

        for(int i = 0; i < units.Count; i++)
        {
            units[i].GetComponent<SoldierMono>().UnSelected();
        }
    }

    private Vector3[] GenerateMovePositionArray(Vector3 targetPosition, int positionCount)
    {
        Vector3[] positionArray = new Vector3[positionCount];
        
        if (positionCount == 0)
        {
            return positionArray;
        }
        positionArray[0] = targetPosition;
        if (positionCount == 1)
        {
            return positionArray;
        }

        float ringSize = 2.2f;
        int ring = 0;
        int positionIndex = 1;

        while (positionIndex < positionCount)
        {
            int ringPositionCount = 3 + ring * 2;

            for (int i = 0; i < ringPositionCount; i++)
            {
                float angle = i * (Mathf.PI * 2 / ringPositionCount);

                Vector3 ringVector = RotateVectorAroundY(angle, ringSize * (ring + 1));
                Vector3 ringPosition = targetPosition + ringVector;

                positionArray[positionIndex] = ringPosition;
                positionIndex++;

                if (positionIndex >= positionCount)
                {
                    break;
                }
            }
            ring++;
        }

        return positionArray;
    }

    Vector3 RotateVectorAroundY(float angle, float distance)
    {
        float x = distance * Mathf.Cos(angle);
        float z = distance * Mathf.Sin(angle);
        return new Vector3(x, 0, z);
    }

    public Rect GetSelectionAreaRect()
    {
        Vector2 selectionEndMousePosition = Input.mousePosition;

        Vector2 lowerLeftCorner = new Vector2(
            Mathf.Min(selectionStartMousePosition.x, selectionEndMousePosition.x),
            Mathf.Min(selectionStartMousePosition.y, selectionEndMousePosition.y));

        Vector2 upperRightCorner = new Vector2(
            Mathf.Max(selectionStartMousePosition.x, selectionEndMousePosition.x),
            Mathf.Max(selectionStartMousePosition.y, selectionEndMousePosition.y));

        return new Rect(
            lowerLeftCorner.x,
            lowerLeftCorner.y,
            upperRightCorner.x - lowerLeftCorner.x,
            upperRightCorner.y - lowerLeftCorner.y
        );
    }
}
