using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using Mirror;

public class UIVirtualButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
{
    // ==============================
    // ENUM FOR BUTTON TYPE
    // ==============================
    public enum ButtonType
    {
        Run,
        Pick,
        Throw
    }

    [System.Serializable]
    public class BoolEvent : UnityEvent<bool> { }

    [System.Serializable]
    public class Event : UnityEvent { }

    // ==============================
    // INSPECTOR FIELDS
    // ==============================

    [Header("Multiplayer Settings")]
    [Tooltip("Enable if this scene uses Mirror multiplayer networking.")]
    public bool isMultiplayer = false;

    [Header("Output Events")]
    public BoolEvent buttonStateOutputEvent;
    public Event buttonClickOutputEvent;

    [Header("Network Character References (for Multiplayer)")]
    public NetworkCharacterControllerMovement NetCharacterControllerMovement;
    public NetworkPickAndThrow NetworkPickAndThrow;

    [Header("Local Character References (for Single Player)")]
    public CharacterControllerMovement localCharacterController;
    public PickandThrow localPickAndThrow;

    [Header("Button Configuration")]
    public ButtonType buttonAction = ButtonType.Run;

    // ==============================
    // UNITY EVENT HANDLERS
    // ==============================
    public void OnPointerDown(PointerEventData eventData)
    {
        OutputButtonStateValue(true);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        OutputButtonStateValue(false);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        OutputButtonClickEvent();
    }

    // ==============================
    // EVENT OUTPUTS
    // ==============================
    void OutputButtonStateValue(bool buttonState)
    {
        buttonStateOutputEvent.Invoke(buttonState);
        HandleButtonAction(buttonState);
    }

    void OutputButtonClickEvent()
    {
        buttonClickOutputEvent.Invoke();

        switch (buttonAction)
        {
            case ButtonType.Pick:
                TryPick();
                break;

            case ButtonType.Throw:
                TryThrow();
                break;
        }
    }

    // ==============================
    // BUTTON LOGIC
    // ==============================
    void HandleButtonAction(bool isPressed)
    {
        if (buttonAction == ButtonType.Run && isPressed)
        {
            if (isMultiplayer)
            {
                // Multiplayer mode
                if (NetCharacterControllerMovement != null && NetCharacterControllerMovement.isLocalPlayer)
                {
                    NetCharacterControllerMovement.MobileRunInput();
                }
                else
                {
                    Debug.LogWarning("NetCharacterControllerMovement is missing or not local player.");
                }
            }
            else
            {
                // Single-player mode
                if (localCharacterController != null)
                {
                    localCharacterController.MobileRunInput();
                }
                else
                {
                    Debug.LogWarning("localCharacterController is missing for single-player mode.");
                }
            }
        }
    }

    void TryPick()
    {
        if (isMultiplayer)
        {
            if (NetworkPickAndThrow != null)
            {
                NetworkPickAndThrow.MobilePickup();
            }
            else
            {
                Debug.LogWarning("NetworkPickAndThrow not assigned in multiplayer mode.");
            }
        }
        else
        {
            if (localPickAndThrow != null)
            {
                localPickAndThrow.MobilePickup();
            }
            else
            {
                Debug.LogWarning("localPickAndThrow not assigned for single-player mode.");
            }
        }
    }

    void TryThrow()
    {
        if (isMultiplayer)
        {
            if (NetworkPickAndThrow != null)
            {
                NetworkPickAndThrow.MobileThrow();
            }
            else
            {
                Debug.LogWarning("NetworkPickAndThrow not assigned in multiplayer mode.");
            }
        }
        else
        {
            if (localPickAndThrow != null)
            {
                localPickAndThrow.MobileThrow();
            }
            else
            {
                Debug.LogWarning("localPickAndThrow not assigned for single-player mode.");
            }
        }
    }
}
