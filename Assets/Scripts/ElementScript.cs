﻿using UnityEngine;
using UnityEngine.UI;
using System;
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
    public Transform[] Reach = new Transform[10]; // 0:右上腕 1:右肘下 2:左上腕 3:左肘下 4:右腿 5:右膝下 6:左腿 7:左膝下

    // 自分と敵の位置
    public Transform Player;
    public Transform Enemy;

    // 飛び道具(仮)
    public GameObject Tobidougu;

    // 攻撃ヒット時に再生するエフェクト
    public ParticleSystem ps;

    // 状態
    private enum State
    {
        // state
        Move,
        Crouch,
        Jump,
        DuringAttack,
        DuringJumpAttack,
        Damage,
        DamageDown,
        KnockOut,
        Nagerare,

        // subState
        Muteki,
        SGuard,
        CGuard,
        Defo
    }

    [SerializeField]
    private State state;
    [SerializeField]
    private State subState;

    // 各コンポーネント
    public Animator animator;
    public Rigidbody rb;
    private AudioSource audio;
    // 敵のRigidBody, ElementScript
    private Rigidbody enRb;
    private ElementScript enEs;

    // audioで用いるAudioClip配列
    // 0: 打撃ヒット 1: ガード
    public AudioClip[] ac = new AudioClip[5];

    // デバッグ用の各種データ
    public TextMesh dTm;
    public int dI = 0;

    // 移動量
    [SerializeField]
    private Vector3 moveDirection;

    // Use this for initialization
    void Start()
    {

        // 各々のコンポーネントの取得
        if (GetComponent<Animator>())
            animator = GetComponent<Animator>();

        if (GetComponent<Rigidbody>())
            rb = GetComponent<Rigidbody>();

        if (GetComponent<AudioSource>())
            audio = GetComponent<AudioSource>();

        if (Enemy)
        {
            enRb = Enemy.gameObject.GetComponent<Rigidbody>();
            enEs = Enemy.gameObject.GetComponent<ElementScript>();
        }

    }

    void FixedUpdate()
    {

        // Guardボタンを押しているか
        if (Input.GetButton("Cir " + PlayerNumber))
        {

            animator.SetBool("isGuard", true);

        }

        else
        {

            animator.SetBool("isGuard", false); // animator.GetBool("isGuard");

        }


        if (state != State.DuringAttack)
        {
            dI = 0;

            ReachAllDefault();
        }




        // 壁にぶつかったらx方向の移動を0にする
        Vector3 dir = new Vector3(moveDirection.x, 0, 0);
        if (Physics.CapsuleCast(transform.position + Vector3.up * 0.5f, transform.position + Vector3.up * 1.5f, 0.5f, dir, dir.sqrMagnitude * Speed * Time.deltaTime, 1 << LayerMask.NameToLayer("LandScape")))
        {

            moveDirection.x = 0;
            Debug.Log("hello");

        }

        transform.position += new Vector3(moveDirection.x, 0, 0) * Speed * Time.deltaTime;

        PositionConfiguration();

    }

    // Update is called once per frame
    void Update()
    {

        if (HP <= 0)
        {
            state = State.KnockOut;
        }

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

                // MoveからSGuardへの遷移
                if (animator.GetBool("isGuard"))
                {

                    subState = State.SGuard;

                }

                // ガード状態でなければ
                if (subState != State.SGuard && subState != State.CGuard)
                {
                    if (Input.GetButtonDown("Squ " + PlayerNumber) && Input.GetAxis("Horizontal " + PlayerNumber) == 0 && Input.GetAxis("Vertical " + PlayerNumber) == 0)
                    {

                        moveDirection.x = 0;

                        animator.SetTrigger("Jab");

                        state = State.DuringAttack;

                    }
                    else if (Input.GetButtonDown("Squ " + PlayerNumber) && Input.GetAxis("Vertical " + PlayerNumber) > 0)
                    {

                        moveDirection.x = 0;

                        animator.SetTrigger("Kick");

                        state = State.DuringAttack;

                    }

                    else if (Input.GetButton("Crs " + PlayerNumber))
                    {

                        rb.velocity = new Vector3(rb.velocity.x, JumpPower);

                        animator.SetBool("Jump", true);

                        state = State.Jump;

                    }

                    else if (Input.GetButtonDown("Tri " + PlayerNumber) && Input.GetAxis("Horizontal " + PlayerNumber) == 0 && Input.GetAxis("Vertical " + PlayerNumber) == 0)
                    {

                        moveDirection.x = 0;

                        animator.Play("Hadou");

                        state = State.DuringAttack;

                    }

                    else if (Input.GetButtonDown("Tri " + PlayerNumber) && (Input.GetAxis("Vertical " + PlayerNumber) != 0))
                    {
                        moveDirection.x = 0;

                        animator.Play("Shoryu");

                        state = State.DuringAttack;
                    }

                    else if (Input.GetButtonDown("Tri " + PlayerNumber) && (Input.GetAxis("Horizontal " + PlayerNumber) != 0))
                    {
                        moveDirection.x = 0;

                        animator.Play("Tosshin");

                        state = State.DuringAttack;
                    }

                    else if (Input.GetAxis("Horizontal " + PlayerNumber) != 0 && (Input.GetButtonDown("Squ " + PlayerNumber)))
                    {
                        moveDirection.x = 0;

                        animator.Play("Hook");

                        state = State.DuringAttack;

                    }

                    else if (Input.GetButton("R2 " + PlayerNumber))
                    {
                        moveDirection.x = 0;

                        animator.Play("Nage");

                        state = State.DuringAttack;
                    }

                }
                break;

            case State.Crouch:

                moveDirection.x = 0;

                if (Input.GetAxis("Vertical " + PlayerNumber) >= 0)
                {
                    animator.SetBool("Crouch", false);

                    state = State.Move;
                }

                // CrouchからCGuardへの遷移
                if (animator.GetBool("isGuard"))
                {

                    subState = State.CGuard;

                }

                // ガード状態でなければ
                if (subState != State.SGuard && subState != State.CGuard)
                {

                    if (Input.GetButtonDown("Squ " + PlayerNumber) && Input.GetAxis("Vertical " + PlayerNumber) <= 0 && Input.GetAxis("Horizontal " + PlayerNumber) != 0)
                    {
                        animator.Play("Daiashi");

                        state = State.DuringAttack;
                    }

                    else if (Input.GetButtonDown("Tri " + PlayerNumber) && Input.GetAxis("Vertical " + PlayerNumber) < 0)
                    {
                        animator.Play("Chuudan");

                        state = State.DuringAttack;
                    }

                    else if (Input.GetButtonDown("Squ " + PlayerNumber) && Input.GetAxis("Horizontal " + PlayerNumber) == 0)
                    {
                        animator.Play("Uppercut");

                        state = State.DuringAttack;
                    }

                }

                break;

            case State.Jump:

                // 状態遷移
                if (rb.velocity.y < 0.1f && IsGrounded())
                {

                    animator.SetBool("Jump", false);

                    state = State.Move;

                }

                // 空中攻撃
                if (Input.GetButtonDown("Squ " + PlayerNumber))
                {
                    animator.Play("JumpHeavyKick");

                    state = State.DuringJumpAttack;
                }

                break;


            // 攻撃中は何もしない
            case State.DuringAttack:

                dI++;

                if (dI > 180)
                {

                    animator.SetBool("Crouch", false);

                    state = State.Move;
                }

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

                if (IsGrounded() && Input.GetAxis("Vertical " + PlayerNumber) < 0)
                {

                    animator.SetTrigger("Headspring");

                }

                break;

            case State.Nagerare:

                moveDirection.x = 0;

                this.transform.position = enEs.AttackDecisions[5].position;

                break;

            case State.KnockOut:

                moveDirection.x = 0;

                animator.Play("KnockOut");

                break;

            default:

                state = State.Move;

                break;

        }

        // subStateによる分岐処理
        switch (subState)
        {
            case State.SGuard:

                moveDirection.x = 0;

                break;

            case State.CGuard:

                moveDirection.x = 0;

                break;

            case State.Muteki:

                break;

            case State.Defo:

                break;

        }

        if (!animator.GetBool("isGuard"))
        {
            subState = State.Defo;
        }

    }

    // 攻撃判定が喰らい判定に重なったら
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

    // 画面端に到達しているか
    private bool IsWall()
    {
        // キャラクターの中心からレイを後ろ方向に飛ばし、壁に設置しているかどうかを調べる
        if (Physics.Raycast(new Ray(transform.position, -transform.forward), 1.0f, 1 << LayerMask.NameToLayer("LandScape")))
        {
            return true;
        }
        else { return false; }
    }

    // 敵の方向を向くメソッド
    private void LookAtEnemy()
    {

        int t;

        if (transform.position.x < Enemy.transform.position.x) t = 90;
        else t = -90;

        transform.rotation = Quaternion.Euler(Vector3.up * t);

    }

    // 特定のタイミングの下入力で受け身を取るメソッド
    public void GetUnder()
    {

        // 地面
        if (IsGrounded() && Input.GetAxis("Vertical " + PlayerNumber) < 0)
        {

            animator.SetTrigger("Headspring");

        }


    }

    // 攻撃を受けたときに呼ばれるメソッド
    public void isHitted(Collider other)
    {

        // Damage他
        AttackDecisionScript t = other.GetComponent<AttackDecisionScript>();

        if (state == State.DamageDown || subState == State.Muteki)
        {
            // 当たり判定がないとして何もしない
        }

        // 正しいガードをしているか
        else if ((t.AttackPoint == 1 && subState == State.CGuard) || (t.AttackPoint == 2 && subState == State.SGuard) || (t.AttackPoint == 0 && (subState == State.SGuard || subState == State.CGuard)))
        {

            HP -= t.Damage / 5;

            // HPが0未満なら0に戻す
            if (HP < 0) HP = 0;

            // locScaleでHPゲージのx軸スケールを変更
            HPGauge.transform.localScale = new Vector3(((float)HP / (float)DefaultHP), 1, 1);

            //ヒットバックの生成
            // this.transform.position = new Vector2(this.transform.position.x - t.HitBack * HitBackVector, this.transform.position.y);
            rb.AddForce(new Vector3(-transform.forward.x, 0, 0) * 100f);

            moveDirection.x = 0;

            // SEを再生
            AudioPlay(1);

            // ガードエフェクトを再生
            EffectPlay(other, 0, 90, 255, 255);

            // 攻撃判定をすべて消す
            AttackAllEnd();
        }

        else
        {

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
            if (IsWall())
            {
                // 飛び道具でなければ
                if (!other.GetComponent<MissileScript>())
                {
                    enRb.AddForce(new Vector3(transform.forward.x, 0, 0) * 150f);
                    Debug.Log("Enemy AddForce.");
                }
            }
            else
            {
                // this.transform.position = new Vector2(this.transform.position.x - t.HitBack * HitBackVector, this.transform.position.y);
                rb.AddForce(new Vector3(-transform.forward.x * t.HitBack, t.HitBackY, 0) * 150f);
            }

            moveDirection.x = 0;

            // SEを再生
            AudioPlay(0);

            // ヒットエフェクトを再生
            EffectPlay(other, 255, 64, 64, 255);

            // 攻撃判定をすべて消す
            AttackAllEnd();

            Debug.Log("hitted.");

        }

    }

    // 攻撃判定を作る
    public void AttackStart(string AttackDecisionNumberAndDamage)
    {

        // スペースで区切って第一引数にAttackDecisionsを第二引数にDamageを渡す

        /********第三引数に相手に与える仰け反り効果を記述
                    1:小仰け反り(L_Kurai), 2:DamageDown, ～      ********/

        // 第四引数にヒットバックをfloat型で記述
        // 第5引数で打ち上げ効果のある攻撃を表現
        // 第6引数で中下段(nullで上段) 1:下段 2:中断

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

        try
        {
            float HitBackY = float.Parse(s[4]);
            t.HitBackY = HitBackY;
        }
        catch (IndexOutOfRangeException e)
        {
            t.HitBackY = 0;
            Debug.Log("第 6 引数がありません");
            Debug.Log(e);
        }

        try
        {
            t.AttackPoint = int.Parse(s[5]);
        }
        catch (IndexOutOfRangeException e)
        {
            t.AttackPoint = 0;
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

        if (animator)
        {
            if (animator.GetBool("Crouch"))
            {
                animator.SetBool("Crouch", false);
            }

            state = State.Move;
        }


    }

    // StateをCrouchにする(しゃがみ攻撃の後など)
    public void SetCrouchState()
    {
        if (animator)
        {
            if (!animator.GetBool("Crouch"))
            {
                animator.SetBool("Crouch", false);
            }

            state = State.Crouch;
        }

    }

    // stateをアニメーションイベントの引数から指定
    public void SetState(string goState)
    {
        switch (goState)
        {

            case "DuringAttack":

                moveDirection.x = 0;

                state = State.DuringAttack;

                break;

            case "Damage":

                state = State.Damage;

                break;

            case "Jump":

                state = State.Jump;

                break;

            case "DamageDown":

                state = State.DamageDown;

                break;
        }

    }

    // subStateの変更
    public void SetSubState(string goState)
    {
        switch (goState)
        {
            case "Muteki":

                subState = State.Muteki;

                break;

            case "Defo":

                subState = State.Defo;

                break;
        }

    }

    // 相手と重ならないように位置を調整する
    public void PositionConfiguration()
    {

        if (Vector3.Distance(Enemy.position, this.transform.position) <= 1.0f)
        {

            if (Enemy.position.x >= this.transform.position.x)
            {
                this.transform.position = new Vector3(this.transform.position.x - 0.1f, this.transform.position.y, this.transform.position.z);
            }
            else
            {
                this.transform.position = new Vector3(this.transform.position.x + 0.1f, this.transform.position.y, this.transform.position.z);
            }
        }

        if (this.transform.position.z != 0.0f)
        {
            transform.position = new Vector3(transform.position.x, transform.position.y, 0);
        }
    }

    // 飛び道具を発射する
    public void Hadou()
    {
        // 飛び道具(仮)
        GameObject target = Instantiate(Tobidougu, transform.position + Vector3.up, Quaternion.identity) as GameObject;
        target.GetComponent<AttackDecisionScript>().PlayerNum = PlayerNumber;
        target.GetComponent<MissileScript>().moveDirection = transform.forward;
    }

    // DamageDownから時間経過で強制的に立たせるメソッド(DamageDownにアタッチ済)
    public void ForciblyStanding()
    {
        if (IsGrounded())
        {
            animator.SetTrigger("Headspring");
        }

    }

    // Audioを初期化して再生する
    public void AudioPlay(int cNum)
    {

        audio.clip = ac[cNum];

        audio.Play();

    }

    // 体のパーツの大きさを変更してリーチを変更
    public void ReachChange(string Num)
    {

        string[] s = Num.Split(' ');

        int PartsNum = int.Parse(s[0]);

        float x = float.Parse(s[1]);

        float y = float.Parse(s[2]);

        Reach[PartsNum].localScale = new Vector3(x, y, 1);

    }

    // 指定したパーツのサイズを初期化
    public void ReachDefault(int PartsNum)
    {

        Reach[PartsNum].localScale = new Vector3(1, 1, 1);

    }

    // 全てのパーツのサイズの初期化
    public void ReachAllDefault()
    {

        try
        {
            foreach (Transform a in Reach)
            {

                a.localScale = new Vector3(1, 1, 1);

            }
        }
        catch (NullReferenceException e)
        {
            // 何もしない
        }
    }

    // 視覚エフェクトを再生
    public void EffectPlay(Collider other, byte R, byte G, byte B, byte A)
    {
        ps.transform.gameObject.transform.position = new Vector3(other.transform.position.x, other.transform.position.y, other.transform.position.z);
        ps.startColor = new Color32(R, G, B, A);
        ps.Play();
    }

    // x軸の移動速度を変更、y軸方向にAddForceで移動量を変更
    public void moveDirConfiguration(string XY)
    {
        string[] s = XY.Split(' ');
        try
        {
            float x = float.Parse(s[0]);
            moveDirection.x = transform.forward.x;
        }
        catch (IndexOutOfRangeException e)
        {

        }

        try
        {
            float strength = float.Parse(s[1]);
            rb.AddForce(new Vector3(0, transform.up.y, 0) * strength);
        }
        catch (IndexOutOfRangeException)
        {

        }


    }

    // デバッグ用のレイを生成する(距離調整などに)
    public void DebugRay()
    {
        Debug.DrawRay(gameObject.transform.position, transform.forward, Color.red, Mathf.Infinity);
    }

    // 背負い投げ、投げのモーションから派生する
    public void Seoi(int Damage)
    {
        // AttackDecisionScriptが無ければAttach
        if (!GetComponent<AttackDecisionScript>()) gameObject.AddComponent<AttackDecisionScript>();

        // Damage
        AttackDecisionScript t = GetComponent<AttackDecisionScript>();

        t.NageDamage = Damage;

        if (Vector3.Distance(Player.position, Enemy.position) <= 1.2f && enEs.IsGrounded() && enEs.state != State.DamageDown && enEs.subState != State.Muteki)
        {
            moveDirection.x = 0;

            animator.StopPlayback();

            enEs.state = State.Nagerare;

            animator.Play("Seoi");

            enEs.animator.Play("Seoware");
        }

    }

    // 投げの効果を反映、投げられた側のアニメーションイベントから呼ぶ
    public void NageReflection()
    {
        AttackDecisionScript t = Enemy.gameObject.GetComponent<AttackDecisionScript>();

        HP -= t.NageDamage;

        // HPが0未満なら0に戻す
        if (HP < 0) HP = 0;

        // locScaleでHPゲージのx軸スケールを変更
        HPGauge.transform.localScale = new Vector3(((float)HP / (float)DefaultHP), 1, 1);

        // SEを再生
        AudioPlay(0);

    }

}
