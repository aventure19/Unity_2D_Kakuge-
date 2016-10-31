using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Pose : MonoBehaviour
{

    // ポーズ画面のUI
    public Canvas ca;
    public Image KO, Prepare, Go;
    public Light light;
    public Text tm;

    // ゲームの状態
    public enum State
    {
        // gameState
        Ready,
        Playable,
        Pose,
        GameOver,

        // Winner
        StillGameNow,
        OneP,
        TwoP,
        DrawGame
    }

    [SerializeField]
    private State gameState;
    [SerializeField]
    private State Winner;

    // キャラクタのスクリプト
    public ElementScript[] es = new ElementScript[2];
    public GameObject[] chara = new GameObject[2];

    public GameObject WinChara;

    private SceneLoad sl;

    void Awake()
    {
        Application.targetFrameRate = 60;
    }

    void Start()
    {

        gameState = State.Ready;
        Winner = State.StillGameNow;

        // Menuを無効化
        ca.enabled = false;

        // ElementScriptを取得
        es[0] = chara[0].GetComponent<ElementScript>();
        es[1] = chara[1].GetComponent<ElementScript>();

        // ゲームがはじまるまで無効化
        es[0].enabled = false;
        es[1].enabled = false;
        KO.enabled = false;
        Prepare.enabled = false;
        Go.enabled = false;
        tm.text = null;

        sl = GetComponent<SceneLoad>();
    }

    // Update is called once per frame
    void Update()
    {

        switch (gameState)
        {
            case State.Ready:

                TimefromStart();

                if (TimefromStart())
                {
                    gameState = State.Playable;
                }

                break;

            case State.Playable:

                es[0].enabled = true;
                es[1].enabled = true;

                GameStateConfiguration();

                break;

            case State.Pose:

                GameStateConfiguration();

                break;

            case State.GameOver:

                // ゲームオーバー
                if (light.intensity > 0.0f)
                {
                    light.intensity -= 0.08f;
                }
                else
                {

                    switch (Winner)
                    {
                        case State.OneP:

                            tm.text = "1P\nWin!";
                            WinChara = chara[0];

                            break;

                        case State.TwoP:

                            tm.text = "2P\nWin!";
                            WinChara = chara[1];

                            break;

                        case State.DrawGame:

                            tm.text = "DrawGame...";
                            WinChara = null;

                            break;
                    }

                    if(Input.anyKey || Input.GetButtonDown("Cir 1") || Input.GetButtonDown("Cir 2"))
                    {
                        sl.Load("ElementTitle");
                    }

                }

                break;

        }



    }

    private bool TimefromStart()
    {

        if(Time.timeSinceLevelLoad < 3.0f)
        {
            Prepare.enabled = true;
        }
        else
        {
            Prepare.enabled = false;
        }

        if (Time.timeSinceLevelLoad >= 3.0f && Time.timeSinceLevelLoad < 5.0f)
        {
            Go.enabled = true;
        }
        else
        {
            Go.enabled = false;
        }

        if (Time.timeSinceLevelLoad >= 5.0f)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private void GameStateConfiguration()
    {

        if (gameState == State.Playable)
        {
            // escapeが押されたらPoseへ
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                ca.enabled = true;

                es[0].enabled = false;

                es[1].enabled = false;

                Time.timeScale = 0;

                gameState = State.Pose;
            }


            // どちらかのHPが0になったらゲームセット
            if(es[0].HP <= 0 || es[1].HP <= 0)
            {
                Invoke("GameTerminator", 5.0f);

                KO.enabled = true;

                Invoke("KODisEnabled", 5.0f);
                
            }


        }

        // Pose画面でReturnが押されたらポーズ画面を閉じてゲームへもどる
        else if (gameState == State.Pose)
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                ca.enabled = false;

                es[0].enabled = true;

                es[1].enabled = true;

                Time.timeScale = 1.0f;

                gameState = State.Playable;
            }
        }
    }

    private void GameTerminator()
    {
        if (es[0].HP <= 0 && es[1].HP <= 0)
        {
            Winner = State.DrawGame;
        }
        else if (es[1].HP <= 0)
        {
            Winner = State.OneP;
        }
        else if (es[0].HP <= 0)
        {
            Winner = State.TwoP;
        }

        es[0].enabled = false;

        es[1].enabled = false;

        gameState = State.GameOver;
    }

    private void KODisEnabled()
    {
        KO.enabled = false;
    }

}
