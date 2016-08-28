using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(CharacterController))]
public class ElementScript : MonoBehaviour
{

    // inspectorで編集可能なフィールド
    public float Speed = 4;
    public float JumpPower = 5;
    public float Gravity = 9.8f;
    public int PlayerNumber;

    // 敵の位置
    public Transform Enemy;

    // 状態
    private enum State
    {
        Move,
        Jump,
        DuringAttack,
        DamageDown
    }

    [SerializeField]
    private State state;

    // 各コンポーネント
    private Animator animator;
    private CharacterController controller;

    [SerializeField]
    private Vector3 moveDirection;

    // Use this for initialization
    void Start()
    {

        // 各々のコンポーネントの取得
        animator = GetComponent<Animator>();
        controller = GetComponent<CharacterController>();

    }

    // Update is called once per frame
    void Update()
    {

        // stateの値をもとに状態遷移
        switch (state)
        {

            case State.Move:

                // 水平移動
                moveDirection.x = Input.GetAxis("Horizontal " + PlayerNumber);

                // 方向転換
                LookAtEnemy();

                // アニメーターに横移動速度を渡す
                animator.SetFloat("Speed", Mathf.Abs(moveDirection.x));

                // 状態遷移
                if (Input.GetButtonDown("Fire1 " + PlayerNumber))
                {

                    moveDirection.x = 0;

                    animator.SetTrigger("Jab");

                    state = State.DuringAttack;

                }
                else if (Input.GetButtonDown("Fire2 " + PlayerNumber))
                {

                    moveDirection.x = 0;

                    animator.SetTrigger("Kick");

                    state = State.DuringAttack;

                }

                else if (Input.GetButtonDown("Jump " + PlayerNumber))
                {

                    moveDirection.y = JumpPower;

                    animator.SetBool("Jump", true);

                    state = State.Jump;

                }

                break;

            case State.Jump:

                // 状態遷移
                if (IsGrounded())
                {

                    animator.SetBool("Jump", false);

                    state = State.Move;

                }
                else
                {
                    moveDirection.y -= Gravity * Time.deltaTime;
                }

                break;

            case State.DuringAttack:

                // 何もしない

                break;

            case State.DamageDown:

                if (GetAnyButtonDown() == true)
                {

                    animator.SetTrigger("Headspring");

                }
                break;

            default:

                state = State.Move;

                break;

        }

        // 移動確定
        controller.Move(moveDirection * Speed * Time.deltaTime);

    }

    void OnTriggerEnter(Collider other)
    {

        // 自分の攻撃には反応しない
        if (other.tag == "Attack" && other.transform.root != transform)
        {

            isHited();

        }

    }

    // 接地しているかを返すメソッド
    private bool IsGrounded()
    {

        // キャラクターの中心からレイを下方向に飛ばし、地面に設置しているかどうかを調べる
        if (Physics.Raycast(new Ray(transform.position + Vector3.up, Vector3.down), 1f)) return true;

        return false;

    }

    // 敵の方向を向くメソッド
    private void LookAtEnemy()
    {

        int t;

        if (transform.position.x < Enemy.transform.position.x) t = 90;
        else t = -90;

        transform.rotation = Quaternion.Euler(Vector3.up * t);

    }

    // 何かボタンが押されていたらtrueを返すメソッド
    private bool GetAnyButtonDown()
    {

        if (Input.GetButtonDown("Fire1 " + PlayerNumber) || Input.GetButtonDown("Fire2 " + PlayerNumber) || Input.GetButtonDown("Fire3 " + PlayerNumber) || Input.GetButtonDown("Jump " + PlayerNumber))
            return true;

        return false;

    }

    // 攻撃を受けたときに呼ばれるメソッド
    public void isHited()
    {

        // ジャンプ中に攻撃を受けたとき
        if (state == State.Jump) animator.SetBool("Jump", false);

        animator.Play("DamageDown");

        moveDirection.x = 0;

        state = State.DamageDown;

    }

    // 攻撃判定を作る
    public void AttackStart(string PartName)
    {

        // 全探索
        Transform[] transformArray = transform.GetComponentsInChildren<Transform>();

        foreach (Transform child in transformArray)
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

    // Stateをアニメーションイベントから操作
    public void SetMoveState()
    {

        state = State.Move;

    }

}
