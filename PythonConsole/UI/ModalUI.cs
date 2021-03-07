using ColossalFramework.UI;

namespace PythonConsole
{
    internal enum MouseButtonState
    {
        None,
        Pressed,
        Held,
        Released,
    }

    internal sealed class ModalUI
    {
        private UIComponent modalView;
        private bool isModal;

        public void Update(bool mouseOverWindow, MouseButtonState middleButtonState)
        {
            if (modalView == null) {
                modalView = UIView.GetAView()?.AddUIComponent(typeof(UILabel));
                if (modalView == null) {
                    return;
                }
                modalView.isInteractive = false;
            }

            if (middleButtonState != MouseButtonState.None) {
                return;
            }

            if (mouseOverWindow) {
                if (!isModal) {
                    isModal = true;
                    UIView.PushModal(modalView);
                }
            } else if (isModal && UIView.GetModalComponent() == modalView) {
                isModal = false;
                UIView.PopModal();
            }
        }
    }
}
