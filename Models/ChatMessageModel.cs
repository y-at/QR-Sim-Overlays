using QRO.Enumerations;
using System;

namespace QRO.Models
{
    public class ChatMessageModel
    {
        #region Fields

        private ChatRole _role;
        private string _content;
        private DateTime _timestamp;

        #endregion

        #region Properties

        /// <summary>
        /// Who authored this message.
        /// </summary>
        public ChatRole Role
        {
            get => _role;
            set => _role = value;
        }

        /// <summary>
        /// Message text.
        /// </summary>
        public string Content
        {
            get => _content;
            set => _content = value;
        }

        /// <summary>
        /// When the message was created (local time).
        /// </summary>
        public DateTime Timestamp
        {
            get => _timestamp;
            set => _timestamp = value;
        }

        #endregion

        #region Constructors

        public ChatMessageModel()
        {
            _timestamp = DateTime.Now;
        }

        public ChatMessageModel(ChatRole role, string content)
        {
            _role = role;
            _content = content;
            _timestamp = DateTime.Now;
        }

        #endregion
    }
}
