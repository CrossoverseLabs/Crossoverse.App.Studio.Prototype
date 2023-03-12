using UnityEngine;
using TMPro;

namespace Crossoverse.StudioApp.Presentation.UIView
{
    public class DefaultScreenView : MonoBehaviour
    {
        [SerializeField] private TMP_Text text;
        
        public void SetText(string message)
        {
            text.text = message;
        }
    }
}