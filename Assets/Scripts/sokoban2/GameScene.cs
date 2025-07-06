using UnityEngine;
using OHGAR;
using RetroSokoban;

public class GameScene : MonoBehaviour
{
    public bool joystickInput = false;
    private SokobanManager sokobanManager;
    public TitleCanvas titleCanvas;
    public Player player;

    /////// 조이스틱이 휙휙 움직여야 인식되는거 바꾸기
    // 입력 간격 시간
    private float inputTime = 0.5f;
    // 현재 입력된 상태
    private bool inputState = false;
    //이전 시간
    private float prevTime = 0;


    private void Awake()
    {        
        sokobanManager = FindFirstObjectByType<SokobanManager>();
        titleCanvas = FindFirstObjectByType<TitleCanvas>();
        player = FindFirstObjectByType<Player>();
    }

    private void Start()
    {
        // Output 스태틱이라 밑에서처럼 사용(위에서 따로 연결 안해도 됨)
        XRKJController.Output += SetJoyStickValue;  // event함수라 +=로 델리게이트는 = 로 사용가능
    }

    private void Update()
    {
        KeyboardInput();
        // 소코반 키보드로 입력 안되게 함
        //if (!joystickInput)
        //{
        //    KeyboardInput();
        //}
    }

    // 조이스틱 입력
    public void SetJoyStickValue(Vector3 direction)
    {
        // 타이틀 캔버스가 켜져 있다면 함수를 종료 (activeSelf : 활성화된 상태)
        if (titleCanvas != null && titleCanvas.gameObject.activeSelf) return;

        int horizontal = (int)direction.x;
        int vertical = (int)direction.z;

        // 스테이지 삭제 후 다음 꺼 부를때 플레이어 다시 연결해줘야 함
        if (player == null)
            player = FindFirstObjectByType<Player>();
        if (player != null)
            player.SetAnimation(horizontal, vertical);

        //한번 입력한 상태라면 다시 입력받을 시간이 되었을 때 동작되도록 처리
        if (inputState)
        {
            float elapsed = Time.time - prevTime;
            if (elapsed > inputTime)
            {
                inputState = false;
            }
            return;
        }

        // 이동처리
        // 오른쪽 이동
        if (vertical == 0 && horizontal == 1)
        {
            inputState = true;
            prevTime = Time.time;

            sokobanManager?.HandleInput(Direction.Right);
        }
        //왼쪽이동
        if (vertical == 0 && horizontal == -1)
        {
            inputState = true;
            prevTime = Time.time;

            sokobanManager?.HandleInput(Direction.Left);
        }
        //위쪽 이동
        if (vertical == 1 && horizontal == 0)
        {
            inputState = true;
            prevTime = Time.time;

            sokobanManager?.HandleInput(Direction.Up);
        }
        //아래쪽 이동
        if (vertical == -1 && horizontal == 0)
        {
            inputState = true;
            prevTime = Time.time;

            sokobanManager?.HandleInput(Direction.Down);
        }
        print(direction);
    }

    public void KeyboardInput()
    {
        // 타이틀 캔버스가 켜져 있다면 함수를 종료 (activeSelf : 활성화된 상태)
        if (titleCanvas != null && titleCanvas.gameObject.activeSelf) return;

        // GetAxis -1~1 범위, GetAxisRaw
        //int horizontal = (int)Input.GetAxis("Horizontal");
        //int vertical = (int)Input.GetAxis("Vertical");
        int horizontal = 0;
        int vertical = 0;

        if (Input.GetKeyDown(KeyCode.LeftArrow)) horizontal = -1;
        if (Input.GetKeyDown(KeyCode.RightArrow)) horizontal = 1;
        if (Input.GetKeyDown(KeyCode.UpArrow)) vertical = 1;
        if (Input.GetKeyDown(KeyCode.DownArrow)) vertical = -1;

        // 상하, 좌우를 가리키는 키가 동시에 입력되고 있다면 상하를 가리키는 키를 무시
        if (vertical != 0 && horizontal != 0) vertical = 0;

        // 해당되는 애니메이션 처리
        // 스테이지 삭제 후 다음 꺼 부를때 플레이어 다시 연결해줘야 함
        if (player == null)
            player = FindFirstObjectByType<Player>();
        if (player != null)
            player.SetAnimation(horizontal, vertical);

        // 이동처리
        // 오른쪽 이동
        if (vertical == 0 && horizontal == 1)
        {
            sokobanManager?.HandleInput(Direction.Right);
        }
        //왼쪽이동
        if (vertical == 0 && horizontal == -1)
        {
            sokobanManager?.HandleInput(Direction.Left);
        }
        //위쪽 이동
        if (vertical == 1 && horizontal == 0)
        {
            sokobanManager?.HandleInput(Direction.Up);
        }
        //아래쪽 이동
        if (vertical == -1 && horizontal == 0)
        {
            sokobanManager?.HandleInput(Direction.Down);
        }
    }
}




//using UnityEngine;
//using OHGAR;

//public class GameScene : MonoBehaviour
//{
//    public bool joystickInput = false;
//    private RetroSokoban.Sokoban sokoban;
//    public TitleCanvas titleCanvas;
//    public Player player;

//    /////// 조이스틱이 휙휙 움직여야 인식되는거 바꾸기
//    // 입력 간격 시간
//    private float inputTime = 0.5f;
//    // 현재 입력된 상태
//    private bool inputState = false;
//    //이전 시간
//    private float prevTime = 0;


//    private void Awake()
//    {
//        sokoban = FindFirstObjectByType<RetroSokoban.Sokoban>();
//        titleCanvas = FindFirstObjectByType<TitleCanvas>();
//        player = FindFirstObjectByType<Player>();
//    }

//    private void Start()
//    {
//        // Output 스태틱이라 밑에서처럼 사용(위에서 따로 연결 안해도 됨)
//       XRKJController.Output += SetJoyStickValue;  // event함수라 +=로 델리게이트는 = 로 사용가능
//    }

//    private void Update()
//    {
//        KeyboardInput();
//        // 소코반 키보드로 입력 안되게 함
//        //if (!joystickInput)
//        //{
//        //    KeyboardInput();
//        //}
//    }

//    // 조이스틱 입력
//    public void SetJoyStickValue(Vector3 direction)
//    {
//        // 타이틀 캔버스가 켜져 있다면 함수를 종료 (activeSelf : 활성화된 상태)
//        if (titleCanvas != null && titleCanvas.gameObject.activeSelf) return;

//        int horizontal = (int)direction.x;
//        int vertical = (int)direction.z;

//        // 스테이지 삭제 후 다음 꺼 부를때 플레이어 다시 연결해줘야 함
//        if (player == null)
//            player = FindFirstObjectByType<Player>();
//        if (player != null)
//            player.SetAnimation(horizontal, vertical);

//        //한번 입력한 상태라면 다시 입력받을 시간이 되었을 때 동작되도록 처리
//        if(inputState)
//        {
//            float elapsed = Time.time - prevTime;
//            if (elapsed > inputTime)
//            {
//                inputState = false;
//            }
//            return;
//        }

//        // 이동처리
//        // 오른쪽 이동
//        if (vertical == 0 && horizontal == 1)
//        {
//            inputState = true;
//            prevTime = Time.time;

//            sokoban?.HandleInput(Direction.Right);
//        }
//        //왼쪽이동
//        if (vertical == 0 && horizontal == -1)
//        {
//            inputState = true;
//            prevTime = Time.time;

//            sokoban?.HandleInput(Direction.Left);
//        }
//        //위쪽 이동
//        if (vertical == 1 && horizontal == 0)
//        {
//            inputState = true;
//            prevTime = Time.time;

//            sokoban?.HandleInput(Direction.Up);
//        }
//        //아래쪽 이동
//        if (vertical == -1 && horizontal == 0)
//        {
//            inputState = true;
//            prevTime = Time.time;

//            sokoban?.HandleInput(Direction.Down);
//        }
//        print(direction);
//    }

//    public void KeyboardInput()
//    {
//        // 타이틀 캔버스가 켜져 있다면 함수를 종료 (activeSelf : 활성화된 상태)
//        if (titleCanvas != null && titleCanvas.gameObject.activeSelf) return;

//        // GetAxis -1~1 범위, GetAxisRaw
//        //int horizontal = (int)Input.GetAxis("Horizontal");
//        //int vertical = (int)Input.GetAxis("Vertical");
//        int horizontal = 0;
//        int vertical = 0;

//        if (Input.GetKeyDown(KeyCode.LeftArrow)) horizontal = -1;
//        if (Input.GetKeyDown(KeyCode.RightArrow)) horizontal = 1;
//        if (Input.GetKeyDown(KeyCode.UpArrow)) vertical = 1;
//        if (Input.GetKeyDown(KeyCode.DownArrow)) vertical = -1;

//        // 상하, 좌우를 가리키는 키가 동시에 입력되고 있다면 상하를 가리키는 키를 무시
//        if (vertical != 0 && horizontal != 0) vertical = 0;

//        // 해당되는 애니메이션 처리
//        // 스테이지 삭제 후 다음 꺼 부를때 플레이어 다시 연결해줘야 함
//        if (player == null)
//            player = FindFirstObjectByType<Player>();
//        if (player != null)
//            player.SetAnimation(horizontal, vertical);

//        // 이동처리
//        // 오른쪽 이동
//        if (vertical == 0 && horizontal == 1)
//        {
//            sokoban?.HandleInput(Direction.Right);
//        }
//        //왼쪽이동
//        if (vertical == 0 && horizontal == -1)
//        {
//            sokoban?.HandleInput(Direction.Left);
//        }
//        //위쪽 이동
//        if (vertical == 1 && horizontal == 0)
//        {
//            sokoban?.HandleInput(Direction.Up);
//        }
//        //아래쪽 이동
//        if (vertical == -1 && horizontal == 0)
//        {
//            sokoban?.HandleInput(Direction.Down);
//        }
//    }
//}
