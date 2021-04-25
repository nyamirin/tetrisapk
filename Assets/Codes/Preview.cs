using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Preview : MonoBehaviour
{
    public Sprite[] images;

    public void OnEnable(){
        if(Global.location==1){
            transform.Translate(Vector2.right*30f);
        }
    }

    [PunRPC]
    public void Coloring(int c)
    {
        GetComponent<SpriteRenderer>().sprite = images[c-1];
    }
}
