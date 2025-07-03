using System;
using Sample.Solver;
using UnityEngine;

namespace Sample.Visual
{
    public class VisualDot : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Color defaultSelectedColor;
        [SerializeField] private Color lockedSelectedColor;
        [SerializeField] private Color lockedColor;
        [SerializeField] private Color defaultColor;

        public event Action<VisualDot> OnSelected = delegate { }; 
        public event Action<VisualDot> OnLockRequested = delegate { };

        private bool _isSelected = false;
        private bool _isLocked = false;

        public void SetIsSelected(bool isSelected)
        {
            _isSelected = isSelected;
            
            if (_isSelected)
            {
                OnSelected(this);
            }
            
            UpdateSelectedColor();
        }

        public void RequestToggleLock()
        {
            OnLockRequested(this);
        }

        public void SetIsLocked(bool isLocked)
        {
            _isLocked = isLocked;
            UpdateSelectedColor();
        }

        private void UpdateSelectedColor()
        {
            if (_isSelected)
            {
                spriteRenderer.color = _isLocked ? lockedSelectedColor : defaultSelectedColor;
            }
            else
            {
                spriteRenderer.color = _isLocked ? lockedColor : defaultColor;
            }
        }
    }
}