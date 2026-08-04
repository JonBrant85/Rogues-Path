using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    const float XSpacing = 1.7321f;
    const float ZSpacing = 1.5f;
    const float XSubSpacing = 0.86607f;
    [Range(1f, 3f)]
    public float hexMargin = 1f;
    [Header("Hexagons to Instantiate")]
    public List<GameObject> HexagonTiles;
    [Header("Hexagons Field Size")]
    public int sizeX;
    public int sizeZ;
    [Header("Random Y pos (Step is 0.5)")]
    public bool RandomY = false;
    public float maxY;
    [Range(0,100)]
    public int chanceY;
    [Header("Random rotation")]
    public bool randomRot = false;
    [Header("Fill settings")]
    public bool addFillParts = false;
    [Range(0, 100)]
    public int chanceFill;
    public List<GameObject> FillPrefabs;
    public bool randomizeFillChilds = false;
    public bool randomizeChildsRotation = false;


    public void GenerateField()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }

        for (int i = 0; i < sizeX; i++)
        {
            for (int a = 0; a < sizeZ; a++)
            {
                Vector3 pos = new Vector3(i * (XSpacing * hexMargin), 0f, a * (ZSpacing * hexMargin));

                if (a % 2 != 0) pos.x += (XSubSpacing * hexMargin);

                if (RandomY)
                {
                    if(Random.Range(0,100) <= chanceY)
                    pos.y = Random.Range(1, (int)(maxY / 0.5f)) * 0.5f;
                }
                GameObject newHex = Instantiate(HexagonTiles[Random.Range(0, HexagonTiles.Count)], pos, new Quaternion());
                newHex.transform.parent = transform;

                if (randomRot) newHex.transform.eulerAngles = new Vector3(0f, Random.Range(0, 7) * 60f, 0f);

                if(addFillParts)
                {
                    if (Random.Range(0, 100) <= chanceFill)
                    {
                        List<GameObject> toDestroy = new List<GameObject>();
                        GameObject fill = Instantiate(FillPrefabs[Random.Range(0, FillPrefabs.Count)], newHex.transform);
                        fill.transform.localPosition = new Vector3(0f, -1f, 0f);
                        switch (Random.Range(0, 4))
                        {
                            case 0:
                                {
                                    fill.transform.localEulerAngles = new Vector3(0, 0, 0);
                                    break;
                                }
                            case 1:
                                {
                                    fill.transform.localEulerAngles = new Vector3(0, 90, 0);
                                    break;
                                }
                            case 2:
                                {
                                    fill.transform.localEulerAngles = new Vector3(0, 180, 0);
                                    break;
                                }
                            case 3:
                                {
                                    fill.transform.localEulerAngles = new Vector3(0, 270, 0);
                                    break;
                                }
                        }
                        if (randomizeFillChilds)
                        {
                            for (int o = 0; o < fill.transform.childCount; o++)
                            {
                                fill.transform.GetChild(o).gameObject.SetActive(Random.Range(0, 2) == 0 ? false : true);
                                if (fill.transform.GetChild(o).gameObject.activeSelf)
                                {
                                    if (randomizeChildsRotation)
                                    {
                                        fill.transform.GetChild(o).localEulerAngles = new Vector3(0, Random.Range(0f, 360f), 0);
                                    }
                                }
                                else
                                {
                                    toDestroy.Add(fill.transform.GetChild(o).gameObject);
                                }
                            }

                            for (int o = toDestroy.Count - 1; o >= 0; o--)
                            {
                                DestroyImmediate(toDestroy[o]);
                            }
                        }
                    }
                }
            }
        }
    }
    public void DeleteField()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }
    }
}
