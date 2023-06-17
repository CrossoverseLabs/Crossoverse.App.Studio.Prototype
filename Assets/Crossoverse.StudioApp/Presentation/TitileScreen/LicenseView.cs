using System;
using UnityEngine;
using UnityEngine.UI;
using UniRx;

namespace Crossoverse.StudioApp.Presentation.TitleScreen
{
    public sealed class LicenseView : MonoBehaviour
    {
        [SerializeField] Button _closeButton;

        IObservable<Unit> OnClickCloseButton => _closeButton.OnClickAsObservable();

        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public void SetLicenseText(string value)
        {
            throw new NotImplementedException();
        }
    }
}