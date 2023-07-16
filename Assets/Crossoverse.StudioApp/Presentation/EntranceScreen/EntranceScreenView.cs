using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UniRx;

namespace Crossoverse.StudioApp.Presentation.EntranceScreen
{
    public sealed class EntranceScreenView : MonoBehaviour
    {
        [SerializeField] TMP_Text _contentTitle;
        [SerializeField] Button _enterButton;

        public IObservable<string> OnClickEnterButton => _enterSubject.TakeUntilDestroy(this);

        private readonly Subject<string> _enterSubject = new();

        private string _selectedContentId;

        void Awake()
        {
            _enterButton.OnClickAsObservable()
                .TakeUntilDestroy(this)
                .Subscribe(_ => 
                {
                    _enterSubject.OnNext(_selectedContentId);
                });

            _selectedContentId = "SampleContent01_StreamingAssets";
            _contentTitle.text = _selectedContentId;
        }

        public void SetContentTitle(string value)
        {
            throw new NotImplementedException();
        }
    }
}