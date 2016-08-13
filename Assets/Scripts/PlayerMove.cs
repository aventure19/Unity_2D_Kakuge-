using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CharacterController))]
public class PlayerMove : MonoBehaviour
{

    public float Speed = 120;
    public float JumpPower = 100;
    public float gravity = 9.8f;
    public Vector3 MoveDirection;
    CharacterController controller;

    // Use this for initialization
    void Start()
    {

        // キャラクターコントローラー
        controller = GetComponent<CharacterController>();

    }

    void Reset()
    {
        controller = GetComponent<CharacterController>();
        controller.center = Vector3.up * 2.5f;
        controller.height = 5;
    }

    // Update is called once per frame
    void Update()
    {

        // 重力
        if (!controller.isGrounded) MoveDirection.y -= gravity;

        // 入力受け付け
        float dx = Input.GetAxis("Horizontal");
        MoveDirection.x = dx * Speed;

        // ジャンプの処理
        if (controller.isGrounded && Input.GetButtonDown("Jump")) MoveDirection.y = JumpPower;

        // 移動の確定
        controller.Move(MoveDirection * Time.deltaTime);

    }
}
