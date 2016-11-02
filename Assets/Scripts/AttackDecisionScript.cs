using UnityEngine;
using System.Collections;

public class AttackDecisionScript : MonoBehaviour
{

    // ダメージ
    public int Damage = 0;

    // 投げのダメージ
    public int NageDamage = 0;

    // 与える攻撃の効果、喰らいモーション
    public int AttackEffection = 0;

    // ヒットバックの大きさ
    public float HitBack = 0.0f;

    // 打ち上げる攻撃の威力
    public float HitBackY = 0.0f;

    // 中下段、nullなら上段
    public int AttackPoint = 0;

    public int PlayerNum = 0;

}
