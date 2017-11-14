using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.IO;
using PipeDataModel.DataTree;
using System.Runtime.Serialization.Formatters.Binary;

namespace PipeDataModel.Pipe
{
    public class WebPipe : Pipe
    {
        #region-fields
        private string _url;
        private Action _callBack = null;
        private string _result;
        private bool _pullSuccessful = false;

        private static readonly string DATA_RECEIVED = "data_received_eea220ce-73d9-46c9-aa4e-664c8c47510a",
            CLOSE_PIPE = "close_pipe_0decf8c3-4016-4cf7-a8db-40658b720da8",
            PULL_FAILED = "pull_failed_9f8f31d8-ffc8-4d8d-93b3-7f5f2cffcd47";
        #endregion

        #region-properties
        public string Url
        {
            get { return _url; }
        }
        public bool DataPostedToUrlSuccessful
        {
            get { return _result == DATA_RECEIVED; }
        }
        public bool PullDataSuccessful
        {
            get { return _pullSuccessful; }
        }
        #endregion

        #region-constructors
        public WebPipe(string url)
        {
            _url = url;
        }
        public WebPipe(string url, Action callBack):this(url)
        {
            _callBack = callBack;
        }
        #endregion

        #region-base class implementation
        protected override DataNode PullData()
        {
            try
            {
                string jsonStr;
                using (WebClient client = new WebClient())
                {
                    jsonStr = client.DownloadString(_url + "?pull_request=true");
                }
                DataNode node = Deserialize(jsonStr);
                _pullSuccessful = true;
                return node;
            }
            catch(Exception e)
            {
                _pullSuccessful = false;
                return null;
            }
        }

        protected override void PushData(DataNode data)
        {
            NameValueCollection post_data = new NameValueCollection();
            post_data.Add("PIPE_DATA", Serialize(data));

            byte[] result;
            using (WebClient client = new WebClient())
            {
                result = client.UploadValues(_url, "POST", post_data);
            }

            _result = Encoding.UTF8.GetString(result);
            if (_callBack != null) { _callBack.Invoke(); }
        }

        public override void ClosePipe()
        {
            string response;
            using (WebClient client = new WebClient())
            {
                response = client.DownloadString(_url + "?close_pipe="+CLOSE_PIPE);
            }
        }
        #endregion

        #region-methods
        private static string Serialize(DataNode node)
        {
            BinaryFormatter bf = new BinaryFormatter();
            byte[] arr;
            using (MemoryStream ms = new MemoryStream())
            {
                bf.Serialize(ms, node);
                arr = ms.ToArray();
            }

            string byteStr = "";
            foreach(byte b in arr)
            {
                byteStr += b.ToString()+"-";
            }

            return byteStr;
        }

        private static DataNode Deserialize(string byteStr)
        {
            List<byte> byteList = new List<byte>();
            string[] bArr = byteStr.Split('-');
            for (int i = 0; i < bArr.Length-1; i++)
            {
                byteList.Add(Convert.ToByte(bArr[i]));
            }

            byte[] arr = byteList.ToArray();
            BinaryFormatter bf = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream(arr))
            {
                object obj = bf.Deserialize(ms);
                return (DataNode)obj;
            }
        }
        #endregion
    }
}
