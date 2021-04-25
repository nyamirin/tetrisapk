using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PhotonControll : MonoBehaviourPunCallbacks
{
    public RightSide left;
    public RightSide right;
    public PhotonView photonview;

    bool oppready = false;
    
    Dictionary<KeyCode, Action> keyDictionary;
    Dictionary<KeyCode, Action> remotDictionary;
    Dictionary<KeyCode, int> keyindex;
    float[] downtime;
    
    void Start()
    {
        if(PhotonNetwork.IsMasterClient){
            int seed=UnityEngine.Random.Range(0,100);
            photonview.RPC("Seed",RpcTarget.All,seed);
            photonview.RPC("Game_Start",RpcTarget.All);
        }

        keyDictionary = new Dictionary<KeyCode, Action>{
            {KeyCode.Space, Hold},
            {KeyCode.DownArrow, Softdrop},
            {KeyCode.LeftArrow, Move_left},
            {KeyCode.RightArrow, Move_right},
            {KeyCode.UpArrow, Harddrop},
            {KeyCode.Z, Rotate_ccw},
            {KeyCode.X, Rotate_cw},
            {KeyCode.T, Test_Function}
        };
        keyindex = new Dictionary<KeyCode, int>{
            {KeyCode.Space, 0},
            {KeyCode.DownArrow, 1},
            {KeyCode.LeftArrow, 2},
            {KeyCode.RightArrow, 3},
            {KeyCode.UpArrow, 4},
            {KeyCode.Z, 5},
            {KeyCode.X, 6},
            {KeyCode.T,7}
        };
        downtime = new float[8];
    }

    // Update is called once per frame
    void Update()
    {
        
        if(Input.anyKeyDown){
            foreach(var dic in keyDictionary){
                if(Input.GetKeyDown(dic.Key)){
                    dic.Value();
                }
            }
            foreach(var dic in keyindex){
                if(Input.GetKeyDown(dic.Key)){
                    downtime[dic.Value]=Time.time+0.4f;
                }
            }
        }
        if(Input.anyKey){
            foreach(var dic in keyDictionary){
                if(Input.GetKey(dic.Key)){
                    if(dic.Key!=KeyCode.UpArrow){
                        if(Time.time >= downtime[keyindex[dic.Key]]+0.05f){
                            dic.Value();
                            downtime[keyindex[dic.Key]]=Time.time;
                        }
                    }
                }
            }
        }
        
    }

    public void Retry_Multy(){
        if(oppready){
            photonview.RPC("Reload",RpcTarget.All);
        }
        else{
            photonview.RPC("Reload",RpcTarget.Others);
        }
    }
    
    public void MainMenu(){
        SceneManager.LoadScene("Lobby");
    }

    [PunRPC]
    public void Seed(int seed){
        left.Seed(seed);
        right.Seed(seed);
    }

    [PunRPC]
    public void Game_Start(){
        left.Game_Start();
        right.Game_Start();
    }

    [PunRPC]
    public void Reload(){
        if(PhotonNetwork.IsMasterClient) PhotonNetwork.LoadLevel("Test1");
    }

    [PunRPC]
    public void Hold(){
        left.Hold();
        photonview.RPC("rHold",RpcTarget.Others);
    }
    [PunRPC]
    public void Softdrop(){
        left.Softdrop();
        photonview.RPC("rSoftdrop",RpcTarget.Others);
    }
    [PunRPC]
    public void Move_left(){
        left.Move_left();
        photonview.RPC("rMove_left",RpcTarget.Others);
    }
    [PunRPC]
    public void Move_right(){
        left.Move_right();
        photonview.RPC("rMove_right",RpcTarget.Others);
    }
    [PunRPC]
    public void Harddrop(){
        left.Harddrop();
        photonview.RPC("rHarddrop",RpcTarget.Others);
    }
    [PunRPC]
    public void Rotate_ccw(){
        left.Rotate_ccw();
        photonview.RPC("rRotate_ccw",RpcTarget.Others);
    }
    [PunRPC]
    public void Rotate_cw(){
        left.Rotate_cw();
        photonview.RPC("rRotate_cw",RpcTarget.Others);
    }
    [PunRPC]
    public void Test_Function(){
        left.Test_Function();
        photonview.RPC("rTest_function",RpcTarget.Others);
    }

    [PunRPC]
    void rHold(){
        right.Hold();
    }
    [PunRPC]
    void rSoftdrop(){
        right.Softdrop();
    }
    [PunRPC]
    void rMove_left(){
        right.Move_left();
    }
    [PunRPC]
    void rMove_right(){
        right.Move_right();
    }
    [PunRPC]
    void rHarddrop(){
        right.Harddrop();
    }
    [PunRPC]
    void rRotate_ccw(){
        right.Rotate_ccw();
    }
    [PunRPC]
    void rRotate_cw(){
        right.Rotate_cw();
    }
    [PunRPC]
    void rTest_Function(){
        right.Test_Function();
    }
}
