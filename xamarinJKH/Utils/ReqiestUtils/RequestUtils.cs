using xamarinJKH.Server;

namespace xamarinJKH.Utils.ReqiestUtils
{
    public class RequestUtils
    {
        public static async void UpdateRequestCons()
        {
            RestClientMP _server = new RestClientMP();
            var requestsListConst = await _server.GetRequestsListConst();
            Settings.UpdateKey = requestsListConst.UpdateKey;
        }
    }
}