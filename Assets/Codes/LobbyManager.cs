using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LobbyManager : MonoBehaviourPunCallbacks
{
    private string gameVersion = "0.1";
    public Text info;
    public Button joinbutton;
    public bool inroom=false;

    // Start is called before the first frame update
    void Start()
    {
        PhotonNetwork.GameVersion=gameVersion;
        PhotonNetwork.ConnectUsingSettings();
        PhotonNetwork.AutomaticallySyncScene = true;

        joinbutton.interactable=false;
        info.text="서버 접속중";
    }

    public override void OnConnectedToMaster(){
        joinbutton.interactable=true;
        info.text="서버 연결 완료";
    }

    public override void OnDisconnected(DisconnectCause cause){
        joinbutton.interactable=false;
        info.text="서버 연결 실패\n재접속 시도중";
        PhotonNetwork.ConnectUsingSettings();
    }

    public void Connect(){
        joinbutton.interactable=false;
        if(PhotonNetwork.IsConnected){
            info.text="대기실 접속중";
            PhotonNetwork.JoinRandomRoom();
        }
        else{
            info.text="서버 연결 실패\n재접속 시도중";
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    public override void OnJoinRandomFailed(short returnCode, string message){
        info.text="대기실 생성중";
        PhotonNetwork.CreateRoom(null, new RoomOptions{MaxPlayers=2});
    }

    public override void OnJoinedRoom(){
        info.text="대기실 접속 완료";
        if(PhotonNetwork.PlayerList.Length==1){
            //Global.location = 0;
            inroom=true;
        }
        else if(PhotonNetwork.PlayerList.Length==2){
            //Global.location = 1;
        }
    }

    public void Update(){
        if(inroom){
            info.text=PhotonNetwork.PlayerList.Length+"명";
            if(PhotonNetwork.PlayerList.Length==2){
                inroom=false;
                PhotonNetwork.LoadLevel("Test1");
            }
        }
    }

    public void Single(){
        PhotonNetwork.Disconnect();
        SceneManager.LoadScene("SinglePlay");
    }
}
