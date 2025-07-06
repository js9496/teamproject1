using System.Collections.Generic;
using UnityEngine;


namespace RetroSokoban
{
    public class GameManager : MonoBehaviour
    {
        // 게임모드 변수
        private GameMode _gameMode = GameMode.Start;

        // 현재 이동할 방향
        public Direction direction = Direction.None;

        private SokobanManager sokobanManager;


        private void Awake()
        {
            sokobanManager = FindFirstObjectByType<SokobanManager>();
        }

        // 레트로코반 전체 프로세스
        public bool Process()
        {
            //모드체크
            switch (_gameMode)
            {
                case GameMode.Start:
                    // 인트로 로고 애니메이션 3초후 비활성 IEnumerator LoadIntro()
                    // 아님 바로 로고 애니메이션만 호출하고 밑에서 수초뒤

                    //3초 뒤에 
                    //Invoke(nameof(ProcessStart), 3.0f);
                    // 시작화면UI
                    ProcessStart();

                  

                    break;
                case GameMode.Main:
                    ProcessMain();
                    /// todo...
                    // VR화면내 Main 로직

                    break;
                //case GameMode.Sokoban:
                //    // ProcessSokoban();
                //    break;
            }
            return _gameMode != GameMode.End; //종료되면 false를 반환하게
        }

        // 게임모드 설정
        public void SetgameMode(GameMode gameMode)
        {
            _gameMode = gameMode;
        }

        // 방향 설정
        public void SetDirection(Direction direction)
        {
            this.direction = direction;
        }

        //스타트모드 프로세스
        private void ProcessStart()
        {
            //// 로고애니메이션 비활성

            //// => UI에서 처리
            //// 시작화면 UI 활성
            //uiManager.startUIPnl.SetActive(true);


            //// UI매니저에서 게임시작 버튼  true 면 
            //bool IsClickStartButton = uiManager.ClickStartButton();

            //if (IsClickStartButton)
            //{
            //    // 시작화면 UI 비활성
            //    startUI.SetActive(false);
            //    // 메인모드 변경
            //    SetgameMode(GameMode.Main);
            //}
            //

            // 스타트에서? 메인에서?            

            SetgameMode(GameMode.Main);
            Process();
        }

        // 메인 모드 프로세스
        private void ProcessMain()
        {
            // 인게임 실행
            // 소코반 초기세팅
            sokobanManager?.InitializeSokoban();
            // 소코반 실행
            //sokobanManager?.StartSokoban();

            // 공지 UI활성(전체 반투명 배경으로 레이캐스트 끔), 배경 블러
            // 시작 버튼 클릭 시 공지 비활성, 배경정상
            // 콘센트로 안내(위치 표시 또는 콘센트 빛(파티클 또는 포인트조명 처리)

            // 코드 꼽기 행동 처리
            // 전체 전원 공급
            // 소코반 게임 공지

            // 종료버튼 클릭 시
            // 앤드모드 변경
            //SetgameMode(GameMode.End);

        }
        
    }   // GameManager 클래스
}

