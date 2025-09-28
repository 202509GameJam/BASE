using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimaitonM : MonoBehaviour
{
    PlayerController controller;
    Rigidbody rb;
    Animator anim;

    int groundParamID;
    int lookParamID;
    int jumpParamID;
    int fallParamID;
    int walkParamID;

    bool islooked;

    void Start()
    {
        groundParamID = Animator.StringToHash("isOnGround");
        lookParamID = Animator.StringToHash("isLooking");
        jumpParamID = Animator.StringToHash("isJumping");
        fallParamID = Animator.StringToHash("verticalVelocity");
        walkParamID = Animator.StringToHash("isWalking");

        Transform parent = transform.parent;

        controller = parent.GetComponent<PlayerController>();
        rb = parent.GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        //anim.SetBool(lookParamID, )
        anim.SetBool(groundParamID, controller.isGrounded);
        anim.SetBool(groundParamID, controller.iswalking);
        anim.SetBool(jumpParamID, controller.isJumping);
        anim.SetFloat(fallParamID, rb.velocity.y);
    }

    //void looking()
    //{
    //    if (!islooked)
    //    {

    //        islooked = true;
    //    }
    //}
}
