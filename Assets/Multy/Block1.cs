using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Block1 : MonoBehaviour
{
    public int color=0;
    public Sprite image_n;
    public Sprite[] image_r;
    public Sprite[] image_t;

    [PunRPC]
    public void Color_n()
    {
        color=0;
        GetComponent<SpriteRenderer>().sprite = image_n;
    }
    
    [PunRPC]
    public void Color_r(int c)
    {
        GetComponent<SpriteRenderer>().sprite = image_r[c-1];
        color=c;
    }
    
    [PunRPC]
    public void Color_t(int c)
    {
        GetComponent<SpriteRenderer>().sprite = image_t[c-1];
    }
}