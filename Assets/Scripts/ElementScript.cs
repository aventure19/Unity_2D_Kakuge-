using UnityEngine;
using UnityEngine.UI;
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

    // HP等諸変数
    public int HP = 1000;
    public int DefaultHP = 1000;
    public Image HPGauge;

    // 各攻撃判定
    public Transform[] AttackDecisions = new Transform[10];  // 0:頭 1:体 2:右ひじ 3:右手 4:左ひじ 5:左手 6:右ひざ 7:右足 8:左ひざ 9:左足

    // 自分と敵の位置
    public Transform Enemy;

    // 飛び道具(仮)
    public GameObject Tobidougu;

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

                else if (Input.GetButtonDown("Fire3 " + PlayerNumber))
                {

                    // 飛び道具(仮)
                    GameObject target = Instantiate(Tobidougu, transform.position + Vector3.up, Quaternion.identity) as GameObject;
                    target.GetComponent<AttackDecisionScript>().PlayerNum = PlayerNumber;
                    target.GetComponent<MissileScript>().moveDirection = transform.forward;

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

                // 地面
                if (IsGrounded())
                {
                    if (GetAnyButtonDown() == true)
                    {

                        animator.SetTrigger("Headspring");

                    }
                }

                // 空中
                else
                {

                    // 重力
                    moveDirection.y -= Gravity * Time.deltaTime;

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

        if (other.tag == "Attack")
        {

            isHited(other);

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
    public void isHited(Collider other)
    {

        // Damage
        AttackDecisionScript t = other.GetComponent<AttackDecisionScript>();

        if (t.PlayerNum == PlayerNumber) return;

        HP -= t.Damage;

        // HPが0未満なら0に戻す
        if (HP < 0) HP = 0;

        // locScaleでgameObjのx軸スケールを変更
        HPGauge.transform.localScale = new Vector3(((float)HP / (float)DefaultHP), 1, 1);

        // ジャンプ中に攻撃を受けたとき
        if (state == State.Jump) animator.SetBool("Jump", false);

        animator.Play("DamageDown");

        moveDirection.x = 0;

        // 攻撃判定をすべて消す
        AttackAllEnd();

        state = State.DamageDown;

    }

    // 攻撃判定を作る
    public void AttackStart(string AttackDecisionNumberAndDamage)
    {

        // スペースで区切って第一引数にAttackDecisionsを第二引数にDamageを渡す
        string[] s = AttackDecisionNumberAndDamage.Split(' ');

        int AttackDecisionNumber = int.Parse(s[0]);

        Transform target = AttackDecisions[AttackDecisionNumber];

        // AttackDecisionScriptが無ければAttach
        if (target.GetComponent<AttackDecisionScript>() == null) target.gameObject.AddComponent<AttackDecisionScript>();

        AttackDecisionScript t = target.GetComponent<AttackDecisionScript>();

        t.PlayerNum = PlayerNumber;

        try
        {
            int Damage = int.Parse(s[1]);
            t.Damage = Damage;
        }
        catch (System.IndexOutOfRangeException e)
        {
            Debug.Log("第二引数がありません");
            Debug.Log(e);
        }

        // 攻撃判定をenable
        target.GetComponent<Collider>().enabled = true;

    }

    // 攻撃判定を消す
    public void AttackEnd(int AttackDecisionNumber)
    {

        AttackDecisions[AttackDecisionNumber].GetComponent<Collider>().enabled = false;

    }

    // 攻撃判定をすべて消す
    private void AttackAllEnd()
    {

        foreach (Transform a in AttackDecisions)
        {

            Collider c = a.GetComponent<Collider>();

            if (c) c.enabled = false;
            else Debug.Log(a.name + " に Collider がアタッチされていないよ！");

        }

    }

    // StateをMoveにする(アニメーションイベントから行う)
    public void SetMoveState()
    {

        state = State.Move;

    }

    public void PositionConfiguration()
    {



    }

    //DamageDownから時間経過で強制的に立たせるメソッド(DamageDownにアタッチ済)
    public void ForciblyStanding()
    {

        animator.SetTrigger("Headspring");
    }

}
