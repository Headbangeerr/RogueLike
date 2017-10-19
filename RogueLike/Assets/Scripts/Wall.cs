using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wall : MonoBehaviour {

    //障碍物的血量，被攻击2次后消失
    public int hp = 2;
    //被攻击后的图片
    public Sprite damageSprite;

	void Start () {
		
	}
	void Update () {
		
	}
    public void GetHit()
    {
        hp--;
        GetComponent<SpriteRenderer>().sprite = damageSprite;
        if (hp <= 0)
        {
            Destroy(this.gameObject);
        }
    }
}
