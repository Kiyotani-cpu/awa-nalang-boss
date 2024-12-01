using UnityEngine;

public class LevelSelect : MonoBehaviour
{
    public void OnLevel1Selected()
    {
        GameManager.Instance.SelectLevel(1);
    }

    public void OnLevel2Selected()
    {
        GameManager.Instance.SelectLevel(2);
    }

    public void OnLevel3Selected()
    {
        GameManager.Instance.SelectLevel(3);
    }

    public void OnBackToHome()
    {
        GameManager.Instance.LoadLevelSelectScene();
    }
}
