using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public int id;
    public string username;
    public CharacterController controller;
    public Transform shootOrigin;
    public float gravity = -9.81f;
    public float moveSpeed = 5f;
    public float jumpSpeed = 5f;
    public float health;
    public float maxHealth = 100f;
    public int score = 0;

    public Animator animator;
    private bool[] inputs;
    public bool isMoving;
    private float yVelocity = 0;

    private void Start()
    {
        gravity *= Time.fixedDeltaTime * Time.fixedDeltaTime;
        moveSpeed *= Time.fixedDeltaTime;
        jumpSpeed *= Time.fixedDeltaTime;
        isMoving = false;
    }
    public void Initialize(int _id, string _username)
    {
        id = _id;
        username = _username;
        inputs = new bool[5];
        health = maxHealth;
    }

    public void FixedUpdate()
    {
        if (health <= 0)
            return;

        Vector2 _inputDirection = Vector2.zero;
        if (inputs[0])
        {
            _inputDirection.y += 1;
        }
        if (inputs[1])
        {
            _inputDirection.y -= 1;
        }
        if (inputs[2])
        {
            _inputDirection.x += 1;
        }
        if (inputs[3])
        {
            _inputDirection.x -= 1;
        }
        Move(_inputDirection);
    }

    private void Move(Vector2 _inputDirection)
    {
        Vector3 _moveDirection = transform.right * _inputDirection.x + transform.forward * _inputDirection.y;
        _moveDirection *= moveSpeed;

        if (_moveDirection != Vector3.zero)
        {
            animator.SetBool("isRunning", true);
        }
        else
        {
            animator.SetBool("isRunning", false);

        }

        if (controller.isGrounded)
        {
            yVelocity = 0;
            if (inputs[4])
            {
                yVelocity = jumpSpeed;
            }
        }
        yVelocity += gravity;

        _moveDirection.y = yVelocity;

        controller.Move(_moveDirection);

        ServerSend.PlayerPosition(this);
        ServerSend.PlayerRotation(this);
        ServerSend.PlayerAnimation(this);
    }
    public void SetInput(bool[] _inputs, Quaternion _rotation)
    {
        inputs = _inputs;
        transform.rotation = _rotation;
    }

    public void Shoot(Vector3 _viewDirection)
    {
        if(health <= 0)
        {
            return;
        }

        ServerSend.PlayerShoot(this);

        if(Physics.Raycast(shootOrigin.position, _viewDirection, out RaycastHit _hit, 25f))
        {
            if (_hit.collider.CompareTag("Player"))
            {
                ServerSend.PlayerHit(this);
                _hit.collider.gameObject.GetComponent<Player>().TakeDamage(50f);
                if (_hit.collider.gameObject.GetComponent<Player>().health <= 0)
                    score++;
            }
        }
    }

    public void TakeDamage(float _damage)
    {
        if (health <= 0)
            return;

        health -= _damage;
        if(health <= 0f)
        {
            health = 0f;
            controller.enabled = false;
            Transform spawnTransform = NetworkManager.instance.spawnPoints[Random.Range(0, 7)];
            transform.position = spawnTransform.position;
            transform.rotation = spawnTransform.rotation;
            ServerSend.PlayerPosition(this);
            ServerSend.PlayerRespawnRotation(this);
            StartCoroutine(Respawn());
        }

        ServerSend.PlayerHealth(this);
    }

    private IEnumerator Respawn()
    {
        yield return new WaitForSeconds(5f);

        health = maxHealth;
        controller.enabled = true;
        ServerSend.PlayerRespawned(this);
    }
}
