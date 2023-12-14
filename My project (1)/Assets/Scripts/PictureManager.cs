using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PictureManager : MonoBehaviour
{
    public Picture PicturePrefab;
    public Transform PicSpawnPosition;
    //tava -2.15f, 3.62f
    public Vector2 StartPosition = new Vector2(-2.0f, 2.82f);

    [Space] [Header("End Game Screen")]
    public GameObject EndGamePanel;

    public GameObject NewBestScoreText;
    public GameObject YourScoreText;
    public GameObject EndTimeText;

    public enum GameState
    {
        NoAction,
        MovingOnPositions,
        DeletingPuzzles,
        FlipBack,
        Checking,
        GameEnd
    };

    public enum PuzzleState
    {
        PuzzleRotating,
        CanRotate
    };

    public enum RevealedState
    {
        NoRevealed,
        OneRevealed,
        TwoRevealed
    };

    [HideInInspector]
    public GameState CurrentGameState;
    [HideInInspector]
    public PuzzleState CurrentPuzzleState;
    [HideInInspector]
    public RevealedState PuzzleRevealedNumber;

    [HideInInspector]
    public List<Picture> PictureList;
    //organizar cartas 2.0 2.22
    private Vector2 _offset = new Vector2(2.0f, 2.22f);
    private Vector2 _offsetForMediumPairs = new Vector2(2.0f, 2.22f);
    private Vector2 _offsetForHardPairs = new Vector2(2.0f, 2.22f);
    private Vector3 _newScaleDown = new Vector3(0.9f, 0.9f, 0.001f);

    private List<Material> _materialList = new List<Material>();
    private List<string> _texturePathList = new List<string>();
    private Material _firstMaterial;
    private string _firstTexturePath;

    private int _firstRevealedPic;
    private int _secondRevealedPic;
    private int _revealedPicNumber = 0;
    private int _picToDestroy1;
    private int _picToDestroy2;

    private bool _corutineStarted = false;

    private int _pairNumbers;
    private int _removedPairs;
    private Timer _gameTimer;

    void Start()
    {
        CurrentGameState = GameState.NoAction;
        CurrentPuzzleState = PuzzleState.CanRotate;
        PuzzleRevealedNumber = RevealedState.NoRevealed;
        _revealedPicNumber = 0;
        _firstRevealedPic = -1;
        _secondRevealedPic = -1;

        _removedPairs = 0;
        _pairNumbers = (int) GameSettings.Instance.GetPairNumber();

        _gameTimer = GameObject.Find("Main Camera").GetComponent<Timer>();

        LoadMaterials();
        if (GameSettings.Instance.GetPairNumber() == GameSettings.EPairNumber.EEasyPairs)
        {
            //obs para numero de colunas e linhas tava 3, 2 ; 3,4 ; 4,4 45
            CurrentGameState = GameState.MovingOnPositions;
            SpwanPictureMesh(3, 2, StartPosition, _offset, false);
            MovePicture(3, 2, StartPosition, _offset);
        }
        else if (GameSettings.Instance.GetPairNumber() == GameSettings.EPairNumber.EMediumPairs)
        {
            //obs para numero de colunas e linhas 3, 2 ; tava 3,4 ; 4,4 45
            CurrentGameState = GameState.MovingOnPositions;
            SpwanPictureMesh(3, 4, StartPosition, _offset, false);
            MovePicture(3, 4, StartPosition, _offsetForMediumPairs);
        }
        else if (GameSettings.Instance.GetPairNumber() == GameSettings.EPairNumber.EHardPairs)
        {
            //obs para numero de colunas e linhas 3, 2 ; 3,4 ; 4,4 45 false altera o tamanho das cartas
            CurrentGameState = GameState.MovingOnPositions;
            SpwanPictureMesh(3, 4, StartPosition, _offset, false);
            MovePicture(3, 4, StartPosition, _offsetForHardPairs);
        }
    }

    public void CheckPicture()
    {
        CurrentGameState = GameState.Checking;
        _revealedPicNumber = 0;

        for (int id = 0; id < PictureList.Count; id++)
        {
            if (PictureList[id].Revealed && _revealedPicNumber < 2)
            {
                if(_revealedPicNumber == 0)
                {
                    _firstRevealedPic = id;
                    _revealedPicNumber++;
                }
                else if (_revealedPicNumber == 1)
                {
                    _secondRevealedPic = id;
                    _revealedPicNumber++;
                }
            }
        }
        if (_revealedPicNumber == 2)
        {
            if (PictureList[_firstRevealedPic].GetIndex() == PictureList[_secondRevealedPic].GetIndex() && _firstRevealedPic != _secondRevealedPic)
            {
                CurrentGameState = GameState.DeletingPuzzles;
                _picToDestroy1 = _firstRevealedPic;
                _picToDestroy2 = _secondRevealedPic;
            }
            else
            {
                CurrentGameState = GameState.FlipBack;
            }
        }

        CurrentPuzzleState = PictureManager.PuzzleState.CanRotate;

        if (CurrentGameState == GameState.Checking)
        {
            CurrentGameState = GameState.NoAction;
        }

    }

    private void DestroyPicture()
    {
        PuzzleRevealedNumber = RevealedState.NoRevealed;
        PictureList[_picToDestroy1].Deactivate();
        PictureList[_picToDestroy2].Deactivate();
        _revealedPicNumber = 0;
        _removedPairs++;
        CurrentGameState = GameState.NoAction;
        CurrentPuzzleState = PuzzleState.CanRotate;
    }

    private IEnumerator FlipBack()
    {
        _corutineStarted = true;

        yield return new WaitForSeconds(0.5f);

        PictureList[_firstRevealedPic].FlipBack();
        PictureList[_secondRevealedPic].FlipBack();

        PictureList[_firstRevealedPic].Revealed = false;
        PictureList[_secondRevealedPic].Revealed = false;

        PuzzleRevealedNumber = RevealedState.NoRevealed;
        CurrentGameState = GameState.NoAction;

        _corutineStarted = false;
    }

    private void LoadMaterials()
    {
        var materialFilePath = GameSettings.Instance.GetMaterialDirectoryName();
        var textureFilePath = GameSettings.Instance.GetPuzzleCategoryTextureDirectoryName();
        var pairNumber = (int)GameSettings.Instance.GetPairNumber();
        const string matBaseName = "Pic";
        var firstMaterialName = "Back";

        for(var index = 1; index <= pairNumber; index++)
        {
            var currentFilePath = materialFilePath + matBaseName + index;
            Material mat = Resources.Load(currentFilePath, typeof(Material)) as Material;
            _materialList.Add(mat);

            var currentTextureFilePath = textureFilePath + matBaseName + index;
            _texturePathList.Add(currentTextureFilePath);
        }
        _firstTexturePath = textureFilePath + firstMaterialName;
        _firstMaterial = Resources.Load(materialFilePath + firstMaterialName, typeof(Material)) as Material;
    }
    
    void Update()
    {
      if (CurrentGameState == GameState.DeletingPuzzles)
        {
            if(CurrentPuzzleState == PuzzleState.CanRotate)
            {
                DestroyPicture();
                CheckGameEnd();
            }
        }

      if (CurrentGameState == GameState.FlipBack)
        {
            if (CurrentPuzzleState == PuzzleState.CanRotate && _corutineStarted == false)
            {
                StartCoroutine(FlipBack());
            }
        }
        
      if (CurrentGameState == GameState.GameEnd)
        {
            if (PictureList[_firstRevealedPic].gameObject.activeSelf == false &&
                PictureList[_secondRevealedPic].gameObject.activeSelf == false &&
                EndGamePanel.activeSelf == false)
            {
                ShowEndGameInformation();
            }
        }
    }

    private bool CheckGameEnd()
    {
        if (_removedPairs == _pairNumbers && CurrentGameState != GameState.GameEnd)
        {
            CurrentGameState = GameState.GameEnd;
            _gameTimer.StopTimer();
            Config.PlaceScoreOnBoad(_gameTimer.GetCurrentTime());
        }

        return (CurrentGameState == GameState.GameEnd);
    }

    private void ShowEndGameInformation()
    {
        EndGamePanel.SetActive(true);

        if (Config.IsBestScore())
        {
            NewBestScoreText.SetActive(true);
            YourScoreText.SetActive(false);
        }
        else
        {
            NewBestScoreText.SetActive(false);
            YourScoreText.SetActive(true);
        }

        var timer = _gameTimer.GetCurrentTime();
        var minutes = Mathf.Floor(timer / 60);
        var seconds = Mathf.RoundToInt(timer % 60);
        var newText = minutes.ToString("00") + ":" + seconds.ToString("00");
        EndTimeText.GetComponent<TMP_Text>().text = newText;
    }
    private void SpwanPictureMesh(int rows, int columns, Vector2 Pos, Vector2 offset, bool scaleDown)
    {

        for(int col = 0; col < columns; col++)
        {
            for(int row = 0; row < rows; row++)
            {
                var tempPicture = (Picture)Instantiate(PicturePrefab, PicSpawnPosition.position, PicturePrefab.transform.rotation);

                if (scaleDown)
                {
                    tempPicture.transform.localScale = _newScaleDown;
                }

                tempPicture.name = tempPicture.name + 'c' + col + 'r' + row;
                PictureList.Add(tempPicture);
            }
        }

        ApplyTextures();
    }

    public void ApplyTextures()
    {
        var rndMatIndex = Random.Range(0, _materialList.Count);
        var AppliedTimes = new int[_materialList.Count];

        for(int i=0; i < _materialList.Count; i++)
        {
            AppliedTimes[i] = 0;
        }

        foreach(var o in PictureList)
        {
            var randPrevious = rndMatIndex;
            var counter = 0;
            var forceMat = false;

            while(AppliedTimes[rndMatIndex] >= 2 || ((randPrevious == rndMatIndex) && !forceMat))
            {
                rndMatIndex = Random.Range(0, _materialList.Count);
                counter++;
                if(counter > 100)
                {
                    for(var j =0; j < _materialList.Count; j++)
                    {
                        if (AppliedTimes[j] < 2)
                        {
                            rndMatIndex = j;
                            forceMat = true;
                        }
                    }

                    if (forceMat == false)
                        return;
                }
            }

            o.SetFirstMaterial(_firstMaterial, _firstTexturePath);
            o.ApplyFirstMaterial();
            o.SetSecondMaterial(_materialList[rndMatIndex], _texturePathList[rndMatIndex]);
            o.SetIndex(rndMatIndex);
            o.Revealed = false;
            AppliedTimes[rndMatIndex] += 1;
            forceMat = false;
        }
    }

    private void MovePicture(int rows, int columns, Vector2 pos, Vector2 offset)
    {
        var index = 0;
        for(var col = 0; col < columns; col++)
        {
            for(int row = 0; row < rows; row++)
            {
                var targetPosition = new Vector3((pos.x + (offset.x * row)), (pos.y - (offset.y * col)), 0.0f);
                StartCoroutine(MoveToPosition(targetPosition, PictureList[index]));
                index++;
            }
        }
    }

    private IEnumerator MoveToPosition(Vector3 target, Picture obj)
    {
        var randomDis = 7;

        while(obj.transform.position != target)
        {
            obj.transform.position = Vector3.MoveTowards(obj.transform.position, target, randomDis * Time.deltaTime);
            yield return 0;
        }
    }
}
