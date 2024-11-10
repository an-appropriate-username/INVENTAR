using System;

namespace INVApp.Services
{
    public class NotificationService
    {
        #region Delegates and Events

        public delegate void NotifyEventHandler(string message);
        public delegate void ConfirmEventHandler(string message);

        /// <summary>
        /// Event triggered when a notification is sent.
        /// </summary>
        public event NotifyEventHandler? OnNotify;
        public event ConfirmEventHandler? OnConfirm;

        #endregion

        #region Public Methods

        /// The notification message to be broadcast.
        public void Notify(string message)
        {
            OnNotify?.Invoke(message);
        }

        public void Confirm(string message)
        {
            OnConfirm?.Invoke(message);
        }

        #endregion
    }
}
