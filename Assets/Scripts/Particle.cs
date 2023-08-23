using UnityEngine;

public class Particle : MonoBehaviour
{
    public enum ParticleDirection { None = 0, Up = 1, Down = 2, Left = 3, Right = 4 }

    private float _xMin = 0f, _xMax = 0f, _yMin = 0f, _yMax = 0f;
    private ParticleDirection _direction = ParticleDirection.None;
    private float _speed = 0f;

    private void Update()
    {
        switch (_direction)
        {
            case ParticleDirection.Up:
                transform.Translate(_speed * Time.deltaTime * Vector3.up);
                break;
            case ParticleDirection.Down:
                transform.Translate(_speed * Time.deltaTime * Vector3.down);
                break;
            case ParticleDirection.Left:
                transform.Translate(_speed * Time.deltaTime * Vector3.left);
                break;
            case ParticleDirection.Right:
                transform.Translate(_speed * Time.deltaTime * Vector3.right);
                break;
            case ParticleDirection.None:
                break;
        }

        // Particle out of area
        if (transform.position.x < _xMin || transform.position.x > _xMax ||
            transform.position.y < _yMin || transform.position.y > _yMax)
        {
            GameManager.mm.particleSpawner.GetComponent<ParticleSpawner>().SpawnParticle();
            Destroy(gameObject);
        }
    }

    public void SetInitialMovement(ParticleDirection direction, float speed, float xMin, float xMax, float yMin, float yMax)
    {
        _direction = direction;
        _speed = speed;
        _xMin = xMin;
        _xMax = xMax;
        _yMin = yMin;
        _yMax = yMax;
    }

    public void SetMovementDirection(ParticleDirection direction)
    {
        _direction = direction;
    }

    public void SetMovementSpeed(float speed)
    {
        _speed = speed;
    }
}
