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
using RestSharp;

namespace TestCaseMovie
{
    class Program
    {
        // themoviedb API key
        static string apikey = "cb44a0858d07d72908cb02677d4f901a";
        
        // facebook movie page id
        static string fb_pgid = "958521567572809";

        // facebook movie test page id
        static string testid = "1151092078344517";

        // testMovie facebook app id
        const string fb_appid = "211128815964111";
        const string fb_appsecret = "5125245db327178fa2504d97acda4a7e";

        // extended access token
        //const string ext_acctoken = "EAADABUITz88BAPIrNjXThBI4JYZC8YI0wnQAyAD5ZAXlsntSHzZByWevqgkZBej6bZAxmmP7QEHqRV1nZBQwvMz08R8tSUVreOm8NALlln9vPFPnZApPDveKH46sexTjP5N5u9iySRFEyT4jZCyL5ZC1IO5Tk7QzMi6KK5lixR0encgZDZD&expires=5183999";
        const string ext_acctoken = "EAACEdEose0cBACmZA1hd3nQD0ZCob91UPZCAdQqEnIJCJgg7b4Opb4OB6WoZBA33DbxHCsKv2dcZAfHwd9aeZBtvTWw8k3VFj3Ut92LMBeTqs2r05MWpWoUdl2dDHPQFZCZAu4AxaZCMeg2Kkr9iWjMEzmj0Damf84GOcVXdNlvmPZAW2uJnh8FLHX";
        private const string AuthenticationUrlFormat = "https://graph.facebook.com/oauth/access_token?client_id={0}&client_secret={1}&grant_type=client_credentials&scope=manage_pages,offline_access,publish_stream,publish_actions,publish_pages";
        string scope = "publish_actions,manage_pages,publish_stream,pubhlish_pages";

        //static void Main(string[] args)
        //{
        //    //JObject json = JObject.Parse(GetInfo());
        //    //Console.WriteLine(json);
        //    //Console.WriteLine(json.First);
        //    //Console.WriteLine(json.Next);
        //    //Console.WriteLine(json.Next);
        //    //Console.WriteLine(json.Next);
        //    //Console.WriteLine(json.Last);
        //    //Console.WriteLine(json["title"]+" "+json"id"]);
        //    //Console.WriteLine(GetInfo());
        //    //string accessToken = GetAccessToken(fb_appid, fb_appsecret);

        //    //PostFacebook(accessToken, "My message");
        //    //Console.WriteLine(GetAccessToken(fb_appid, fb_appsecret));
        //    //PostToPage("hhhhhhhhhhxxx", GetAccessToken(fb_appid,fb_appsecret), testid);
        //    //Console.WriteLine(GetAccessToken(fb_appid, fb_appsecret));
        //    //PostToPage("hello", ext_acctoken, testid);
        //    JToken json=null;
        //    while (true)
        //    {
        //        if (!JToken.DeepEquals(json,GetJson()))
        //        {
        //            json = GetJson();
        //            var lis = GetInfo(json);
        //            PostToPage(lis[0], lis[1], ext_acctoken, testid);
        //        }
        //    }
            
            
        //}

        // get the synopsis, video url, title of the movie
        public static JToken GetJson()
        {   
            // Get the upcoming movies
            var client = new RestClient("https://api.themoviedb.org/3/movie/upcoming?language=en-US&api_key=" + apikey+"&page=1");
            //var client = new RestClient("https://api.themoviedb.org/3/movie/latest?language=en-US&api_key=" + apikey);
            //var client = new RestClient(" https://api.themoviedb.org/3/movie/346672/videos?api_key=cb44a0858d07d72908cb02677d4f901a&language=en-US");
            var request = new RestRequest(Method.GET);
            request.AddParameter("undefined", "{}", ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);
            JToken jsonlist = JToken.Parse(response.Content);
            JToken json = jsonlist["results"][0];
            //Console.WriteLine(json);
            //Console.WriteLine(info);
            //Console.WriteLine(json);

            
            return json;
        }

        static List<String> GetInfo(JToken json)
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
            Console.WriteLine(json_video);
            string video_key = json_video["results"][0]["key"].ToString();
            string video_url = "https://www.youtube.com/watch?v=" + video_key;

            info.Add(video_url);

            return info;
            
        }

        // Post the info to facebook
        static void PostFacebook(string accesstoken, string info)
        {
            dynamic messagePost = new ExpandoObject();
            messagePost.access_token = accesstoken;
            //messagePost.picture = "[A_PICTURE]";
            //messagePost.link = "[SOME_LINK]";
            //messagePost.name = "[SOME_NAME]";
            //messagePost.caption = "{*actor*} " + "[YOUR_MESSAGE]"; //<---{*actor*} is the user (i.e.: Aaron)
            //messagePost.description = "[SOME_DESCRIPTION]";
            messagePost.message="haha";
            var pageid = 1151092078344517;
            

            try
            {
                FacebookClient app = new FacebookClient(accesstoken);
                var result = app.Post("/" + pageid + "/feed", messagePost);
            }
            catch (FacebookOAuthException ex)
            {
                    //handle something
            }
            catch (FacebookApiException ex)
            {
                    //handle something else
            }
        }

        static string GetAccessToken(string apiId, string apiSecret)
        {
            string accessToken = string.Empty;
            string url = string.Format(AuthenticationUrlFormat, apiId, apiSecret);

            WebRequest request = WebRequest.Create(url);
            WebResponse response = request.GetResponse();

            using (Stream responseStream = response.GetResponseStream())
            {
                StreamReader reader = new StreamReader(responseStream, Encoding.UTF8);
                String responseString = reader.ReadToEnd();

                NameValueCollection query = HttpUtility.ParseQueryString(responseString);

                accessToken = query["access_token"];
            }

            if (accessToken.Trim().Length == 0)
                throw new Exception("There is no Access Token");

            return accessToken;
        }
        private static void PostToPage(string message, string link, string pageAccessToken, string pageId)
        {
            var fb = new FacebookClient(pageAccessToken);
            dynamic messagePost = new ExpandoObject();
            messagePost.access_token = pageAccessToken;
            messagePost.message = message;
            messagePost.link = link;

            var result = fb.Post("/"+pageId+"/feed", messagePost);
        }

        
    }
}
