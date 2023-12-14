using System.Collections;
using System.Collections.Generic;
using UnityEditor.TextCore.Text;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text.RegularExpressions;

public class ScoreBoard : MonoBehaviour
{
    public TMP_Text[] scoresText_EasyPairs;
    public TMP_Text[] dateText_EasyPairs;

    public TMP_Text[] scoresText_MediumPairs;
    public TMP_Text[] dateText_MediumPairs;

    public TMP_Text[] scoresText_HardPairs;
    public TMP_Text[] dateText_HardPairs;
    void Start()
    {
        UpdateScoreBoard();
    }

    public void UpdateScoreBoard()
    {
        Config.UpdateScoreList();

        DisplayPairsScoreData(Config.ScoreTimeListEasyPairs, Config.PairNumberListEasyPairs, scoresText_EasyPairs, dateText_EasyPairs);
        DisplayPairsScoreData(Config.ScoreTimeListMediumPairs, Config.PairNumberListMediumPairs, scoresText_MediumPairs, dateText_MediumPairs);
        DisplayPairsScoreData(Config.ScoreTimeListHardPairs, Config.PairNumberListHardPairs, scoresText_HardPairs, dateText_HardPairs);
    }

    private void DisplayPairsScoreData(float[] scoreTimeList, string[] pairNumberList, TMP_Text[] scoreText, TMP_Text[] dataText)
    {
        for (var index = 0; index < 3; index++)
        {
            if (scoreTimeList[index] > 0)
            {
                var dataTime = Regex.Split(pairNumberList[index], "T");

                var minutes = Mathf.Floor(scoreTimeList[index] / 60);
                float seconds = Mathf.RoundToInt(scoreTimeList[index] % 60);

                scoreText[index].text = minutes.ToString("00") + ":" + seconds.ToString("00");
                dataText[index].text = dataTime[0] + " " + dataTime[1];
            }
            else
            {
                scoreText[index].text = " ";
                dataText[index].text = " ";
            }
        }
    }
}
