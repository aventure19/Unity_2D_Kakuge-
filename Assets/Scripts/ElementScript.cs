using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(Rigidbody))]
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
    public int HitBackVector;

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
        Crouch,
        Jump,
        DuringAttack,
        DuringJumpAttack,
        Damage,
        DamageDown
    }

    [SerializeField]
    private State state;

    // 各コンポーネント
    private Animator animator;
    private Rigidbody rb;

    [SerializeField]
    private Vector3 moveDirection;

    // Use this for initialization
    void Start()
    {

        // 各々のコンポーネントの取得
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();

    }

    void FixedUpdate()
    {

        transform.position += new Vector3(moveDirection.x, 0, 0) * Speed * Time.deltaTime;

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

                if (Input.GetAxis("Vertical " + PlayerNumber) < 0)
                {
                    animator.SetBool("Crouch", true);

                    state = State.Crouch;
                }

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

                    rb.AddForce(Vector3.up * JumpPower);

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

                else if (Input.GetAxis("Horizontal " + PlayerNumber) > 0 && Input.GetKeyDown("q"))
                {
                    moveDirection.x = 0;

                    animator.Play("Hook");

                    state = State.DuringAttack;

                }

                break;

            case State.Crouch:

                moveDirection.x = 0;

                if (Input.GetAxis("Vertical " + PlayerNumber) >= 0)
                {
                    animator.SetBool("Crouch", false);

                    state = State.Move;
                }

                else if (Input.GetKeyDown("q"))
                {
                    animator.Play("Daiashi");

                    state = State.DuringAttack;
                }

                break;

            case State.Jump:

                // 状態遷移
                if (rb.velocity.y < 0 && IsGrounded())
                {

                    animator.SetBool("Jump", false);

                    state = State.Move;

                }

                if (Input.GetKeyDown("q"))
                {
                    animator.Play("JumpHeavyKick");

                    state = State.DuringJumpAttack;
                }

                break;


            case State.DuringAttack:

                // 何もしない

                break;

            case State.DuringJumpAttack:

                animator.SetBool("Jump", false);

                // 状態遷移
                if (IsGrounded())
                {

                    AttackAllEnd();

                    animator.Play("Move");

                    state = State.Move;

                }

                break;

            case State.Damage:

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

                break;


            default:

                state = State.Move;

                break;

        }

    }

    void OnTriggerStay(Collider other)
    {

        if (other.tag == "Attack")
        {

            if (other.GetComponent<AttackDecisionScript>().PlayerNum != PlayerNumber)
            {

                isHitted(other);

                other.enabled = false;

            }

        }

    }

    // 接地しているかを返すメソッド
    private bool IsGrounded()
    {

        // キャラクターの中心からレイを下方向に飛ばし、地面に設置しているかどうかを調べる
        return Physics.Raycast(new Ray(transform.position + Vector3.up, Vector3.down), 1f, 1 << LayerMask.NameToLayer("LandScape"));

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
    public void isHitted(Collider other)
    {

        //ヒットバックのベクトルの係数を自キャラの向きから初期化        
        if (transform.localEulerAngles.y == 90)
            HitBackVector = 1;
        else
        {
            HitBackVector = -1;
        }

        // Damage
        AttackDecisionScript t = other.GetComponent<AttackDecisionScript>();

        HP -= t.Damage;

        // HPが0未満なら0に戻す
        if (HP < 0) HP = 0;

        // locScaleでHPゲージのx軸スケールを変更
        HPGauge.transform.localScale = new Vector3(((float)HP / (float)DefaultHP), 1, 1);

        // ジャンプ中に攻撃を受けたとき
        if (state == State.Jump) animator.SetBool("Jump", false);

        if (t.AttackEffection == 1)
        {
            animator.Play("L_Kurai");
        }

        if (t.AttackEffection == 2)
        {
            animator.Play("DamageDown");

            state = State.DamageDown;
        }
        else
        {
            Debug.Log("設定されていない第三引数が使われています : Enemy,AttackEffection : " + t.AttackEffection);
        }

        //ヒットバックの生成
        // this.transform.position = new Vector2(this.transform.position.x - t.HitBack * HitBackVector, this.transform.position.y);
        rb.AddForce(new Vector3(-HitBackVector, 1, 0) * 150f);

        moveDirection.x = 0;

        // 攻撃判定をすべて消す
        AttackAllEnd();



    }

    // 攻撃判定を作る
    public void AttackStart(string AttackDecisionNumberAndDamage)
    {

        // スペースで区切って第一引数にAttackDecisionsを第二引数にDamageを渡す

        /********第三引数に相手に与える仰け反り効果を記述
                    1:小仰け反り(L_Kurai), 2:DamageDown, ～      ********/

        //第四引数にヒットバックをfloat型で記述

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

        try
        {
            int AttackEffection = int.Parse(s[2]);
            t.AttackEffection = AttackEffection;
        }
        catch (System.IndexOutOfRangeException e)
        {
            Debug.Log("第三引数がありません");
            Debug.Log(e);
        }

        try
        {
            float HitBack = float.Parse(s[3]);
            t.HitBack = HitBack;
        }
        catch (System.IndexOutOfRangeException e)
        {
            Debug.Log("第四引数がありません");
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

    // StateをCrouchにする(しゃがみ攻撃の後など)
    public void SetCrouchState()
    {
        state = State.Crouch;
    }

    public void SetState(string goState)
    {
        switch (goState)
        {

            case "Damage":

                state = State.Damage;

                break;

            case "Jump":

                state = State.Jump;

                break;
        }

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
