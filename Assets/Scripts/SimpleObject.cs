using UnityEngine;

public class SimpleObject : MonoBehaviour
{
    Vector3 velocity;

    void Start()
    {
        velocity = new Vector3(Random.Range(-1f, 1f), Random.Range(2f, 3f), Random.Range(-1f, -2f));
    }

    void Update()
    {
        velocity = new Vector3(velocity.x, velocity.y - (5 * Time.deltaTime), velocity.z);
        transform.Translate(velocity * Time.deltaTime);
        if (transform.position.y < -5) Destroy(gameObject);
    }
}