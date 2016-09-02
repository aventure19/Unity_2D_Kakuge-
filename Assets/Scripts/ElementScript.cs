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

    // HPbar
    public Image HPGauge;

    // 各攻撃判定
    public Transform[] AttackDecisions = new Transform[10];  // 0:頭 1:体 2:右ひじ 3:右手 4:左ひじ 5:左手 6:右ひざ 7:右足 8:左ひざ 9:左足

    // 自分と敵の位置
    public Vector3 vPlayer;
    public Transform Enemy;
    public GameObject EnemyObj;
    private ElementScript Ees;

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

    //時間制御のデータ
    private float Now = 0.0f;
    private float TimeElapsed = 0.0f;
    private bool NowTrigger = true;

    //攻撃の持つデータ
    [SerializeField]
    private string AttackType = null;   //攻撃の詳細な名前 「Jab」「Kick」等
    [SerializeField]
    private string AttackEffection = null;  //攻撃が相手に当たったときの効果 「L_Bend(小仰け反り)」、「GroundFall(こける)」、「TurnUp(上を向いて仰け反る)」等
    [SerializeField]
    private int AttackDamage = 0; //攻撃の持つダメージ

    // Use this for initialization
    void Start()
    {
        //デフォルトのHPを格納
        DefaultHP = HP;

        vPlayer = this.transform.position;

        // 各々のコンポーネントの取得
        animator = GetComponent<Animator>();
        controller = GetComponent<CharacterController>();

        Ees = Enemy.GetComponent<ElementScript>();

    }

    // Update is called once per frame
    void Update()
    {


        // HPの変更処理(yキーでテスト可能)
        if (Input.GetKeyDown("y"))
        {
            Debug.Log("y pressed.");
            HP -= 20;
            Debug.Log("HP is Changed");

            HPGuageConfiguration(true);
        }

        if (HP < 0) HP = 0;

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

        AttackTypeConfig();

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

        HPGuageConfiguration(true);

        // ジャンプ中に攻撃を受けたとき
        if (state == State.Jump) animator.SetBool("Jump", false);

        animator.Play("DamageDown");

        moveDirection.x = 0;

        // 攻撃判定をすべて消す
        AttackAllEnd();

        state = State.DamageDown;

    }

    // 攻撃判定を作る
    public void AttackStart(int AttackDecisionNumber)
    {

        AttackDecisions[AttackDecisionNumber].GetComponent<Collider>().enabled = true;

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

    // ゲージの長さを調節する(yキーでテスト可能)
    public void HPGuageConfiguration(bool HPGTrigger)
    {

        HP -= Ees.AttackDamage;

        if (HPGTrigger == true)
        {
            HPGauge.transform.localScale = new Vector3(((float)HP / (float)DefaultHP), 1, 1); // locScaleでgameObjのx軸スケールを変更
        }

        if (HPGauge.transform.localScale.x < 0)
        {
            HPGauge.transform.localScale = new Vector3(0, 1, 1);
        }

        Debug.Log("Scale is Changed.");

        HPGTrigger = false;
    }

    /****通り抜けないようにするメソッド(未完成)
    public void PositionConfiguration()
    {
        if ((Vector2.Distance(this.gameObject.transform.position, EnemyObj.transform.position) < 1.2f))
        {
            if (this.gameObject.transform.position.x < EnemyObj.transform.position.x)
            {
                vPlayer.x -= 0.1f;
            }
            else
            {
                vPlayer.x += 0.1f;
            }
        }
    }
    ******************************************/

    //DamageDownから時間経過で強制的に立たせるメソッド(DamageDownにアタッチ済)
    public void ForciblyStanding()
    {

        animator.SetTrigger("Headspring");
    }

    // 攻撃判定の有無からダメージ等の諸変数を決定する
    public void AttackTypeConfig()
    {

        SphereCollider sc = GetComponent<SphereCollider>();
        BoxCollider bc = GetComponent<BoxCollider>();
        CapsuleCollider cc = GetComponent<CapsuleCollider>();
        Collider c = GetComponent<Collider>();

        // 0:頭 1:体 2:右ひじ 3:右手 4:左ひじ 5:左手 6:右ひざ 7:右足 8:左ひざ 9:左足


        if (AttackDecisions[0]) //頭部パーツによる攻撃
        {

        }

        if (AttackDecisions[1]) //胴体パーツによる攻撃
        {

        }

        if (AttackDecisions[2]) //右ひじパーツによる攻撃
        {

        }

        if (AttackDecisions[3]) //右手パーツによる攻撃
        {

        }

        if (AttackDecisions[4]) //左ひじパーツによる攻撃
        {

        }

        if (AttackDecisions[5].GetComponent<Collider>().enabled == true) //左手パーツによる攻撃
        {
            AttackEffection = "L_Bend";
            AttackDamage = 30;
        }

        if (AttackDecisions[6]) //右ひざパーツによる攻撃
        {

        }

        if (AttackDecisions[7].GetComponent<Collider>().enabled == true) //右足パーツによる攻撃
        {
            AttackEffection = "H_Bend";
            AttackDamage = 60;
        }

        if (AttackDecisions[8]) //左ひざパーツによる攻撃
        {

        }

        if (AttackDecisions[9]) //左足パーツによる攻撃
        {

        }
    }
}
