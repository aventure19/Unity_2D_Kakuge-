using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(CharacterController))]
public class ElementScript : MonoBehaviour
{

    public float Speed = 5;
    public float JumpPower = 10;
    public float Gravity = 9.8f;

    private Animator animator;
    private CharacterController controller;

    [SerializeField]
    private Vector3 moveDirection;

    void Awake()
    {

        Application.targetFrameRate = 60;

    }

    // Use this for initialization
    void Start()
    {

        animator = GetComponent<Animator>();
        controller = GetComponent<CharacterController>();


    }

    // Update is called once per frame
    void Update()
    {

        // 地上
        if (controller.isGrounded == true)
        {

            // 水平移動
            moveDirection.x = Input.GetAxis("Horizontal");

            // ジャンプ
            if (Input.GetAxis("Vertical") > 0.8f)
            {

                moveDirection.y = JumpPower;

                animator.SetTrigger("Jump");

            }

            animator.SetBool("IsGrounded", true);

            // パンチ
            if (Input.GetButtonDown("Fire1") == true) animator.SetTrigger("Punch");

            // キック
            if (Input.GetButtonDown("Fire2") == true) animator.SetTrigger("Kick");

        }
        // 空中
        else
        {

            animator.SetBool("IsGrounded", false);

            moveDirection.y -= Gravity * Time.deltaTime;

        }

        // 移動確定
        controller.Move(moveDirection * Speed * Time.deltaTime);

        // アニメーターに横移動速度を渡す
        animator.SetFloat("Speed", Mathf.Abs(moveDirection.x));

    }

    void OnGUI()
    {

        float fps = 1 / Time.deltaTime;

        GUILayout.Label(fps.ToString());

    }


}
