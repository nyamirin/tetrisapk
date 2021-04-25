using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SingleManager : MonoBehaviour
{
    //Input
    Dictionary<KeyCode, Action> keyDictionary;
    Dictionary<KeyCode, int> keyindex;
    float[] downtime;

    //////////////////////////////////////////////////////////////
    //game
    
    public GameObject endui;

    int drop_x;
    int drop_y;
    int rot_state;
    const int delay = 1000;
    int bycnt = 1;
    bool falling = false;
    int cur_mino = 0;
    int hold_mino = 0;
    bool holding = false;
    float last_fall_time;
    int gauge = 0;
    int attack = 0;
    bool spin = false;
    int combo = 0;
    int btb = 0;
    public bool play = false;

    // Start is called before the first frame update
    void Start()
    {
        play=true;
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
        last_fall_time=Time.time;
        Bag_init();
        Make_preset();
        Clear_real();
        Make_drop();
        //nextdiv();
        //load_img();
        //document.onkeydown = kpress;
        //Display_real();
        //Display_falling();
    }

    // Update is called once per frame
    public float inv=0;
    void Update()
    {
        if(play==false)return;
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
        if(last_fall_time+1f<=Time.time){
            last_fall_time=Time.time;
            Move_down();
        }
    }
    
    public void Game_Over(){
        play=false;
        endui.SetActive(true);
    }

    void Make_drop()
    {
        if (real_board[4, 19])
        {
            Game_Over();
            Debug.Log("Game Over");
        }
        else
        {
            falling = true;
            cur_mino = Next_mino();
            drop_x = 4;
            drop_y = 19;
            rot_state = 0;
            Show_fallingmino();
            Show_next();
            //timer = setTimeout(Move_down, delay);
        }
    }

    public void Rotate_cw() {
        if(!Input.GetKey(KeyCode.Z)){
            if (Can_cw()) {
                spin = true;
                rot_state = (rot_state + 1) % 4;
                Show_fallingmino();
            } else { //kick_wall
                int cur;
                if (0==cur_mino) cur = 1;
                else cur = 0;
                int dx = drop_x;
                int dy = drop_y;
                for (int i = 0; i < 4; i++) {
                    drop_x += xpre_cw[cur,rot_state,i];
                    drop_y += ypre_cw[cur,rot_state,i];
                    if (Can_cw()) {
                        spin = true;
                        rot_state = (rot_state + 1) % 4;
                        Show_fallingmino();
                        return;
                    } else {
                        drop_x = dx;
                        drop_y = dy;
                    }
                }
            }
        }
    }

    public void Rotate_ccw() {
        if(!Input.GetKey(KeyCode.X)){
            if (Can_ccw()) {
                spin = true;
                if (rot_state == 0) rot_state = 3;
                else rot_state--;
                Show_fallingmino();
            } else { //kick_wall\
                int cur;
                if (0==cur_mino) cur = 1;
                else cur = 0;
                int dx = drop_x;
                int dy = drop_y;
                for (int i = 0; i < 4; i++) {
                    drop_x += xpre_ccw[cur,rot_state,i];
                    drop_y += ypre_ccw[cur,rot_state,i];
                    if (Can_ccw()) {
                        spin = true;
                        if (rot_state == 0) rot_state = 3;
                        else rot_state--;
                        Show_fallingmino();
                        return;
                    } else {
                        drop_x = dx;
                        drop_y = dy;
                    }
                }
            }
        }
    }

    public void Move_left() {
        if(!Input.GetKey(KeyCode.RightArrow)){
            if (Can_left()) {
                spin=false;
                drop_x--;
                Show_fallingmino();
            }
            inv=2;
        }
    }

    public void Move_right() {
        if(!Input.GetKey(KeyCode.LeftArrow)){
            if (Can_right()) {
                spin=false;
                drop_x++;
                Show_fallingmino();
            }
            inv=2;
        }
    }

    public void Move_down() {
        if (Can_down()) {
            spin=false;
            drop_y--;
            Show_fallingmino();
        } else {
            falling = false;
            Stick();
        }
        last_fall_time=Time.time;
    }

    public void Softdrop() {
        if (Can_down()) {
            //clearTimeout(timer);
            Move_down();
        }
        inv=2;
    }

    public void Harddrop() {
        //clearTimeout(timer);
        while (Can_down()) {
            spin=false;
            drop_y--;
            Show_fallingmino();
        }
        Stick();
    }

    bool Can_down() {
        int cnt = 0;
        if (!falling) return false;
        for (int i = 0; i < 4; i++) {
            int cx = drop_x + mino_locx[cur_mino,rot_state,i];
            int cy = drop_y + mino_locy[cur_mino,rot_state,i] - 1;
            if (-1 < cy){
                if(real_board[cx,cy]) cnt++;
            }
            else return false;
        }
        if (cnt==0) return true;
        else return false;
    }
    bool Can_down(int rx,int ry) {
        int cnt = 0;
        for (int i = 0; i < 4; i++) {
            int cx = rx + mino_locx[cur_mino,rot_state,i];
            int cy = ry + mino_locy[cur_mino,rot_state,i] - 1;
            if (-1 < cy){
                if(real_board[cx,cy]) cnt++;
            }
            else return false;
        }
        if (0==cnt) return true;
        else return false;
    }

    bool Can_ccw() {
        int cnt = 0;
        int crt;
        if (rot_state == 0) crt = 3;
        else crt = rot_state - 1;
        for (int i = 0; i < 4; i++) {
            int cx = drop_x + mino_locx[cur_mino,crt,i];
            int cy = drop_y + mino_locy[cur_mino,crt,i];
            if (-1 < cx && cx < 10 && -1 < cy){
                if(real_board[cx,cy]) cnt++;
            }
            else return false;
        }
        if (0==cnt) return true;
        else return false;
    }

    bool Can_cw() {
        int cnt = 0;
        for (int i = 0; i < 4; i++) {
            int cx = drop_x + mino_locx[cur_mino,(rot_state + 1) % 4,i];
            int cy = drop_y + mino_locy[cur_mino,(rot_state + 1) % 4,i];
            if (-1 < cx && cx < 10 && -1 < cy){
                if(real_board[cx,cy]) cnt++;
            }
            else return false;
        }
        if (0==cnt) return true;
        else return false;
    }

    bool Can_left() {
        int cnt = 0;
        for (int i = 0; i < 4; i++) {
            int cx = drop_x + mino_locx[cur_mino,rot_state,i] - 1;
            int cy = drop_y + mino_locy[cur_mino,rot_state,i];
            if (-1 < cx){
                if(real_board[cx,cy]) cnt++;
            }
            else return false;
        }
        if (0==cnt) return true;
        else return false;
    }

    bool Can_right() {
        int cnt = 0;
        for (int i = 0; i < 4; i++) {
            int cx = drop_x + mino_locx[cur_mino,rot_state,i] + 1;
            int cy = drop_y + mino_locy[cur_mino,rot_state,i];
            if (cx < 10){
                if(real_board[cx,cy]) cnt++;
            }
            else return false;
        }
        if (0==cnt) return true;
        else return false;
    }

    public void Hold() {
        if (!holding) {
            holding = true;
            //clearTimeout(timer);
            int tmp = hold_mino;
            if (0==hold_mino) {
                hold_mino = cur_mino;
                Clear_falling();
                Make_drop();
            } else {
                hold_mino = cur_mino;
                cur_mino = tmp;
                Clear_falling();
                falling = true;
                drop_x = 4;
                drop_y = 19;
                rot_state = 0;
                Show_fallingmino();
                //timer = setTimeout(Move_down, delay);
            }
        }
        Show_hold();
    }

    ////////////////////////////////////////////////////////
    //board
    
    public bool[,] real_board = new bool[10,50];
    public bool[,] last_board = new bool[10,50];
    public int[,] falling_board = new int[10,21];
    public int[,] transparent_board = new int[10,21];
    public GameObject real_block;
    //public GameObject falling_block;
    public GameObject transparent_block;
    public GameObject[,] real_prefabs = new GameObject[10,50];
    public GameObject[] prefabs;
    public GameObject[] tprefabs;
    public GameObject[] previews;

    public void Clear_real(){
        for(int x=0;x<10;x++){
            for(int y=0;y<50;y++){
                real_board[x,y]=false;
                real_prefabs[x,y]=null;
            }
        }
    }

    
    public void Clear_last(){
        for(int x=0;x<10;x++){
            for(int y=0;y<50;y++){
                last_board[x,y]=false;
            }
        }
    }
    public void Make_last(){
        for(int x=0;x<10;x++){
            for(int y=0;y<50;y++){
                last_board[x,y]=real_board[x,y];
            }
        }
    }

    public void Clear_falling(){
        for(int x=0;x<10;x++){
            for(int y=0;y<21;y++){
                falling_board[x,y]=0;
            }
        }
    }

    public void Clear_transparent(){
        for(int x=0;x<10;x++){
            for(int y=0;y<21;y++){
                transparent_board[x,y]=0;
            }
        }
    }

    public GameObject Make_RealBlock(int x,int y,int color){
        GameObject instance = Instantiate(real_block, new Vector2(x+0.5f,y), Quaternion.Euler(0,0,0));
        instance.GetComponent<Block>().Coloring(color);
        return instance;
    }
    /*
    public GameObject Make_FallingBlock(int x,int y,int color){
        GameObject instance = Instantiate(falling_block, new Vector2(x,y), Quaternion.Euler(0,0,0));
        instance.GetComponent<Block>().Coloring(color);
        return instance;
    }
    */
    public GameObject Make_TransparentBlock(int x,int y,int color){
        GameObject instance = Instantiate(transparent_block, new Vector2(x+0.5f,y), Quaternion.Euler(0,0,0));
        instance.GetComponent<Block>().Coloring(color);
        return instance;
    }

    /*
    public void Display_real(){
    }
    */

    public void Display_falling(){
        int cnt=0;
        if(prefabs.Length!=0){
            for(int i=0;i<4;i++){
                Destroy(prefabs[i]);
                prefabs[i]=null;
            }
        }
        for(int x=0;x<10;x++){
            for(int y=0;y<21;y++){
                if(falling_board[x,y]!=0){
                    prefabs[cnt++]=Make_RealBlock(x,y,falling_board[x,y]);
                }
            }
        }
    }

    public void Display_transparent(){
        int cnt=0;
        
        if(tprefabs.Length!=0){
            for(int i=0;i<4;i++){
                Destroy(tprefabs[i]);
                tprefabs[i]=null;
            }
        }

        for(int x=0;x<10;x++){
            for(int y=0;y<21;y++){
                if(transparent_board[x,y]!=0){
                    tprefabs[cnt++]=Make_TransparentBlock(x,y,transparent_board[x,y]);
                }
            }
        }
    }

    public void Clear_line(){
        Make_last();
        bool did=false;
        for(int y=49;y>=0;y--){
            int cnt=0;
            for(int x=0;x<10;x++){
                if(real_board[x,y]) cnt++;
            }
            if(cnt==10){
                did=true;
                for(int x=0;x<10;x++){
                    Destroy(real_prefabs[x,y]);
                    real_prefabs[x,y]=null;
                    real_board[x,y]=false;
                }
                for(int l=y;l<49;l++){
                    for(int k=0;k<10;k++){
                        real_board[k,l]=real_board[k,l+1];
                        if(real_prefabs[k,l+1]!=null){
                            real_prefabs[k,l+1].transform.Translate(Vector2.down);
                            real_prefabs[k,l]=real_prefabs[k,l+1];
                            real_prefabs[k,l+1]=null;
                        }
                    }
                }
                
                if(attack!=0)Debug.Log(attack);
            }
            else {
                cnt=0;
            }
        }
        if(did){
            combo++;
            attack=Evaluate();
            Clear_last();
        }
        else combo=0;
    }

    ///////////////////////////////////////////
    //mino
    
    public int[] bag = { 1, 2, 3, 4, 5, 6, 7, 1, 2, 3, 4, 5, 6, 7 };
    int bagcnt = 0;
    int[,,] mino_locx = new int[8,4,4];
    int[,,] mino_locy = new int[8,4,4];
    int[,,] xpre_cw = new int[2,4,4];
    int[,,] ypre_cw = new int[2,4,4];
    int[,,] xpre_ccw = new int[2,4,4];
    int[,,] ypre_ccw = new int[2,4,4];
    int rx;
    int ry;

    public void Bag_init()
    {
        int tmp;
        int random;
        for (int i = 7; i > 1; i--)
        {
            random = UnityEngine.Random.Range(0, 7);
            tmp = bag[random];
            bag[random] = bag[i - 1];
            bag[i - 1] = tmp;
        }
        for (int i = 14; i > 7; i--)
        {
            random = UnityEngine.Random.Range(0, 7) + 7;
            tmp = bag[random];
            bag[random] = bag[i - 1];
            bag[i - 1] = tmp;
        }
    }

    public void Shffle_bag(){
        for(int i=0;i<7;i++){
            bag[i]=bag[i+7];
        }
        int tmp;
        int random;
        for(int i=14;i>7;i--){
            random=UnityEngine.Random.Range(0,7)+7;
            tmp=bag[random];
            bag[random]=bag[i-1];
            bag[i-1]=tmp;
        }
    }

    public int Next_mino(){
        int rt = bag[bagcnt++];
        if(bagcnt!=7)return rt;
        else{
            bagcnt=0;
            Shffle_bag();
            return rt;
        }
    }

    public void Show_fallingmino(){
        Clear_falling();
        for(int i=0;i<4;i++){
            int x=drop_x + mino_locx[cur_mino,rot_state,i];
            int y=drop_y + mino_locy[cur_mino,rot_state,i];
            falling_board[x,y] = cur_mino;
        }
        Make_tmino();
        Display_falling();
    }
    
    void Show_next(){
        for(int i=0;i<5;i++){
            previews[i].GetComponent<Preview>().Coloring(bag[bagcnt+i]);
        }
    }
    
    void Show_hold(){
        previews[5].GetComponent<Preview>().Coloring(hold_mino);
    }
    
    void Make_tmino() {
        rx = drop_x;
        ry = drop_y;
        while (Can_down(rx, ry)) {
            ry--;
        }
        Clear_transparent();
        for (int i = 0; i < 4; i++) {
            int x=rx + mino_locx[cur_mino,rot_state,i];
            int y=ry + mino_locy[cur_mino,rot_state,i];
            transparent_board[x,y] = cur_mino;
        }
        Display_transparent();
    }

    void Show_tmino() {
        Clear_transparent();
        for (int i = 0; i < 4; i++) {
            int x=rx + mino_locx[cur_mino,rot_state,i];
            int y=ry + mino_locy[cur_mino,rot_state,i];
            transparent_board[x,y] = cur_mino;
        }
        Display_transparent();
    }

    void Stick() {
        for (int x = 0; x < 10; x++) {
            for (int y = 0; y < 21; y++) {
                if(falling_board[x,y]!=0){
                    real_prefabs[x,y]=Make_RealBlock(x,y,falling_board[x,y]);
                    real_board[x,y]=true;
                    falling_board[x,y]=0;
                    //last_board[x,y]=real_board[x,y];
                }
            }
        }
        for(int i=0;i<4;i++){
            Destroy(prefabs[i]);
            prefabs[i]=null;
        }
        holding = false;
        Clear_falling();
        //Display_real();
        Show_next();
        Clear_line();
        bool atk=false;
        while(attack>0&&gauge>0){
            atk=true;
            attack--;
            gauge--;
        }
        if(!atk&&gauge!=0){
            Make_garbage(gauge);
            gauge=0;
        }
        attack=0;
        //공격

        Make_drop();
    }

    void Make_garbage(int n){
        int pos = UnityEngine.Random.Range(0, 10);
        for(int i=0;i<n;i++){
            if(UnityEngine.Random.Range(0,10)>6) pos = UnityEngine.Random.Range(0, 10);
            Garbage(pos);
        }
    }

    void Garbage(int pos){
        for(int y=48;y>=0;y--){
            for(int x=0;x<10;x++){
                real_board[x,y+1]=real_board[x,y];
                real_board[x,y]=false;
                if(real_prefabs[x,y]!=null){
                    real_prefabs[x,y].transform.Translate(Vector2.up);
                    real_prefabs[x,y+1]=real_prefabs[x,y];
                    real_prefabs[x,y]=null;
                }
            }
        }
        for(int x=0;x<10;x++){
            if(x!=pos){
                real_prefabs[x,0]=Make_RealBlock(x,0,8);
                real_board[x,0]=true;
            }
        }
    }

    int Evaluate(){
        int atk=0;
        int cnt=0;
        for(int y=0;y<50;y++){
            int tcnt=0;
            for(int x=0;x<10;x++){
                if(last_board[x,y])tcnt++;
            }
            if(tcnt==10) cnt++;
        }
        int wcnt=0;
        if(drop_x==9) wcnt++;
        else if(last_board[drop_x+1,drop_y+1])wcnt++;
        if(drop_x==9 || drop_y==0) wcnt++;
        else if(last_board[drop_x+1,drop_y-1])wcnt++;
        if(drop_x==0) wcnt++;
        else if(last_board[drop_x-1,drop_y+1])wcnt++;
        if(drop_x==0 || drop_y==0) wcnt++;
        else if(last_board[drop_x-1,drop_y-1])wcnt++;
        if(cur_mino==6&&spin&&wcnt>2){
            btb++;
            Debug.Log("tspin");
            if(cnt==1){atk++;Debug.Log("tspin single");}
            else if(cnt==2){atk+=4;Debug.Log("tspin double");}
            else if(cnt==3){atk+=6;Debug.Log("tspin triple");}
        }
        else{
            Debug.Log("normal");
            btb=0;
            if(cnt==2){atk++;Debug.Log("double");}
            else if(cnt==3){atk+=2;Debug.Log("triple");}
            else if(cnt==4){atk+=4;Debug.Log("tetris");}
        }
        if(combo==3){atk+=1;}
        else if(combo==4){atk+=1;}
        else if(combo==5){atk+=1;}
        else if(combo==6){atk+=1;}
        else if(combo==7){atk+=1;}
        else if(combo==8){atk+=2;}
        else if(combo==9){atk+=2;}
        else if(combo==10){atk+=2;}
        else if(combo==11){atk+=2;}
        else if(combo==12){atk+=3;}
        else if(combo==13){atk+=3;}
        else if(combo==14){atk+=3;}
        else if(combo==15){atk+=3;}
        else if(combo==16){atk+=4;}
        else if(combo==17){atk+=4;}
        else if(combo==18){atk+=4;}
        else if(combo==19){atk+=4;}
        else if(combo>=20){atk+=5;}
        if(combo>1)Debug.Log(combo-1+"Combo");
        if(btb>1){atk++;Debug.Log("btb");}


        if(Is_perfect()){
            atk+=10;
            Debug.Log("perfect clear");
        }

        return atk;
    }

    bool Is_perfect(){
        for(int x=0;x<10;x++){
            if(real_board[x,0])return false;
        }
        return true;
    }

    void Make_preset() {
        //[i미노,0~270도,4개블록]
        mino_locx[1,0,0] = -1;
        mino_locy[1,0,0] = 0;
        mino_locx[1,0,1] = 0;
        mino_locy[1,0,1] = 0;
        mino_locx[1,0,2] = 1;
        mino_locy[1,0,2] = 0;
        mino_locx[1,0,3] = 2;
        mino_locy[1,0,3] = 0;
        mino_locx[1,1,0] = 1;
        mino_locy[1,1,0] = 1;
        mino_locx[1,1,1] = 1;
        mino_locy[1,1,1] = 0;
        mino_locx[1,1,2] = 1;
        mino_locy[1,1,2] = -1;
        mino_locx[1,1,3] = 1;
        mino_locy[1,1,3] = -2;
        mino_locx[1,2,0] = -1;
        mino_locy[1,2,0] = -1;
        mino_locx[1,2,1] = 0;
        mino_locy[1,2,1] = -1;
        mino_locx[1,2,2] = 1;
        mino_locy[1,2,2] = -1;
        mino_locx[1,2,3] = 2;
        mino_locy[1,2,3] = -1;
        mino_locx[1,3,0] = 0;
        mino_locy[1,3,0] = 1;
        mino_locx[1,3,1] = 0;
        mino_locy[1,3,1] = 0;
        mino_locx[1,3,2] = 0;
        mino_locy[1,3,2] = -1;
        mino_locx[1,3,3] = 0;
        mino_locy[1,3,3] = -2;
        //[j미노,0~270도,4개블록]
        mino_locx[2,0,0] = 0;
        mino_locy[2,0,0] = 0;
        mino_locx[2,0,1] = -1;
        mino_locy[2,0,1] = 0;
        mino_locx[2,0,2] = -1;
        mino_locy[2,0,2] = 1;
        mino_locx[2,0,3] = 1;
        mino_locy[2,0,3] = 0;
        mino_locx[2,1,0] = 0;
        mino_locy[2,1,0] = 0;
        mino_locx[2,1,1] = 0;
        mino_locy[2,1,1] = -1;
        mino_locx[2,1,2] = 0;
        mino_locy[2,1,2] = 1;
        mino_locx[2,1,3] = 1;
        mino_locy[2,1,3] = 1;
        mino_locx[2,2,0] = 0;
        mino_locy[2,2,0] = 0;
        mino_locx[2,2,1] = -1;
        mino_locy[2,2,1] = 0;
        mino_locx[2,2,2] = 1;
        mino_locy[2,2,2] = 0;
        mino_locx[2,2,3] = 1;
        mino_locy[2,2,3] = -1;
        mino_locx[2,3,0] = 0;
        mino_locy[2,3,0] = 0;
        mino_locx[2,3,1] = 0;
        mino_locy[2,3,1] = -1;
        mino_locx[2,3,2] = 0;
        mino_locy[2,3,2] = 1;
        mino_locx[2,3,3] = -1;
        mino_locy[2,3,3] = -1;
        //[l미노,0~270도,4개블록]
        mino_locx[3,0,0] = 0;
        mino_locy[3,0,0] = 0;
        mino_locx[3,0,1] = -1;
        mino_locy[3,0,1] = 0;
        mino_locx[3,0,2] = 1;
        mino_locy[3,0,2] = 0;
        mino_locx[3,0,3] = 1;
        mino_locy[3,0,3] = 1;
        mino_locx[3,1,0] = 0;
        mino_locy[3,1,0] = 0;
        mino_locx[3,1,1] = 0;
        mino_locy[3,1,1] = -1;
        mino_locx[3,1,2] = 0;
        mino_locy[3,1,2] = 1;
        mino_locx[3,1,3] = 1;
        mino_locy[3,1,3] = -1;
        mino_locx[3,2,0] = 0;
        mino_locy[3,2,0] = 0;
        mino_locx[3,2,1] = -1;
        mino_locy[3,2,1] = 0;
        mino_locx[3,2,2] = 1;
        mino_locy[3,2,2] = 0;
        mino_locx[3,2,3] = -1;
        mino_locy[3,2,3] = -1;
        mino_locx[3,3,0] = 0;
        mino_locy[3,3,0] = 0;
        mino_locx[3,3,1] = 0;
        mino_locy[3,3,1] = 1;
        mino_locx[3,3,2] = 0;
        mino_locy[3,3,2] = -1;
        mino_locx[3,3,3] = -1;
        mino_locy[3,3,3] = 1;
        //[o미노,0~270도,4개블록]
        mino_locx[4,0,0] = 0;
        mino_locy[4,0,0] = 0;
        mino_locx[4,0,1] = 1;
        mino_locy[4,0,1] = 0;
        mino_locx[4,0,2] = 0;
        mino_locy[4,0,2] = 1;
        mino_locx[4,0,3] = 1;
        mino_locy[4,0,3] = 1;
        mino_locx[4,1,0] = 0;
        mino_locy[4,1,0] = 0;
        mino_locx[4,1,1] = 1;
        mino_locy[4,1,1] = 0;
        mino_locx[4,1,2] = 0;
        mino_locy[4,1,2] = 1;
        mino_locx[4,1,3] = 1;
        mino_locy[4,1,3] = 1;
        mino_locx[4,2,0] = 0;
        mino_locy[4,2,0] = 0;
        mino_locx[4,2,1] = 1;
        mino_locy[4,2,1] = 0;
        mino_locx[4,2,2] = 0;
        mino_locy[4,2,2] = 1;
        mino_locx[4,2,3] = 1;
        mino_locy[4,2,3] = 1;
        mino_locx[4,3,0] = 0;
        mino_locy[4,3,0] = 0;
        mino_locx[4,3,1] = 1;
        mino_locy[4,3,1] = 0;
        mino_locx[4,3,2] = 0;
        mino_locy[4,3,2] = 1;
        mino_locx[4,3,3] = 1;
        mino_locy[4,3,3] = 1;
        //[s미노,0~270도,4개블록]
        mino_locx[5,0,0] = 0;
        mino_locy[5,0,0] = 0;
        mino_locx[5,0,1] = 0;
        mino_locy[5,0,1] = 1;
        mino_locx[5,0,2] = -1;
        mino_locy[5,0,2] = 0;
        mino_locx[5,0,3] = 1;
        mino_locy[5,0,3] = 1;
        mino_locx[5,1,0] = 0;
        mino_locy[5,1,0] = 0;
        mino_locx[5,1,1] = 0;
        mino_locy[5,1,1] = 1;
        mino_locx[5,1,2] = 1;
        mino_locy[5,1,2] = 0;
        mino_locx[5,1,3] = 1;
        mino_locy[5,1,3] = -1;
        mino_locx[5,2,0] = 0;
        mino_locy[5,2,0] = 0;
        mino_locx[5,2,1] = 0;
        mino_locy[5,2,1] = -1;
        mino_locx[5,2,2] = 1;
        mino_locy[5,2,2] = 0;
        mino_locx[5,2,3] = -1;
        mino_locy[5,2,3] = -1;
        mino_locx[5,3,0] = 0;
        mino_locy[5,3,0] = 0;
        mino_locx[5,3,1] = 0;
        mino_locy[5,3,1] = -1;
        mino_locx[5,3,2] = -1;
        mino_locy[5,3,2] = 0;
        mino_locx[5,3,3] = -1;
        mino_locy[5,3,3] = 1;
        //[t미노,0~270도,4개블록]
        mino_locx[6,0,0] = 0;
        mino_locy[6,0,0] = 0;
        mino_locx[6,0,1] = -1;
        mino_locy[6,0,1] = 0;
        mino_locx[6,0,2] = 0;
        mino_locy[6,0,2] = 1;
        mino_locx[6,0,3] = 1;
        mino_locy[6,0,3] = 0;
        mino_locx[6,1,0] = 0;
        mino_locy[6,1,0] = 0;
        mino_locx[6,1,1] = 0;
        mino_locy[6,1,1] = -1;
        mino_locx[6,1,2] = 0;
        mino_locy[6,1,2] = 1;
        mino_locx[6,1,3] = 1;
        mino_locy[6,1,3] = 0;
        mino_locx[6,2,0] = 0;
        mino_locy[6,2,0] = 0;
        mino_locx[6,2,1] = -1;
        mino_locy[6,2,1] = 0;
        mino_locx[6,2,2] = 0;
        mino_locy[6,2,2] = -1;
        mino_locx[6,2,3] = 1;
        mino_locy[6,2,3] = 0;
        mino_locx[6,3,0] = 0;
        mino_locy[6,3,0] = 0;
        mino_locx[6,3,1] = -1;
        mino_locy[6,3,1] = 0;
        mino_locx[6,3,2] = 0;
        mino_locy[6,3,2] = 1;
        mino_locx[6,3,3] = 0;
        mino_locy[6,3,3] = -1;
        //[z미노,0~270도,4개블록]
        mino_locx[7,0,0] = 0;
        mino_locy[7,0,0] = 0;
        mino_locx[7,0,1] = 0;
        mino_locy[7,0,1] = 1;
        mino_locx[7,0,2] = 1;
        mino_locy[7,0,2] = 0;
        mino_locx[7,0,3] = -1;
        mino_locy[7,0,3] = 1;
        mino_locx[7,1,0] = 0;
        mino_locy[7,1,0] = 0;
        mino_locx[7,1,1] = 0;
        mino_locy[7,1,1] = -1;
        mino_locx[7,1,2] = 1;
        mino_locy[7,1,2] = 0;
        mino_locx[7,1,3] = 1;
        mino_locy[7,1,3] = 1;
        mino_locx[7,2,0] = 0;
        mino_locy[7,2,0] = 0;
        mino_locx[7,2,1] = 0;
        mino_locy[7,2,1] = -1;
        mino_locx[7,2,2] = -1;
        mino_locy[7,2,2] = 0;
        mino_locx[7,2,3] = 1;
        mino_locy[7,2,3] = -1;
        mino_locx[7,3,0] = 0;
        mino_locy[7,3,0] = 0;
        mino_locx[7,3,1] = 0;
        mino_locy[7,3,1] = 1;
        mino_locx[7,3,2] = -1;
        mino_locy[7,3,2] = 0;
        mino_locx[7,3,3] = -1;
        mino_locy[7,3,3] = -1;

        //[j~t미노,0~170도,프리셋0~4]
        xpre_cw[0,0,0] = -1;
        ypre_cw[0,0,0] = 0;
        xpre_cw[0,0,1] = -1;
        ypre_cw[0,0,1] = 1;
        xpre_cw[0,0,2] = 0;
        ypre_cw[0,0,2] = -2;
        xpre_cw[0,0,3] = -1;
        ypre_cw[0,0,3] = -2;
        xpre_ccw[0,1,0] = 1;
        ypre_ccw[0,1,0] = 0;
        xpre_ccw[0,1,1] = 1;
        ypre_ccw[0,1,1] = -1;
        xpre_ccw[0,1,2] = 0;
        ypre_ccw[0,1,2] = 2;
        xpre_ccw[0,1,3] = 1;
        ypre_ccw[0,1,3] = 2;
        xpre_cw[0,1,0] = 1;
        ypre_cw[0,1,0] = 0;
        xpre_cw[0,1,1] = 1;
        ypre_cw[0,1,1] = -1;
        xpre_cw[0,1,2] = 0;
        ypre_cw[0,1,2] = 2;
        xpre_cw[0,1,3] = 1;
        ypre_cw[0,1,3] = 2;
        xpre_ccw[0,2,0] = -1;
        ypre_ccw[0,2,0] = 0;
        xpre_ccw[0,2,1] = -1;
        ypre_ccw[0,2,1] = 1;
        xpre_ccw[0,2,2] = 0;
        ypre_ccw[0,2,2] = -2;
        xpre_ccw[0,2,3] = -1;
        ypre_ccw[0,2,3] = -2;
        xpre_cw[0,2,0] = 1;
        ypre_cw[0,2,0] = 0;
        xpre_cw[0,2,1] = 1;
        ypre_cw[0,2,1] = 1;
        xpre_cw[0,2,2] = 0;
        ypre_cw[0,2,2] = -2;
        xpre_cw[0,2,3] = 1;
        ypre_cw[0,2,3] = -2;
        xpre_ccw[0,3,0] = -1;
        ypre_ccw[0,3,0] = 0;
        xpre_ccw[0,3,1] = -1;
        ypre_ccw[0,3,1] = -1;
        xpre_ccw[0,3,2] = 0;
        ypre_ccw[0,3,2] = 2;
        xpre_ccw[0,3,3] = -1;
        ypre_ccw[0,3,3] = 2;
        xpre_cw[0,3,0] = -1;
        ypre_cw[0,3,0] = 0;
        xpre_cw[0,3,1] = -1;
        ypre_cw[0,3,1] = -1;
        xpre_cw[0,3,2] = 0;
        ypre_cw[0,3,2] = 2;
        xpre_cw[0,3,3] = -1;
        ypre_cw[0,3,3] = 2;
        xpre_ccw[0,0,0] = 1;
        ypre_ccw[0,0,0] = 0;
        xpre_ccw[0,0,1] = 1;
        ypre_ccw[0,0,1] = 1;
        xpre_ccw[0,0,2] = 0;
        ypre_ccw[0,0,2] = -2;
        xpre_ccw[0,0,3] = 1;
        ypre_ccw[0,0,3] = -2;
        //[i미노,0~170도,프리셋0~4]
        xpre_cw[1,0,0] = -2;
        ypre_cw[1,0,0] = 0;
        xpre_cw[1,0,1] = 1;
        ypre_cw[1,0,1] = 0;
        xpre_cw[1,0,2] = -2;
        ypre_cw[1,0,2] = -1;
        xpre_cw[1,0,3] = 1;
        ypre_cw[1,0,3] = 2;
        xpre_ccw[1,1,0] = 2;
        ypre_ccw[1,1,0] = 0;
        xpre_ccw[1,1,1] = -1;
        ypre_ccw[1,1,1] = 0;
        xpre_ccw[1,1,2] = 2;
        ypre_ccw[1,1,2] = 1;
        xpre_ccw[1,1,3] = -1;
        ypre_ccw[1,1,3] = -2;
        xpre_cw[1,1,0] = -1;
        ypre_cw[1,1,0] = 0;
        xpre_cw[1,1,1] = 2;
        ypre_cw[1,1,1] = 0;
        xpre_cw[1,1,2] = -1;
        ypre_cw[1,1,2] = 2;
        xpre_cw[1,1,3] = 2;
        ypre_cw[1,1,3] = -1;
        xpre_ccw[1,2,0] = 1;
        ypre_ccw[1,2,0] = 0;
        xpre_ccw[1,2,1] = -2;
        ypre_ccw[1,2,1] = 0;
        xpre_ccw[1,2,2] = 1;
        ypre_ccw[1,2,2] = -2;
        xpre_ccw[1,2,3] = -2;
        ypre_ccw[1,2,3] = 1;
        xpre_cw[1,2,0] = 2;
        ypre_cw[1,2,0] = 0;
        xpre_cw[1,2,1] = -1;
        ypre_cw[1,2,1] = 0;
        xpre_cw[1,2,2] = 2;
        ypre_cw[1,2,2] = 1;
        xpre_cw[1,2,3] = -1;
        ypre_cw[1,2,3] = -2;
        xpre_ccw[1,3,0] = -2;
        ypre_ccw[1,3,0] = 0;
        xpre_ccw[1,3,1] = 1;
        ypre_ccw[1,3,1] = 0;
        xpre_ccw[1,3,2] = -2;
        ypre_ccw[1,3,2] = -1;
        xpre_ccw[1,3,3] = 1;
        ypre_ccw[1,3,3] = 2;
        xpre_cw[1,3,0] = 1;
        ypre_cw[1,3,0] = 0;
        xpre_cw[1,3,1] = -2;
        ypre_cw[1,3,1] = 0;
        xpre_cw[1,3,2] = 1;
        ypre_cw[1,3,2] = -2;
        xpre_cw[1,3,3] = -2;
        ypre_cw[1,3,3] = 1;
        xpre_ccw[1,0,0] = -1;
        ypre_ccw[1,0,0] = 0;
        xpre_ccw[1,0,1] = 2;
        ypre_ccw[1,0,1] = 0;
        xpre_ccw[1,0,2] = -1;
        ypre_ccw[1,0,2] = 2;
        xpre_ccw[1,0,3] = -2;
        ypre_ccw[1,0,3] = -1;
    }

    void Test_Function(){
        gauge++;
    }

}
