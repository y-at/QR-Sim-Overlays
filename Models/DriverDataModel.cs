using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iRacing_Quick_Release.Models
{
    public class DriverDataModel
    {
        #region Fields

        private bool _isPlayer;
        private int _id;
        private string _name;
        private int _iRating;
        private string _safetyRating;
        private string _safetyRatingColour;
        private bool _isAI;

        #endregion

        #region Properties
        /// <summary>
        /// Is this driver the player.
        /// </summary>
        public bool IsPlayer
        {
            get { return _isPlayer; }
            set { _isPlayer = value; }
        }
        /// <summary>
        /// Online ID number of the driver
        /// </summary>
        public int Id
        {
            get { return _id; }
            set { _id = value; }
        }
        /// <summary>
        /// Name of the driver.
        /// </summary>
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }
        /// <summary>
        /// iRating of the driver (make skill rating in the future to support more games).
        /// </summary>
        public int IRating
        {
            get { return _iRating; }
            set { _iRating = value; }
        }
        /// <summary>
        /// Safety rating of the driver.
        /// </summary>
        public string SafetyRating
        {
            get { return _safetyRating; }
            set { _safetyRating = value; }
        }
        /// <summary>
        /// Is this car controlled by AI
        /// </summary>
        public bool IsAI
        {
            get { return _isAI; }
            set { _isAI = value; }
        }

        #endregion
    }
}
