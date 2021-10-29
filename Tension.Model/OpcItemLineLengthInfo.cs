using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tension.Model
{
    public class OpcItemLineLengthInfo
    {
        #region ActualLeft
        public string ActualLeft1 { set; get; }
        public string ActualLeft2 { set; get; }
        public string ActualLeft3 { set; get; }
        #endregion

        #region ActualDevLeft
        public string ActualDevLeft1 { set; get; }
        public string ActualDevLeft2 { set; get; }
        public string ActualDevLeft3 { set; get; }
 
        #endregion

        #region AllowDevLeft
        public string AllowDevLeft1 { set; get; }
        public string AllowDevLeft2 { set; get; }
        public string AllowDevLeft3 { set; get; }
        
        #endregion

        #region SampleLeft
        public string SampleLeft1 { set; get; }
        public string SampleLeft2 { set; get; }
        public string SampleLeft3 { set; get; }

        #endregion

        public string PressLeft { set; get; }

        #region ActualRight
        public string ActualRight1 { set; get; }
        public string ActualRight2 { set; get; }
        public string ActualRight3 { set; get; }
  
        #endregion

        #region ActualDevRight
        public string ActualDevRight1 { set; get; }
        public string ActualDevRight2 { set; get; }
        public string ActualDevRight3 { set; get; }

        #endregion

        #region AllowDevRight
        public string AllowDevRight1 { set; get; }
        public string AllowDevRight2 { set; get; }
        public string AllowDevRight3 { set; get; }

        #endregion

        #region SampleRight
        public string SampleRight1 { set; get; }
        public string SampleRight2 { set; get; }
        public string SampleRight3 { set; get; }
        #endregion

        public string PressRight { set; get; }

        #region TurnNo
        public string TurnNoActual { set; get; }
        #endregion

        public string Finished { set; get; }

        public string TypeNo { set; get; }


    }
}
