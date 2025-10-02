using System.Collections.Generic;
using UnityEngine;

public class PoolManager : MonoBehaviour
{
    [SerializeField] private GameObject prefab;      // 어떤 프리팹이든 가능
    [SerializeField] private int initialCount = 10;

    private List<GameObject> pool = new();

    private void Awake()
    {
        for (int i = 0; i < initialCount; i++)
        {
            var obj = Instantiate(prefab, transform);
            obj.SetActive(false);
            pool.Add(obj);
        }
    }

    public GameObject Get()
    {
        foreach (var obj in pool)
        {
            if (!obj.activeSelf)
            {
                obj.SetActive(true);
                return obj;
            }
        }

        var newObj = Instantiate(prefab, transform);
        pool.Add(newObj);
        return newObj;
    }

    public void ReleaseAll()
    {
        foreach (var obj in pool)
            obj.SetActive(false);
    }
}
