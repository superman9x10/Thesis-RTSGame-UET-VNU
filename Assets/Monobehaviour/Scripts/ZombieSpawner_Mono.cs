using System.Collections;
using UnityEngine;

public class ZombieSpawner_Mono : MonoBehaviour
{
    public GameObject zombiePrefab;
    public Transform targetPos;
    public Transform spawnPos;

    private void Start()
    {
        StartCoroutine(Spawner_ie());
    }
    private IEnumerator Spawner_ie()
    {
        while(true)
        {
            yield return new WaitForSeconds(Random.Range(1, 3));

            GameObject zombie = Instantiate(zombiePrefab, spawnPos);

            zombie.GetComponent<ZombieMono>().hqPos = targetPos;
        }

    }
}
