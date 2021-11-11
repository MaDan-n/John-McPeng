using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProceduralGeneration : Singleton<ProceduralGeneration>
{
    //References
    public GameObject tilePrefab;

    //Attributes
    public int verticalTiles;
    public int horizontalPercentage;


    //Slice
    public int sliceFilling;
    public int filling;
    [SerializeField]
    private int sliceDistance;
    [SerializeField]
    private int numberOfHorizontalTiles;

    //Map limits
    [SerializeField]
    private float leftHorizontalLimit;
    [SerializeField]
    private float rightHorizontalLimit;
    private float currentTopLimit;

    //Lists
    List<GameObject> tilesList;
    List<GameObject> currentHorizontalPathList;

    public enum pathDirection
    {
        vertical,
        horizontal
    }

    private void Start()
    {
        tilesList = new List<GameObject>();

        //Initialize map limits
        float width = verticalTiles - (verticalTiles * horizontalPercentage) / 100;
        leftHorizontalLimit = (width / 2) * -1;
        rightHorizontalLimit = (width / 2) * 1;

        //For horizontal paths
        currentHorizontalPathList = new List<GameObject>();
        sliceDistance = verticalTiles - (verticalTiles * sliceFilling) / 100;
        numberOfHorizontalTiles = ((int)rightHorizontalLimit * filling) / 100;

        StartCoroutine(CreatePath());
    }

    IEnumerator CreatePath()
    {
        InstantiateTile(new Vector3(0, 0, 0), null, pathDirection.vertical);

        //This create vertical main path
        for(int i = 1; i < verticalTiles; i++)
        {
            Vector3 tilePosition = GetPosition(tilesList[i - 1].transform.position);
            InstantiateTile(tilePosition, tilesList[i - 1], pathDirection.vertical);
            yield return new WaitForSeconds(0.1f);
        }

        //Draw lateral limits lines
        Debug.DrawLine(new Vector3(leftHorizontalLimit, tilesList[0].transform.position.y, 0), new Vector3(leftHorizontalLimit, tilesList[tilesList.Count - 1].transform.position.y, 0), Color.red, 100f);
        Debug.DrawLine(new Vector3(rightHorizontalLimit, tilesList[0].transform.position.y, 0), new Vector3(rightHorizontalLimit, tilesList[tilesList.Count - 1].transform.position.y, 0), Color.red, 100f);

        StartCoroutine(CreateHorizontalPath());
    }

    IEnumerator CreateHorizontalPath()
    {
        List<GameObject> SliceList = new List<GameObject>();

        //Go through the whole main path, and slice it in equal parts
        for(int i = 0; i < tilesList.Count; i+= sliceDistance)
        {
            GameObject selectedTile = tilesList[i];
            SliceList.Add(selectedTile);
            selectedTile.transform.name = "Slice";

            //Draw limits lines
            Debug.DrawLine(new Vector3(leftHorizontalLimit, selectedTile.transform.position.y, 0), new Vector3(rightHorizontalLimit, selectedTile.transform.position.y, 0), Color.yellow, 100f);
        }

        //Draw a a top limit line for the last tile of the main path
        Debug.DrawLine(new Vector3(leftHorizontalLimit, tilesList[tilesList.Count - 1].transform.position.y, 0), new Vector3(rightHorizontalLimit, tilesList[tilesList.Count - 1].transform.position.y, 0), Color.yellow, 100f);

        //Repeat for each slice
        for (int i = 0; i < SliceList.Count; i++)
        {
            Debug.Log(i + 1 + "° Slice");

            GameObject selectedSlice = SliceList[i];

            //Initialize current top limit
            if (i < SliceList.Count - 1)
                currentTopLimit = SliceList[i + 1].transform.position.y;

            else
                currentTopLimit = tilesList[tilesList.Count - 1].transform.position.y;

            currentHorizontalPathList.Clear();

            //Add slice tile in temporary horizontal list
            currentHorizontalPathList.Add(selectedSlice);

            for (int h = 1; h < numberOfHorizontalTiles; h++)
            {
                GameObject currentReferenceTile = currentHorizontalPathList[h - 1];

                Debug.Log("Tile");
                Vector3 tilePosition = GetHorizontalPosition(currentReferenceTile.transform.position);

                Debug.Log("Position done");
                InstantiateTile(tilePosition, currentReferenceTile, pathDirection.horizontal);
                yield return new WaitForSeconds(0.2f);
            }
        }
    }

    void InstantiateTile(Vector3 _position, GameObject _precedentTile, pathDirection _direction)
    {
        //Create tile
        GameObject tileObject = Instantiate(tilePrefab, _position, Quaternion.identity);
        tileObject.GetComponent<Tile>().precedentTile = _precedentTile;
        tileObject.transform.SetParent(gameObject.transform);

        //Add it in the tiles list
        tilesList.Add(tileObject);

        if (_direction == pathDirection.horizontal)
            currentHorizontalPathList.Add(tileObject);

        Debug.Log("Instantiated");
    }

    Vector3 GetPosition(Vector3 _position)
    {
        //That + 1 it's cuz position is calculated at the center of the mass, so we have to ignore half cube + half cube
        float mainLower = 1.5f + 1;
        float mainHigher = 3f + 1;
        float secondaryLower = 1f + 1;
        float secondaryHigher = 1.5f + 1;

        bool isPossible = false;
        Vector3 position;

        do
        {
            //Choose if to go up/down and left/right
            float xDir = Random.value < .5f ? -1f : 1f;

            position = new Vector3(Random.Range(secondaryLower, secondaryHigher) * xDir, Random.Range(mainLower, mainHigher), 0f);

            //Check if the tile is out of the bound
            if (_position.x + position.x > leftHorizontalLimit && _position.x + position.x < rightHorizontalLimit)
                isPossible = true;

            else
            {
                mainHigher -= .5f;
                secondaryHigher -= .5f;
            }
        }

        while (!isPossible);

        return _position + position;
    }

    Vector3 GetHorizontalPosition(Vector3 _position)
    {
        //That + 1 it's cuz position is calculated at the center of the mass, so we have to ignore half cube + half cube
        float mainLower = 2f + 1;
        float mainHigher = 3.5f + 1;
        float secondaryLower = 1.5f + 1;
        float secondaryHigher = 2.5f + 1;

        Vector3 position;
        bool isFree = false;

        do
        {
            //Choose if to go left/right
            float xDir = Random.value < .5 ? -1f : 1f;

            //position = new Vector3(Random.Range(mainLower, mainHigher) * xDir, Random.Range(secondaryLower, secondaryHigher), 0f);
            position = new Vector3(Random.Range(secondaryLower, secondaryHigher) * xDir, Random.Range(mainLower, mainHigher), 0f);

            //Check if the tile go out od the current top limit
            if (_position.y + position.y > currentTopLimit)
                position = new Vector3(Random.Range(mainLower, mainHigher) * xDir, Random.Range(secondaryLower - .5f, secondaryHigher - 1), 0f);

            //Check if the tile go out od the lateral limits
            if (_position.x + position.x < leftHorizontalLimit || _position.x + position.x > rightHorizontalLimit)
                    position.x *= -1;

            //Check if in that position there is another tile
            if (!Physics.CheckSphere(_position + position, 1.5f) && _position.x + position.x > leftHorizontalLimit && _position.x + position.x < rightHorizontalLimit)
                isFree = true;

            else
            {
                mainHigher -= .2f;
                secondaryHigher -= .2f;
            }
        }

        while (!isFree);

        return _position + position;
    }
}