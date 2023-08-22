using UnityEngine;
using System;
using Random = UnityEngine.Random;

public class ParticleSpawner : MonoBehaviour
{
    [SerializeField]
    private GameObject particlePrefab;

    private float xMin, xMax, yMin, yMax;

    public int particleCount = 50;
    public float particleSpeed = 15f;
    public Particle.ParticleDirection currentDirection;


    private void Awake()
    {
        // 개수와 속도 조절이 필요한 경우 ParticleSpawner 프리팹을 선택하고 인스펙터에서 수치 조정!
        currentDirection = Particle.ParticleDirection.None;
    }

    private Vector3 GetRandomPosition()
    {
        Vector3 pos = Vector3.zero;

        switch (currentDirection)
        {
            case Particle.ParticleDirection.None:
                pos.x = Random.Range(xMin, xMax);
                pos.y = Random.Range(yMin, yMax);
                break;
            case Particle.ParticleDirection.Up:
                pos.x = Random.Range(xMin, xMax);
                pos.y = yMin;
                break;
            case Particle.ParticleDirection.Down:
                pos.x = Random.Range(xMin, xMax);
                pos.y = yMax;
                break;
            case Particle.ParticleDirection.Left:
                pos.x = xMax;
                pos.y = Random.Range(yMin, yMax);
                break;
            case Particle.ParticleDirection.Right:
                pos.x = xMin;
                pos.y = Random.Range(yMin, yMax);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        return pos;
    }

    public void SetSpawnerPosition(int sizeX, int sizeY)
    {
        xMin = -3f;
        yMin = -3f;
        xMax = sizeX + 4;
        yMax = sizeY + 4;
        Debug.Log($"Set particle spawner worldPos to ({xMax}, {yMax})");
    }

    public void SpawnParticle()
    {
        Vector3 spawnPosition = GetRandomPosition();
        GameObject particle = Instantiate(particlePrefab, spawnPosition, Quaternion.identity);
        particle.transform.SetParent(gameObject.transform);
        particle.GetComponent<Particle>().SetInitialMovement(currentDirection, particleSpeed, xMin, xMax, yMin, yMax);
    }

    public void DestroyAllParticles()
    {
        foreach (Transform t in GetComponentsInChildren<Transform>())
        {
            if (t == transform) continue;
            Destroy(t.gameObject);
        }
    }

    public void SpawnInitialParticles(int mapSizeX, int mapSizeY)
    {
        SetSpawnerPosition(mapSizeX, mapSizeY);

        DestroyAllParticles();

        currentDirection = Particle.ParticleDirection.None;

        for (int i = 0; i < particleCount; i++)
        {
            SpawnParticle();
        }
    }

    public void ModifyDirection(Particle.ParticleDirection direction)
    {
        currentDirection = direction;

        foreach (Particle p in GetComponentsInChildren<Particle>())
        {
            p.SetMovementDirection(currentDirection);
        }
    }
}
