using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolManager : Singleton<PoolManager> {
    private Dictionary<int, Pool> pools = new Dictionary<int, Pool>();

    [SerializeField]
    private Transform poolContainer;

    private void Start() {
        if (poolContainer != null) {
            poolContainer.gameObject.SetActive(false);
        }
        else {
            // error
        }
    }

    public void InitPool(GameObject original, int initialSize, int maxSize) {
        CheckPoolExistence(original);
        for (int i = 0; i < initialSize; i++) {
            GameObject clone = Object.Instantiate(original) as GameObject;
            clone.transform.SetParent(poolContainer, false);
            //clone.SetActive(false);
            pools[original.GetInstanceID()].StoreObject(clone);
        }
    }

    public GameObject SpawnObject(GameObject type, Transform parent) {
        if (!pools.ContainsKey(type.GetInstanceID())) {
            Pool newPool = new Pool();
            newPool.ObjectToClone = type;
            pools.Add(type.GetInstanceID(), newPool);
        }
        GameObject objectToSpawn = pools[type.GetInstanceID()].GetObject();
        //objectToSpawn.SetActive(true);
        objectToSpawn.transform.SetParent(parent, false);
        return objectToSpawn;
    }

    public void DespawnObject(GameObject original, GameObject objectToKill) {
        objectToKill.transform.SetParent(poolContainer, false);
        //objectToKill.SetActive(false);
        CheckPoolExistence(original);
        pools[original.GetInstanceID()].StoreObject(objectToKill);
    }

    public void DespawnChildren(GameObject original, Transform parent) {
        while (parent.childCount > 0) {
            DespawnObject(original, parent.GetChild(0).gameObject);
        }
    }

    private void CheckPoolExistence(GameObject original) {
        if (!pools.ContainsKey(original.GetInstanceID())) {
            Pool pool = new Pool();
            //pool.MaxSize = maxSize;
            pool.ObjectToClone = original;
            //pool.InitialSize = initialSize;
            pools.Add(original.GetInstanceID(), pool);
        }
    }
}