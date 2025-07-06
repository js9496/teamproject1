using RetroSokoban;
using UnityEngine;

public class StartVRGame : MonoBehaviour
{
    private GameManager gameManager;

    private void Awake()
    {
        gameManager = FindFirstObjectByType<GameManager>();
    }

    private void Start()
    {
        //StartRetrokoban();
        gameManager.Process();
    }

    // 레트로코반 시작!!(VR 프로그램 시작)
    private void StartRetrokoban()
    {
        bool running = true;

        while (running)
        {
            running = gameManager.Process(); //프로세스가 end가 되면 false반환해서 와일문 종료
        }

        // 레트로코반 종료
        ExitGame();
    }

    // 프로그램 종료
    private void ExitGame()
    {
        print("프로그램 종료");
        Application.Quit(); //프로그램 종료

        //에디터용 코드
#if UNITY_EDITOR
        print("프로그램 종료");
        UnityEditor.EditorApplication.isPlaying = false;    //에디터의 플레이버튼 끔
#endif
    }
}


//using RetroSokoban;
//using UnityEngine;

//public class StartVRGame : MonoBehaviour
//{
//    private GameManager gameManager;

//    private void Awake()
//    {
//        gameManager = FindFirstObjectByType<GameManager>();
//    }

//    private void Start()
//    {
//        StartRetrokoban();
//    }

//    // 레트로코반 시작!!(VR 프로그램 시작)
//    private void StartRetrokoban()
//    {
//        bool running = true;

//        while (running)
//        {
//            running = gameManager.Process(); //프로세스가 end가 되면 false반환해서 와일문 종료
//        }
//    }
//}
