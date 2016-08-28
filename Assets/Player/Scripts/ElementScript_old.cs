using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(CharacterController))]
public class ElementScript_old : MonoBehaviour
{

    public float Speed = 4;
    public float JumpPower = 5;
    public float Gravity = 9.8f;

    // animation遷移にかける時間
    public float TransitionDuration = 0.01f;

    // 敵の位置
    public Transform Enemy;

    private Animator animator;
    private CharacterController controller;

    private enum State
    {
        Move,
        Jump,
        Punch,
        Kick
    }

    [SerializeField]
    private State state;
    private State oldState;

    [SerializeField]
    private Vector3 moveDirection;

    // Use this for initialization
    void Start()
    {

        animator = GetComponent<Animator>();
        controller = GetComponent<CharacterController>();

        // Stateの初期化
        state = oldState = State.Move;


    }

    // Update is called once per frame
    void Update()
    {

        // stateの値をもとに状態遷移
        switch (state)
        {

            case State.Move:

                // 状態遷移時に一回だけ呼ばれる
                if (oldState != state) animator.CrossFadeInFixedTime("Move", TransitionDuration);

                // 水平移動
                moveDirection.x = Input.GetAxis("Horizontal");

                // 方向転換
                LookAtEnemy();

                // アニメーターに横移動速度を渡す
                animator.SetFloat("Speed", Mathf.Abs(moveDirection.x));

                // 状態遷移
                if (Input.GetButtonDown("Fire1")) state = State.Punch;
                else if (Input.GetButtonDown("Fire2")) state = State.Kick;
                else if (Input.GetAxis("Vertical") > 0.8f) state = State.Jump;

                // oldStateに現在のステートを代入
                oldState = State.Move;

                break;

            case State.Jump:

                // 状態遷移時に一回だけ呼ばれる
                if (oldState != state)
                {

                    moveDirection.y = JumpPower;

                    animator.CrossFadeInFixedTime("Jump", TransitionDuration);

                }

                // 状態遷移
                if (controller.isGrounded)
                {
                    state = State.Move;
                }
                else
                {
                    moveDirection.y -= Gravity * Time.deltaTime;
                }

                // oldStateに現在のステートを代入
                oldState = State.Jump;

                break;

            case State.Punch:

                // 状態遷移時に一回だけ呼ばれる
                if (oldState != state)
                {

                    animator.CrossFadeInFixedTime("Jab", TransitionDuration);

                    moveDirection.x = 0;

                }

                // 状態遷移
                if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1) state = State.Move;

                // oldStateに現在のステートを代入
                oldState = State.Punch;

                break;

            case State.Kick:

                // 状態遷移時に一回だけ呼ばれる
                if (oldState != state)
                {

                    animator.CrossFadeInFixedTime("Kick", TransitionDuration);

                    moveDirection.x = 0;

                }

                // 状態遷移
                if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1) state = State.Move;

                // oldStateに現在のステートを代入
                oldState = State.Kick;

                break;

            default:

                state = oldState = State.Move;

                break;

        }

        // 移動確定
        controller.Move(moveDirection * Speed * Time.deltaTime);

    }

    // 敵の方向を向くメソッド
    public void LookAtEnemy()
    {

        int t;

        if (transform.position.x < Enemy.transform.position.x) t = 90;
        else t = -90;

        transform.rotation = Quaternion.Euler(Vector3.up * t);

    }

    // 攻撃を受けたときに呼ばれるメソッド
    public void isHited()
    {

        Debug.Log(gameObject.ToString() + "isHited");

    }

    // 攻撃判定を作る
    public void AttackStart(string PartName)
    {

        // 全探索
        Transform[] transformArray = transform.GetComponentsInChildren<Transform>();

        foreach(Transform child in transformArray)
        {

            if (child.name == PartName)
            {

                try
                {

                    child.GetComponent<Collider>().enabled = true;

                }
                catch (System.Exception e)
                {

                    Debug.Log(e);
                    
                }

                break;

            }

        }

    }

    // 攻撃判定を消す
    public void AttackEnd(string PartName)
    {

        // 全探索
        Transform[] transformArray = transform.GetComponentsInChildren<Transform>();

        foreach (Transform child in transformArray)
        {

            if (child.name == PartName)
            {

                try
                {

                    child.GetComponent<Collider>().enabled = false;

                }
                catch (System.Exception e)
                {

                    Debug.Log(e);

                }

                break;

            }

        }

    }

    void OnTriggerEnter(Collider other)
    {

        // 自分の攻撃には反応しない
        if (other.tag == "Attack" && other.transform.root != transform)
        {

            isHited();

        }

    }

}
