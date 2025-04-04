using System;
using UnityEngine;

public class SessionUIController : MonoBehaviour
{
    public static SessionUIController Instance { get; private set; }
    private SessionUIModel _sessionUIModel;
    [SerializeField] SessionUIView _sessionUIView;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    
}
