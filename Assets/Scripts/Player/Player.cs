using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
    Animator _playerAnimator;
    Rigidbody2D _playerRb;
    Vector2 _movement;
    TextMeshProUGUI _moneyText;
    TextMeshProUGUI healthText; 
    int zombiesKilled = 0;
    private float timeAlive = 0f;
    
    [SerializeField] GameObject heartPrefab; 
    [SerializeField] GameObject healthPanel; 
    

    public bool HasWeapon { get; set; } = false;
    
    [SerializeField] LayerMask solidObjectsLayer;

    [SerializeField] float movespeed;

    [SerializeField] int money = 500;
    [SerializeField] int health = 5; 
    [SerializeField] float invincibilityDuration = 2f; 
    private bool isInvincible = false;
    
    
    void Awake()
    {
        _playerRb = GetComponent<Rigidbody2D>();
        _playerAnimator = GetComponent<Animator>();
        
        _moneyText = GameObject.FindWithTag("MoneyUI").GetComponent<TextMeshProUGUI>();
        healthText = GameObject.FindWithTag("LifeUI").GetComponent<TextMeshProUGUI>();
       UpdateHealthUI();
    }

    void OnMove(InputValue value)
    {
        _movement = value.Get<Vector2>();
    }
    
    void MovePlayer()
    {
        Vector3 targetPosition = _playerRb.position + _movement * (movespeed * Time.fixedDeltaTime);
        
        if (IsWalkable(targetPosition))
        {
            _playerRb.MovePosition(targetPosition); 
        }

        bool isWalking = _movement.magnitude > Mathf.Epsilon;
        _playerAnimator.SetBool("isWalking", isWalking);

        _playerAnimator.SetFloat("Horizontal", _movement.x);
        _playerAnimator.SetFloat("Vertical", _movement.y);
    }

    bool IsWalkable(Vector3 targetPosition)
    {
        return Physics2D.OverlapCircle(targetPosition, 0.2f, solidObjectsLayer) is null;
    }
    
    public void TakeDamage(int amount)
    {
        if (isInvincible) return;
        
        health -= amount; 
        UpdateHealthUI();

        if (health <= 0)
        {
            PlayerPrefs.SetFloat("TimeAlive", timeAlive);
            PlayerPrefs.SetInt("ZombiesKilled", zombiesKilled);
            SceneManager.LoadScene("GameOver");
        }
        else
        {
            StartCoroutine(InvincibilityCoroutine());
        }
    }
    public void ZombieKilled()
    {
        zombiesKilled++;
    }
    IEnumerator InvincibilityCoroutine()
    {
        isInvincible = true; 
        
        float elapsed = 0f;
       
        while (elapsed < invincibilityDuration)
        {
            yield return new WaitForSeconds(0.2f); 
            elapsed += 0.1f;
        }
        isInvincible = false; 
    }

    void UpdateHealthUI()
    {
        foreach (Transform child in healthPanel.transform)
        {
            Destroy(child.gameObject);
        }
        
        for (int i = 0; i < health; i++)
        {
            Instantiate(heartPrefab, healthPanel.transform);
        }
    }

    public bool SpendMoney(int amount)
    {
        if (money < amount) return false;
        
        money -= amount;
        UpdateMoneyUI();
        return true;
    }

    void UpdateMoneyUI()
    {
        _moneyText.text = money.ToString();
    }

    public GameObject bullet, spawnerBulletPos;
    void Update()
    {
        timeAlive += Time.deltaTime;
        Vector3 mousePos = Input.mousePosition;
        mousePos = Camera.main.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, Camera.main.nearClipPlane));

      
        Vector2 direction = new Vector2(mousePos.x - spawnerBulletPos.transform.position.x, mousePos.y - spawnerBulletPos.transform.position.y);
        direction.Normalize();

        if (Input.GetMouseButtonDown(0))
        {
            
            GameObject newBullet = Instantiate(bullet, spawnerBulletPos.transform.position, Quaternion.identity);
            
            newBullet.transform.up = direction;
        }
        MovePlayer();
    }
}