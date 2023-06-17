using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UniRx;

namespace Crossoverse.StudioApp.Presentation.TitleScreen
{
    public sealed class TitleScreenView : MonoBehaviour
    {
        [SerializeField] TMP_Text _appTitle;
        [SerializeField] TMP_Text _appSubtitle;
        [SerializeField] TMP_Text _appVersion;
        [SerializeField] Button _startButton;
        [SerializeField] Button _licenseButton;

        public IObservable<Unit> OnClickStartButton => _startButton.OnClickAsObservable();
        public IObservable<Unit> OnClickLicenseButton => _licenseButton.OnClickAsObservable();

        public void SetAppTitle(string value)
        {
            _appTitle.text = value;
        }

        public void SetAppVersion(string value)
        {
            _appVersion.text = $"App version: {value}";
        }
    }
}