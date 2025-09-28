using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimaitonS : MonoBehaviour
{
    PlayerController controller;
    Rigidbody2D rb;
    Animator anim;
    private SpriteRenderer spriteRenderer;
    private float moveInput;

    int groundParamID;
    int lookParamID;
    int jumpParamID;
    int fallParamID;
    int walkParamID;

    private float timer;
    private float interval;

    void Start()
    {
        groundParamID = Animator.StringToHash("isOnGround");
        lookParamID = Animator.StringToHash("isLooked");
        jumpParamID = Animator.StringToHash("isJumping");
        fallParamID = Animator.StringToHash("isFalling");
        walkParamID = Animator.StringToHash("isWalking");

        Transform parent = transform.parent;

        controller = parent.GetComponent<PlayerController>();
        rb = parent.GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();

        spriteRenderer = GetComponent<SpriteRenderer>();

        ResetTimer();
    }

    void Update()
    {
        anim.SetBool("isOnGround", controller.isGrounded);
        anim.SetFloat("isFalling", rb.velocity.y);

        moveInput = Input.GetAxisRaw("Horizontal"); // A/D �� ��/�Ҽ� -1/0/1

        if (moveInput > 0) // ������
            spriteRenderer.flipX = false;
        else if (moveInput < 0) // ������
            spriteRenderer.flipX = true;

        if (moveInput != 0)
            anim.SetBool("isWalking", true);
        else
            anim.SetBool("isWalking", false);

        if (controller.isGrounded && Input.GetKeyDown(KeyCode.Space))
        {
            anim.SetTrigger("isJumping"); // ��������˲��Ķ���
        }

        timer -= Time.deltaTime;
        if (timer <= 0)
        {
            anim.SetTrigger("isLooked"); // ��������Idle����
            ResetTimer();
        }
    }

    void ResetTimer()
    {
        interval = Random.Range(5f, 10f); // ÿ5~10�봥��һ��
        timer = interval;
    }
}
