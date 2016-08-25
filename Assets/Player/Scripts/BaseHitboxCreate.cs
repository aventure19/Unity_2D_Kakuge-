using UnityEngine;
using System.Collections;

public class BaseHitboxCreate : MonoBehaviour {

    public GameObject atari;

    // Use this for initialization
    void Start () {

        // プレハブを取得
        GameObject prefab = atari;
        Vector2 pos = new Vector2(atari.transform.position.x, atari.transform.position.y);
        /*******************/

        // プレハブからインスタンスを生成
        // GameObject obj = (GameObject)Instantiate(prefab, GameObject.Find("Element").transform.position, Quaternion.identity);
        GameObject obj1 = (GameObject)Instantiate(prefab, GameObject.Find("Element (1)").transform.position, Quaternion.identity);
        /******************/

        // 作成したオブジェクトを子として登録
        // obj.transform.parent = GameObject.Find("Element").transform;
        obj1.transform.parent = GameObject.Find("Element (1)").transform;
        /******************/

    }

   


}
