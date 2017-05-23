using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FEA_BusinessLogic
{
    public class BaseJsonResult
    {
        public BaseJsonResult()
        {
            this.ErrorCode = 0;
            this.Message = String.Empty;
        }
        public BaseJsonResult(int errorCode,string message)
        {
            this.ErrorCode = errorCode;
            this.Message = message;
        }

        public BaseJsonResult(Exception ex)
        {
            this.ErrorCode = -1;
            this.Message = ex.Message;
            this.ObjectResult = ex.Message;
        }

        public int ErrorCode { set; get; }//1 has error, 0: true
        public string Message { set; get; }
        public object ObjectResult { set; get; }
    }
}
