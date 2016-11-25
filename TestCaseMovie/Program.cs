using System;
using Facebook;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Dynamic;
using System.Net;
using System.IO;
using System.Collections.Specialized;
using System.Web;
using System.Web.UI.WebControls;
using RestSharp;

namespace TestCaseMovie
{
    class Program
    {
        // themoviedb API key
        static string apikey = "cb44a0858d07d72908cb02677d4f901a";
        
        // facebook movieware page id
        static string fb_pgid = "958521567572809";

        // facebook testMovie page id
        static string testMovieId = "1151092078344517";

        // testMovie facebook app id
        // this is the app I created to post on facebook page
        const string fb_appId = "211128815964111";
        const string fb_appSecret = "5125245db327178fa2504d97acda4a7e";

        // this user access token can only be generated in Graph API Explorer
        // Check https://developers.facebook.com/tools/explorer/
        const string fb_appAccessToken = "EAADABUITz88BANAC89k4UWvk5dsMaPmSmFPFNLTKbIa89ReqJrsc6tibm4N90ytZBadbvdYVyZBCn0fy3ZAmWhEHOASW1cIBfuToO6w0rZBGvxMl2Iillqz3FRZASWG9I7q1iERDciluLn2z3aP8Wjq4MRjtI1QVaUZBlKSidWtQZDZD";
        
        // This is permanent page access token to the page
        // I used this directly to post to facebook
        const string perm_access_token = "EAADABUITz88BADhfSXZCvMonwW0tXWVMN8NZCvot1NefPgaJGzmU9RwudaNR2ls8M2MoZAM51LJ4nlwZBaNZCQn7eSatrFYbM23tNmgCfwySdkwXeND2sk2EiyrgxeHKiZAhVLwl45iWHXhge1uo8RuAIWc8GJVwEU4jQrPN08BAZDZD";

        // get the synopsis, video url, title of the movie
        public static JToken GetJson()
        {   
            // Get the upcoming movies
            var client = new RestClient("https://api.themoviedb.org/3/movie/upcoming?language=en-US&api_key=" + apikey+"&page=1");
            var request = new RestRequest(Method.GET);
            request.AddParameter("undefined", "{}", ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);
            JToken jsonlist = JToken.Parse(response.Content);
            return jsonlist["results"][0];            
        }

        // Extract information as string from json
        public static List<String> GetInfo(JToken json)
        {
            List<String> info = new List<String>();
            var title = json["title"].ToString();
            var overview = json["overview"].ToString();
            var id = json["id"].ToString();
            string infoStr = "Title - " + title + "\nOverview - " + overview+"\n";
            info.Add(infoStr);

            // Get youtube video link
            var client = new RestClient("https://api.themoviedb.org/3/movie/" + id + "/videos?language=en-US&api_key=" + apikey);
            var request = new RestRequest(Method.GET);
            request.AddParameter("undefined", "{}", ParameterType.RequestBody);
            var response = client.Execute(request);
            JToken json_video = JToken.Parse(response.Content);
            string video_key = json_video["results"][0]["key"].ToString();
            string video_url = "https://www.youtube.com/watch?v=" + video_key;

            info.Add(video_url);

            return info;
            
        }

        // Get page access token
        // this will return short-lived(2 hrs) page access token
        public static string GetPageAccessToken(string appId, string appSecret, string userAccessToken)
        {

            string pageAccessToken = string.Empty;
            string url = string.Format("https://graph.facebook.com/{0}?fields=access_token&access_token={1}", testMovieId, userAccessToken);

            WebRequest request = WebRequest.Create(url);
            WebResponse response = request.GetResponse();

            using (Stream responseStream = response.GetResponseStream())
            {
                StreamReader reader = new StreamReader(responseStream, Encoding.UTF8);
                String responseString = reader.ReadToEnd();

                NameValueCollection query = HttpUtility.ParseQueryString(responseString);

                pageAccessToken = query["access_token"];
            }
            if (pageAccessToken.Trim().Length == 0)
                throw new Exception("There is no Access Token");

            return pageAccessToken;
        }

        // Post the info to facebook
        public static void PostFacebook(string message, string link, string accessToken, string pageId)
        {
            var fb = new FacebookClient(accessToken);
            dynamic messagePost = new ExpandoObject();
            messagePost.access_token = accessToken;
            messagePost.message = message;
            messagePost.link = link;

            var result = fb.Post("/"+pageId+"/feed", messagePost);
        }

        static void Main(string[] args)
        {
            //JObject json = JObject.Parse(GetInfo());
            JToken json = null;
            while (true)
            {
                if (!JToken.DeepEquals(json, GetJson()))
                {
                    json = GetJson();
                    var lis = GetInfo(json);
                    PostFacebook(lis[0], lis[1], perm_access_token, testMovieId);
                    DatabaseQuery.UpdateTable(json);
                }
            }
        }
    }
}
