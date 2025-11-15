using UnityEngine;
using UnityEngine.UI;

public class TurnFPSOnOff : MonoBehaviour
{
    public TopDownCameraFollow cameraFollow;
    public Transform parentArrowTopdown;
    public Transform parentArrowFirstPerson;
    public Transform arrow;
    public GameObject UIPanelPlayer;
    public Text txtPanelScore;
    public Text txtScore;
    public Text txtStamina;
    public Text txtExp;
    public Text txtPlayerName;
    public Text txtCoins;


    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public void TurnOnOff()
    {
        if (cameraFollow.enableFirstPerson == true)
        {
            cameraFollow.enableFirstPerson = false;
            // arrow.transform.SetParent(null);
            // arrow.transform.SetParent(parentArrowTopdown, false);

        }
        else
        {
            cameraFollow.enableFirstPerson = true;
            // arrow.transform.SetParent(null);
            // arrow.transform.SetParent(parentArrowFirstPerson, false);
        }

        // arrow.transform.localPosition = Vector3.zero;
        // arrow.transform.localRotation = Quaternion.identity;
        // arrow.transform.localScale = Vector3.one;
    }



}
