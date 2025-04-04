using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f; // Speed of the enemy movement

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(Vector3.back * moveSpeed * Time.deltaTime, Space.World);

        // Check if the enemy is out of bounds
        if (transform.position.z < -12f)
        {
            // Destroy the enemy if it goes out of bounds
            Destroy(gameObject);
        }
    }
}
