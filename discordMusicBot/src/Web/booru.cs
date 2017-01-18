using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using discordMusicBot.src.sys;

namespace discordMusicBot.src.Web
{
    class booru
    {

        logs _logs = new logs();

        public class Datum
        {
            public string file { get; set; }
            public int delay { get; set; }
        }

        public class PixivUgoiraFrameData
        {
            public int id { get; set; }
            public int post_id { get; set; }
            public List<Datum> data { get; set; }
            public string content_type { get; set; }
        }

        public class ListDanbooruRoot
        {
            public int id { get; set; }
            public string created_at { get; set; }
            public int uploader_id { get; set; }
            public int score { get; set; }
            public string source { get; set; }
            public string md5 { get; set; }
            public string last_comment_bumped_at { get; set; }
            public string rating { get; set; }
            public int image_width { get; set; }
            public int image_height { get; set; }
            public string tag_string { get; set; }
            public bool is_note_locked { get; set; }
            public int fav_count { get; set; }
            public string file_ext { get; set; }
            public string last_noted_at { get; set; }
            public bool is_rating_locked { get; set; }
            public int? parent_id { get; set; }
            public bool has_children { get; set; }
            public int? approver_id { get; set; }
            public int tag_count_general { get; set; }
            public int tag_count_artist { get; set; }
            public int tag_count_character { get; set; }
            public int tag_count_copyright { get; set; }
            public int file_size { get; set; }
            public bool is_status_locked { get; set; }
            public string fav_string { get; set; }
            public string pool_string { get; set; }
            public int up_score { get; set; }
            public int down_score { get; set; }
            public bool is_pending { get; set; }
            public bool is_flagged { get; set; }
            public bool is_deleted { get; set; }
            public int tag_count { get; set; }
            public string updated_at { get; set; }
            public bool is_banned { get; set; }
            public int? pixiv_id { get; set; }
            public string last_commented_at { get; set; }
            public bool has_active_children { get; set; }
            public int bit_flags { get; set; }
            public string uploader_name { get; set; }
            public bool has_large { get; set; }
            public string tag_string_artist { get; set; }
            public string tag_string_character { get; set; }
            public string tag_string_copyright { get; set; }
            public string tag_string_general { get; set; }
            public bool has_visible_children { get; set; }
            public string file_url { get; set; }
            public string large_file_url { get; set; }
            public string preview_file_url { get; set; }
            public PixivUgoiraFrameData pixiv_ugoira_frame_data { get; set; }
        }

        public async Task<string[]> webRequestStart(string site, string tag)
        {
            try
            {
                await Task.Delay(1);

                string returnURL = await webURL(site, tag); //get the url that is going to be parsed

                string returnJson = await webParse(returnURL); //hit the url and get the return infomation

                string[] returnResult = null;

                switch (site) //figure out what site we hit
                {
                    case "danbooru":
                        returnResult = await webParseDan(returnJson, tag);
                        return returnResult;
                    case "konachan":
                        returnResult = await webParseKonachan(returnJson, tag);
                        return returnResult;
                    case "yandere":
                        returnResult = await webParseYandere(returnJson, tag);
                        return returnResult;
                    case "rule34":
                        return returnResult;
                    default:
                        return null;
                }
            }
            catch(Exception error)
            {
                _logs.logMessage("Error", "system.cmd_play", error.ToString(), "system");
                return null;
            }
        }

        private async Task<string> webURL(string site, string tag)
        {
            try
            {
                await Task.Delay(1);

                string url = null;
                switch (site)
                {
                    case "danbooru":
                        url = $"https://danbooru.donmai.us/posts.json?limit=100";
                        break;
                    case "konachan":
                        url = $"https://konachan.com/post.json?limit=100";
                        break;
                    case "yandere":
                        url = $"https://yande.re/post.json?limit=100";
                        break;
                    case "rule34":
                        url = $"https://rule34.xxx/index.php?page=dapi&s=post&q=index";
                        break;
                    default:
                        //error out
                        return null;
                }

                if(tag != null)
                {
                    url = url + $"&tags={tag}";
                }

                return url;
            }
            catch(Exception error)
            {
                _logs.logMessage("Error", "booru.webRequest", error.ToString(), "system");
                return null;
            }
        }

        private async Task<string> webParse(string url)
        {
            try
            {
                await Task.Delay(1);

                HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(url);

                if (url.Contains("konachan.com"))
                {
                    httpWebRequest.Method = WebRequestMethods.Http.Post;
                }
                else
                {
                    httpWebRequest.Method = WebRequestMethods.Http.Get;
                }
                
                // This goes out and actually performs the web client request sorta thing	
                using (HttpWebResponse response = (HttpWebResponse)httpWebRequest.GetResponse())
                {
                    // Get the stream associated with the response.
                    // so the webpage ran, it did it's thing, now we need the stream of data obtained
                    Stream receiveStream = response.GetResponseStream();

                    using (StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8))
                    {
                        // We got the stream recieved!
                        // Let's ReadToEnd (meaning, let's read to the end of the stream, it outputs the whole stream obtained as a string)
                        string json = readStream.ReadToEnd();

                        return json;
                    }
                }
            }
            catch(Exception error)
            {
                _logs.logMessage("Error", "booru.webParse", error.ToString(), "system");
                return null;
            }
        }
        
        private async Task<string[]> webParseDan(string rawJson,string tag)
        {
            try
            {
                await Task.Delay(1);

                var json = JsonConvert.DeserializeObject<List<ListDanbooruRoot>>(rawJson);

                if (json.Count >= 1)
                {
                    Random rng = new Random();

                    bool loop = true;
                    while(loop == true)
                    {
                        int counter = rng.Next(0, json.Count);
                        if(json[counter].file_url != "")
                        {
                            string url = "https://danbooru.donmai.us" + json[counter].file_url;

                            string[] returnResult = { url, "Danbooru", json[counter].tag_string, tag };
                            return returnResult;
                        }
                    }
                    return null;
                }
                else
                {
                    //return we found nothing
                    return null;
                }
            }
            catch(Exception error)
            {
                _logs.logMessage("Error", "booru.webParseDan", error.ToString(), "system");
                return null;
            }
        }

        public class ListKonachan
        {
            public int id { get; set; }
            public string tags { get; set; }
            public int created_at { get; set; }
            public int creator_id { get; set; }
            public string author { get; set; }
            public int change { get; set; }
            public string source { get; set; }
            public int score { get; set; }
            public string md5 { get; set; }
            public int file_size { get; set; }
            public string file_url { get; set; }
            public bool is_shown_in_index { get; set; }
            public string preview_url { get; set; }
            public int preview_width { get; set; }
            public int preview_height { get; set; }
            public int actual_preview_width { get; set; }
            public int actual_preview_height { get; set; }
            public string sample_url { get; set; }
            public int sample_width { get; set; }
            public int sample_height { get; set; }
            public int sample_file_size { get; set; }
            public string jpeg_url { get; set; }
            public int jpeg_width { get; set; }
            public int jpeg_height { get; set; }
            public int jpeg_file_size { get; set; }
            public string rating { get; set; }
            public bool has_children { get; set; }
            public int? parent_id { get; set; }
            public string status { get; set; }
            public int width { get; set; }
            public int height { get; set; }
            public bool is_held { get; set; }
            public string frames_pending_string { get; set; }
            public List<object> frames_pending { get; set; }
            public string frames_string { get; set; }
            public List<object> frames { get; set; }
        }

        private async Task<string[]> webParseKonachan(string rawJson, string tag)
        {
            try
            {
                await Task.Delay(1);

                var json = JsonConvert.DeserializeObject<List<ListKonachan>>(rawJson);
                
                if (json.Count >= 1)
                {
                    Random rng = new Random();

                    bool loop = true;
                    while(loop == true)
                    {
                        int counter = rng.Next(0, json.Count);
                        if (json[counter].file_url != "")
                        {
                            string[] returnResult = { "https:"+json[counter].jpeg_url, "Konachan", json[counter].tags, tag };
                            return returnResult;
                        }
                    }
                }
                return null;
            }
            catch(Exception error)
            {
                _logs.logMessage("Error", "danbooru.webParseKonachan", error.ToString(), "system");
                return null;
            }
        }

        public class ListYandere
        {
            public int id { get; set; }
            public string tags { get; set; }
            public int created_at { get; set; }
            public int updated_at { get; set; }
            public int? creator_id { get; set; }
            public object approver_id { get; set; }
            public string author { get; set; }
            public int change { get; set; }
            public string source { get; set; }
            public int score { get; set; }
            public string md5 { get; set; }
            public int file_size { get; set; }
            public string file_ext { get; set; }
            public string file_url { get; set; }
            public bool is_shown_in_index { get; set; }
            public string preview_url { get; set; }
            public int preview_width { get; set; }
            public int preview_height { get; set; }
            public int actual_preview_width { get; set; }
            public int actual_preview_height { get; set; }
            public string sample_url { get; set; }
            public int sample_width { get; set; }
            public int sample_height { get; set; }
            public int sample_file_size { get; set; }
            public string jpeg_url { get; set; }
            public int jpeg_width { get; set; }
            public int jpeg_height { get; set; }
            public int jpeg_file_size { get; set; }
            public string rating { get; set; }
            public bool is_rating_locked { get; set; }
            public bool has_children { get; set; }
            public int? parent_id { get; set; }
            public string status { get; set; }
            public bool is_pending { get; set; }
            public int width { get; set; }
            public int height { get; set; }
            public bool is_held { get; set; }
            public string frames_pending_string { get; set; }
            public List<object> frames_pending { get; set; }
            public string frames_string { get; set; }
            public List<object> frames { get; set; }
            public bool is_note_locked { get; set; }
            public int last_noted_at { get; set; }
            public int last_commented_at { get; set; }
        }

        private async Task<string[]> webParseYandere(string rawJson,string tag)
        {
            try
            {
                await Task.Delay(1);
                var json = JsonConvert.DeserializeObject<List<ListYandere>>(rawJson);

                if (json.Count >= 1)
                {
                    Random rng = new Random();

                    bool loop = true;
                    while (loop == true)
                    {
                        int counter = rng.Next(0, json.Count);
                        if (json[counter].file_url != "")
                        {
                            string[] returnResult = { json[counter].jpeg_url, "Yandere", json[counter].tags, tag };
                            return returnResult;
                        }
                    }
                }
                return null;
            }
            catch (Exception error)
            {
                _logs.logMessage("Error", "danbooru.webParseYandere", error.ToString(), "system");
                return null;
            }
        }
    }
}
