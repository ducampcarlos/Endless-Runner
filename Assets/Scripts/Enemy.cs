using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f; // Speed of the enemy movement
    private float ZLimit = -10f; // Z position limit for the enemy

    public float MoveSpeed
    {
        get => moveSpeed;
        set => moveSpeed = value;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ZLimit = Camera.main.gameObject.transform.position.z;
    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(Vector3.back * moveSpeed * Time.deltaTime, Space.World);

        // Check if the enemy is out of bounds
        if (transform.position.z < ZLimit)
        {
            GameManager.Instance.ScoreUp(); // Increment the score in the GameManager
            gameObject.SetActive(false);
        }
    }
}
