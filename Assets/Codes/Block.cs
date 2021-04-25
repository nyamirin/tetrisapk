using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Block : MonoBehaviour
{
    public Sprite[] images;

    [PunRPC]
    public void Coloring(int c)
    {
        GetComponent<SpriteRenderer>().sprite = images[c-1];
    }
}
