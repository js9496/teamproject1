using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;


namespace RetroSokoban
{
    public class SokobanManager : MonoBehaviour
    {
        //비어있는 장소를 가리키는 식별값
        private const int Empty = 0;
        //벽을 가리키는 식별 값
        private const int Wall = 1;
        //아이템이 들어갈 장소
        private const int Goal = 2;
        //이동시킬 박스
        private const int Box = 3;
        //플레이어의 식별 값
        private const int Player = 4;

        /// ====== 카메라 관련 ======
        // 카메라
        private Camera gameCamera;
        // sub카메라 게임오브젝트
        private GameObject subCamera;

        // 카메라 위치 리스트
        [SerializeField]
        private List<Vector2> cameraPositions = new List<Vector2>();

        /// ====== 플레이어 관련 ======
        //플레이어
        private Player player;
        // 플레이어 위치
        private Position playerPosition = new Position();

        /// ====== 데이터 관련 ======
        //제이슨 파일에서 가져올 꺼임
        private int[,]? currentBoard = null;
        //private Slot[,] currentBoard = null;

        // 생성된 스프라이트 리스트
        private SpriteRenderer[,] sprites = null;

        // 제이슨 파일에서 받아올 정보
        private List<Sokoban_StageData> stages;

        /// ====== 스테이지 관련 ======
        //전체 스테이지 수
        private int totalStageCount = 0;

        //현재 스테이지
        private int currentStage = 1;

        // 스테이지 오브젝트
        private GameObject stageGameObject;

        // 현재 스테이지의 아이템을 넣을 장소
        private List<Position> goalPositions = new List<Position>(0);

        // 지정한 스테이지의 행과 열값
        private int width;
        private int height;


        private void Awake()
        {
            // subCamera 태그값 지정해서 찾아오기
            subCamera = GameObject.FindGameObjectWithTag("SubCamera");
        }

        // 소코반 초기세팅
        public void InitializeSokoban()
        {
            // 이건 소코반 모드에서???
            if (subCamera != null) gameCamera = subCamera.GetComponent<Camera>();

            // 전체 스테이지를 로드
            stages = LoadJsonDate();
            // 스테이지 데이터 할당
            SetStages(stages);

            // 현재 스테이지를 구성(밑에 있는거 위로 올림)
            Setupstage(currentStage - 1);
        }

        // 소코반 시작
        public void StartSokoban()
        {
            // 현재 스테이지를 구성
            Setupstage(currentStage - 1);
        }


        /// <summary>
        /// Json에서 데이터 로드
        /// </summary>
        private List<Sokoban_StageData> LoadJsonDate()
        {
            // 확장자 없이 파일 이름만 써도 됨 json파일 가져오기
            TextAsset asset = Resources.Load<TextAsset>("JsonFiles/Sokoban");
            stages = JsonConvert.DeserializeObject<List<Sokoban_StageData>>(asset.text);

            return stages;
        }

        /// <summary>
        /// 스테이지 데이터 할당하기 (제이슨에서 읽은후 호출)
        /// </summary>
        /// <param name="stages"></param>
        public void SetStages(List<Sokoban_StageData> stages)
        {
            this.stages = stages;
            totalStageCount = stages.Count;
        }

        /// <summary>
        /// 스테이지들 만들기
        /// </summary>
        /// <param name="stage"></param>
        public void Setupstage(int stage)
        {
            // 스테이지 생성
            CreateStage(stage);

            // 스프라이트 설정
            SetSprits(stageGameObject);

            // 카메라 위치 설정
            SetCameraPosition(stage);

            // 현재 보드 설정
            SetCurrentBoard(stage);

            // Goal 위치 스테이지에서 아이템을 저장할 공간을 찾음
            FindGoalPositions();

            // 플레이어 위치 설정
            SetPlayerPosition();
        }

        /// <summary>
        /// 스테이지 생성 후 스프라이트 삽입
        /// </summary>
        /// <param name="stage"></param>
        private void CreateStage(int stage)
        {
            // 기존 스테이지가 남아있다면 삭제
            if (stageGameObject != null) Destroy(stageGameObject);

            //현재 스테이지 이름을 구함
            string stageText = $"Stage{stage + 1}";
            // 리소스를 로드
            var stageObj = Resources.Load<GameObject>($"Prefabs/Stages/{stageText}");
            if (stageObj == null) return;

            // 지정한 스테이지의 행과 열의 값을 받음(배열의 크기값을 구하고)
            height = stages[stage].Map.GetLength(0);
            width = stages[stage].Map.GetLength(1);
            // 스프라이트 배열을 생성
            sprites = new SpriteRenderer[height, width];

            // 스테이지 생성
            stageGameObject = Instantiate(stageObj, Vector3.zero, Quaternion.identity);
        }

        /// <summary>
        /// 생성된 스테이지를 받아서 스프라이트 삽입
        /// </summary>
        private void SetSprits(GameObject stageGameObject)
        {
            // 스테이지 부모 오브젝트의 트랜스폼
            Transform rootTransform = stageGameObject.transform;

            // rootTransform 오브젝트 밑에 있는 스테이지들 수만큼 반복
            for (int i = 0; i < rootTransform.childCount; i++)
            {
                // 위에서부터 순서대로 child에 넣음 
                Transform child = rootTransform.GetChild(i);
                var rc = child.name.Split(",");
                //  rc name 입력받음
                int.TryParse(rc[0], out int row);
                int.TryParse(rc[1], out int column);

                // 스프라이트 배열에 요소를 채움(row,column 위치에 각각 스프라이트 넣음)
                sprites[row, column] = child.GetComponent<SpriteRenderer>();
            }
        }


        /// <summary>
        /// 카메라 위치 설정
        /// </summary>
        private void SetCameraPosition(int stage)
        {
            // 카메라가 없고 카메라 포지션의 수가 0보다 크면
            if (gameCamera != null && cameraPositions.Count > 0)
            {
                Vector3 position = cameraPositions[stage];
                position.z = gameCamera.transform.position.z;
                // cameraPositions List를 <Position>이 아닌 <Vector2>로 해놓으면 사용가능
                gameCamera.transform.position = position;
            }
        }

        /// <summary>
        /// currentBoard(현재 보드) 설정
        /// </summary>
        private void SetCurrentBoard(int stage)
        {
            //현재 보드에 배열을 할당
            currentBoard = new int[height, width];

            //스테이지 데이터를 CurrtentBoard에복사함
            Array.Copy(stages[stage].Map, currentBoard, currentBoard.Length);
        }

        /// <summary>
        /// 보드판에서 Goal의 위치찾아서 리스트에 저장
        /// </summary>
        private void FindGoalPositions()
        {
            // 저장소의 내용을 삭제
            goalPositions.Clear();

            if (currentBoard == null) return;

            //보드판 순회
            for (int r = 0; r < currentBoard.GetLength(0); r++)
            {
                for (int c = 0; c < currentBoard.GetLength(1); c++)
                {
                    if (currentBoard[r, c] == Goal)
                    {
                        //아이템을 넣을 수 있는 공간인 경우 위치를 저장
                        // 메모리를 할당하면서 저장
                        Position position = new Position { X = c, Y = r };
                        goalPositions.Add(position);
                    }
                }
            }
        }

        /// <summary>
        /// 플레이어 위치 설정
        /// </summary>
        private void SetPlayerPosition()
        {
            // Player 스크립트 찾아오기
            player = FindFirstObjectByType<Player>();

            // 캐릭터의 위치를 찾음
            FindPlayerPosition();

            // 플레이어의 위치를 이동시킴
            player.SetPosition(playerPosition.X, (height - 1) - playerPosition.Y);
        }

        /// <summary>
        /// 2차원 배열에서 캐릭터의 위치를 찾음
        /// </summary>
        public void FindPlayerPosition()
        {
            for (int r = 0; r < currentBoard.GetLength(0); r++)
            {
                for (int c = 0; c < currentBoard.GetLength(1); c++)
                {
                    if (currentBoard[r, c] == Player)
                    {
                        playerPosition.Y = r;
                        playerPosition.X = c;
                        return;
                    }
                }
            }
        }

        // 입력 키 조정
        public void HandleInput(Direction direction)
        {
            //플레이어의 위치를 찾음
            FindPlayerPosition();

            //키 입력을 받아서 처리하는 내용
            InputMoveKey(direction);

            // 골자리가 비어있으면 다시 그리기
            IsGoalEmpty();

            //게임이 클리어 되었을 때 처리
            ClearGame();
        }

        // 입력 키 조정
        public void InputMoveKey(Direction direction)
        {
            //입력된 키값을 받아옴
            //키 입력을 받아서 처리하는 내용
            switch (direction)
            {
                // x-1 캐릭터의 왼쪽                
                case Direction.Left:
                    // 캐릭터의 왼쪽이 비어있거나 아이템을 놓을 수 있는 장소Goal 라면 이동처리
                    if (currentBoard[playerPosition.Y, playerPosition.X - 1] == Empty ||
                        currentBoard[playerPosition.Y, playerPosition.X - 1] == Goal)
                    {
                        // 배열의 값을 갱신하는 코드 임
                        // 캐릭터를 이동
                        currentBoard[playerPosition.Y, playerPosition.X - 1] = Player;

                        //캐릭터가있던 자리에 비어있는 식별코드를 넣음
                        currentBoard[playerPosition.Y, playerPosition.X] = Empty;

                        // 플레이어의 위치를 이동시킴
                        player.SetPosition(playerPosition.X - 1, (height - 1) - playerPosition.Y);
                    }
                    //캐릭터의 왼쪽에 박스가 있다면 처리
                    else if (currentBoard[playerPosition.Y, playerPosition.X - 1] == Box)
                    {
                        //박스옆 -2 이 무엇인지 봐야함(캐릭터의 옆의 옆자리)
                        if (currentBoard[playerPosition.Y, playerPosition.X - 2] == Empty ||
                            currentBoard[playerPosition.Y, playerPosition.X - 2] == Goal)
                        {
                            // 캐릭터를 이동시킬 수 있다면 배열을 갱신
                            // 캐릭터의 옆에 박스를 옮기고
                            currentBoard[playerPosition.Y, playerPosition.X - 2] = Box;
                            // 박스 자리에 캐릭터를 옮기고
                            currentBoard[playerPosition.Y, playerPosition.X - 1] = Player;
                            // 캐릭터 자리는 비어놓음
                            currentBoard[playerPosition.Y, playerPosition.X] = Empty;

                            // 커서의 위치를 이동하고 플레이어를 출력(2칸짜리)

                            // Box 처리
                            sprites[playerPosition.Y, playerPosition.X - 2] = sprites[playerPosition.Y, playerPosition.X - 1];
                            sprites[playerPosition.Y, playerPosition.X - 2].transform.position = new Vector3(playerPosition.X - 2, (height - 1) - playerPosition.Y);
                            sprites[playerPosition.Y, playerPosition.X - 1] = null;

                            // 플레이어 위치 설정
                            player.transform.position = new Vector3(playerPosition.X - 1, (height - 1) - playerPosition.Y);
                        }
                    }
                    break;
                case Direction.Right:
                    // 캐릭터의 오른쪽이 비어있거나 아이템을 놓을 수 있는 장소Goal 라면 이동처리
                    if (currentBoard[playerPosition.Y, playerPosition.X + 1] == Empty ||
                        currentBoard[playerPosition.Y, playerPosition.X + 1] == Goal)
                    {
                        // 배열의 값을 갱신하는 코드 임
                        // 캐릭터를 이동
                        currentBoard[playerPosition.Y, playerPosition.X + 1] = Player;

                        //캐릭터가있던 자리에 비어있는 식별코드를 넣음
                        currentBoard[playerPosition.Y, playerPosition.X] = Empty;

                        // 커서의 위치를 이동하고 플레이어를 출력
                        // 플레이어의 위치를 이동시킴
                        player.SetPosition(playerPosition.X + 1, (height - 1) - playerPosition.Y);

                    }
                    //캐릭터의 오른쪽에 박스가 있다면 처리
                    else if (currentBoard[playerPosition.Y, playerPosition.X + 1] == Box)
                    {
                        //박스옆 -2 이 무엇인지 봐야함(캐릭터의 옆의 옆자리)
                        if (currentBoard[playerPosition.Y, playerPosition.X + 2] == Empty ||
                            currentBoard[playerPosition.Y, playerPosition.X + 2] == Goal)
                        {
                            // 캐릭터를 이동시킬 수 있다면 배열을 갱신
                            // 캐릭터의 옆에 박스를 옮기고
                            currentBoard[playerPosition.Y, playerPosition.X + 2] = Box;
                            // 박스 자리에 캐릭터를 옮기고
                            currentBoard[playerPosition.Y, playerPosition.X + 1] = Player;
                            // 캐릭터 자리는 비어놓음
                            currentBoard[playerPosition.Y, playerPosition.X] = Empty;

                            // 커서의 위치를 이동하고 플레이어를 출력(2칸짜리)
                            // Box 처리
                            sprites[playerPosition.Y, playerPosition.X + 2] =
                                 sprites[playerPosition.Y, playerPosition.X + 1];
                            sprites[playerPosition.Y, playerPosition.X + 2].transform.position =
                                 new Vector3(playerPosition.X + 2, (height - 1) - playerPosition.Y);

                            sprites[playerPosition.Y, playerPosition.X + 1] = null;

                            // 플레이어 위치 설정
                            player.transform.position = new Vector3(playerPosition.X + 1, (height - 1) - playerPosition.Y);
                        }
                    }
                    break;
                case Direction.Up:
                    // 캐릭터의 위쪽이 비어있거나 아이템을 놓을 수 있는 장소Goal 라면 이동처리
                    if (currentBoard[playerPosition.Y - 1, playerPosition.X] == Empty ||
                        currentBoard[playerPosition.Y - 1, playerPosition.X] == Goal)
                    {
                        // 배열의 값을 갱신하는 코드 임
                        // 캐릭터를 이동
                        currentBoard[playerPosition.Y - 1, playerPosition.X] = Player;

                        //캐릭터가있던 자리에 비어있는 식별코드를 넣음
                        currentBoard[playerPosition.Y, playerPosition.X] = Empty;

                        // 플레이어의 위치를 이동시킴
                        player.SetPosition(playerPosition.X, (height - 1) - playerPosition.Y + 1);

                    }
                    //캐릭터의 위쪽에 박스가 있다면 처리
                    else if (currentBoard[playerPosition.Y - 1, playerPosition.X] == Box)
                    {
                        //박스옆 -2 이 무엇인지 봐야함(캐릭터의 위의 위자리)
                        if (currentBoard[playerPosition.Y - 2, playerPosition.X] == Empty ||
                            currentBoard[playerPosition.Y - 2, playerPosition.X] == Goal)
                        {
                            // 캐릭터를 이동시킬 수 있다면 배열을 갱신
                            // 캐릭터의 위에 박스를 옮기고
                            currentBoard[playerPosition.Y - 2, playerPosition.X] = Box;
                            // 박스 자리에 캐릭터를 옮기고
                            currentBoard[playerPosition.Y - 1, playerPosition.X] = Player;
                            // 캐릭터 자리는 비어놓음
                            currentBoard[playerPosition.Y, playerPosition.X] = Empty;

                            // 커서의 위치를 이동하고 플레이어를 출력(2칸짜리)
                            // Box 처리
                            sprites[playerPosition.Y - 2, playerPosition.X] =
                                 sprites[playerPosition.Y - 1, playerPosition.X];
                            sprites[playerPosition.Y - 2, playerPosition.X].transform.position =
                                 new Vector3(playerPosition.X, (height - 1) - playerPosition.Y + 2);

                            sprites[playerPosition.Y - 1, playerPosition.X] = null;

                            // 플레이어 위치 설정
                            player.transform.position = new Vector3(playerPosition.X, (height - 1) - playerPosition.Y + 1);

                        }
                    }
                    break;
                case Direction.Down:
                    // 캐릭터의 아래가 비어있거나 아이템을 놓을 수 있는 장소Goal 라면 이동처리
                    if (currentBoard[playerPosition.Y + 1, playerPosition.X] == Empty ||
                        currentBoard[playerPosition.Y + 1, playerPosition.X] == Goal)
                    {
                        // 배열의 값을 갱신하는 코드 임
                        // 캐릭터를 이동
                        currentBoard[playerPosition.Y + 1, playerPosition.X] = Player;

                        //캐릭터가있던 자리에 비어있는 식별코드를 넣음
                        currentBoard[playerPosition.Y, playerPosition.X] = Empty;

                        // 플레이어의 위치를 이동시킴
                        player.SetPosition(playerPosition.X, (height - 1) - playerPosition.Y - 1);

                    }
                    //캐릭터의 아래에 박스가 있다면 처리
                    else if (currentBoard[playerPosition.Y + 1, playerPosition.X] == Box)
                    {
                        //박스옆 -2 이 무엇인지 봐야함(캐릭터의 아래의 아래자리)
                        if (currentBoard[playerPosition.Y + 2, playerPosition.X] == Empty ||
                            currentBoard[playerPosition.Y + 2, playerPosition.X] == Goal)
                        {
                            // 캐릭터를 이동시킬 수 있다면 배열을 갱신
                            // 캐릭터의 아래에 박스를 옮기고
                            currentBoard[playerPosition.Y + 2, playerPosition.X] = Box;
                            // 박스 자리에 캐릭터를 옮기고
                            currentBoard[playerPosition.Y + 1, playerPosition.X] = Player;
                            // 캐릭터 자리는 비어놓음
                            currentBoard[playerPosition.Y, playerPosition.X] = Empty;

                            // 커서의 위치를 이동하고 플레이어를 출력(2칸짜리)
                            // Box 처리
                            sprites[playerPosition.Y + 2, playerPosition.X] = sprites[playerPosition.Y + 1, playerPosition.X];
                            sprites[playerPosition.Y + 2, playerPosition.X].transform.position = new Vector3(playerPosition.X, (height - 1) - playerPosition.Y - 2);

                            sprites[playerPosition.Y + 1, playerPosition.X] = null;

                            // 플레이어 위치 설정
                            player.transform.position = new Vector3(playerPosition.X, (height - 1) - playerPosition.Y - 1);
                        }
                    }
                    break;
            }
        }


        /// <summary>
        /// 골 들어갔다 나오면 다시 그려주기
        /// </summary>
        public void IsGoalEmpty()
        {
            // 스테이지 추가 모드에서는 순회하는 범위도 변경되어야 함
            for (int i = 0; i < goalPositions.Count; i++)
            {
                int row = goalPositions[i].Y;
                int column = goalPositions[i].X;
                if (currentBoard[row, column] == Empty)
                {
                    currentBoard[row, column] = Goal;
                }
            }
        }

        // 스테이지 클리어시 처리
        public void ClearGame()
        {
            // 골이 비어있지 않으면 true 반환 시
            if (IsLevelCleared())
            {
                if (currentStage >= totalStageCount)
                {
                    // UI가 나와야 함
                    print("모든스테이지를 클리어");
                    print("게임을 종료합니다!");
                    return;
                }
                else // goalPositions 안에 Box 있지 않은게 있으면
                {
                    // UI가 나와야 함
                    print("다음 스테이지를 플레이 하시겠습니까?");
                    print("다음 스테이지로 이동하려면 y키를 입력");

                    // To do...
                    // 다음스테이지이동키(R, A버튼) 입력받기 => 버튼처리
                    ClickNextStageButton();
                    // 밑에 코드 버튼 누를때 처리
                    ++currentStage;
                    Setupstage(currentStage - 1);
                }
            }
        }

        /// <summary>
        /// Goal에 Box존재 유무 확인
        /// </summary>
        /// <returns></returns>
        // 아이템을 저장할 공간을 순회하고 Box가 없으면 false
        private bool IsLevelCleared()
        {
            for (int i = 0; i < goalPositions.Count; i++)
            {
                int row = goalPositions[i].Y;
                int column = goalPositions[i].X;
                // 골에 box가 없으면 넣을 수 있게
                if (currentBoard[row, column] != Box)
                    return false;
            }
            return true;
        }


        // 다음스테이지이동키(R) 입력받기 => UI매니저로 옮기기
        public void ClickNextStageButton()
        {
            // vr에선 A버튼
        }

        // 소코반 스테이지 내 이동처리
        /// <summary>
        /// 좌,우 이동처리
        /// </summary>
        public void MoveHorizontal(int moveNumber1, int moveNumber2)
        {
            if (currentBoard[playerPosition.Y, playerPosition.X + moveNumber1] == Empty ||
                        currentBoard[playerPosition.Y, playerPosition.X + moveNumber1] == Goal)
            {
                // 배열의 값을 갱신하는 코드 임
                // 캐릭터를 이동
                currentBoard[playerPosition.Y, playerPosition.X + moveNumber1] = Player;

                //캐릭터가있던 자리에 비어있는 식별코드를 넣음
                currentBoard[playerPosition.Y, playerPosition.X] = Empty;

                // 플레이어의 위치를 이동시킴
                player.SetPosition(playerPosition.X + moveNumber1, (height - 1) - playerPosition.Y);
            }
            //캐릭터의 왼쪽에 박스가 있다면 처리
            else if (currentBoard[playerPosition.Y, playerPosition.X + moveNumber1] == Box)
            {
                //박스옆 -2 이 무엇인지 봐야함(캐릭터의 옆의 옆자리)
                if (currentBoard[playerPosition.Y, playerPosition.X + moveNumber2] == Empty ||
                    currentBoard[playerPosition.Y, playerPosition.X + moveNumber2] == Goal)
                {
                    // 캐릭터를 이동시킬 수 있다면 배열을 갱신
                    // 캐릭터의 옆에 박스를 옮기고
                    currentBoard[playerPosition.Y, playerPosition.X + moveNumber2] = Box;
                    // 박스 자리에 캐릭터를 옮기고
                    currentBoard[playerPosition.Y, playerPosition.X + moveNumber1] = Player;
                    // 캐릭터 자리는 비어놓음
                    currentBoard[playerPosition.Y, playerPosition.X] = Empty;

                    // 커서의 위치를 이동하고 플레이어를 출력(2칸짜리)

                    // Box 처리
                    sprites[playerPosition.Y, playerPosition.X + moveNumber2] = sprites[playerPosition.Y, playerPosition.X + moveNumber1];
                    sprites[playerPosition.Y, playerPosition.X + moveNumber2].transform.position = new Vector3(playerPosition.X + moveNumber2, (height - 1) - playerPosition.Y);
                    sprites[playerPosition.Y, playerPosition.X + moveNumber1] = null;

                    // 플레이어 위치 설정
                    player.transform.position = new Vector3(playerPosition.X + moveNumber1, (height - 1) - playerPosition.Y);
                }
            }
        }

        /// <summary>
        /// 위, 아래 이동처리
        /// </summary>
        public void MoveVertical(int moveNumber1, int moveNumber2)
        {
            // 캐릭터의 위쪽이 비어있거나 아이템을 놓을 수 있는 장소Goal 라면 이동처리
            if (currentBoard[playerPosition.Y + moveNumber1, playerPosition.X] == Empty ||
                currentBoard[playerPosition.Y + moveNumber1, playerPosition.X] == Goal)
            {
                // 배열의 값을 갱신하는 코드 임
                // 캐릭터를 이동
                currentBoard[playerPosition.Y + moveNumber1, playerPosition.X] = Player;

                //캐릭터가있던 자리에 비어있는 식별코드를 넣음
                currentBoard[playerPosition.Y, playerPosition.X] = Empty;

                // 플레이어의 위치를 이동시킴
                player.SetPosition(playerPosition.X, (height - 1) - playerPosition.Y + moveNumber1);

            }
            //캐릭터의 위쪽에 박스가 있다면 처리
            else if (currentBoard[playerPosition.Y + moveNumber1, playerPosition.X] == Box)
            {
                //박스옆 -2 이 무엇인지 봐야함(캐릭터의 위의 위자리)
                if (currentBoard[playerPosition.Y + moveNumber2, playerPosition.X] == Empty ||
                    currentBoard[playerPosition.Y + moveNumber2, playerPosition.X] == Goal)
                {
                    // 캐릭터를 이동시킬 수 있다면 배열을 갱신
                    // 캐릭터의 위에 박스를 옮기고
                    currentBoard[playerPosition.Y + moveNumber2, playerPosition.X] = Box;
                    // 박스 자리에 캐릭터를 옮기고
                    currentBoard[playerPosition.Y + moveNumber1, playerPosition.X] = Player;
                    // 캐릭터 자리는 비어놓음
                    currentBoard[playerPosition.Y, playerPosition.X] = Empty;

                    // 커서의 위치를 이동하고 플레이어를 출력(2칸짜리)
                    // Box 처리
                    sprites[playerPosition.Y + moveNumber2, playerPosition.X] =
                         sprites[playerPosition.Y + moveNumber1, playerPosition.X];
                    sprites[playerPosition.Y + moveNumber2, playerPosition.X].transform.position =
                         new Vector3(playerPosition.X, (height - 1) - playerPosition.Y + moveNumber2);

                    sprites[playerPosition.Y + moveNumber1, playerPosition.X] = null;

                    // 플레이어 위치 설정
                    player.transform.position = new Vector3(playerPosition.X, (height - 1) - playerPosition.Y + moveNumber1);
                }
            }
        }



        /// <summary>
        /// 게임 리셋
        /// </summary>
        public void GameReset()
        {
            // 현재 스테이지 번호
            currentStage = 1;
            // 현재 스테이지를 구성
            Setupstage(currentStage - 1);
        }

    } // 소코반 매니저 클래스
}
