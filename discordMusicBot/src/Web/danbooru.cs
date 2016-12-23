using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace discordMusicBot.src.Web
{
    class danbooru
    {

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

        public string[] danSearchTag(string tag)
        {
            try
            {
                string ParseValue = $"https://danbooru.donmai.us/posts.json?limit=100&tags={tag}";

                HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(ParseValue);
                httpWebRequest.Method = WebRequestMethods.Http.Get;

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
                        var json = readStream.ReadToEnd();

                        var t = JsonConvert.DeserializeObject<List<ListDanbooruRoot>>(json);

                        if(t.Count >= 1)
                        {
                            Random rng = new Random();

                            bool loop = true;
                            while(loop == true)
                            {
                                int counter = rng.Next(0, t.Count);
                                if(t[counter].file_url != "")
                                {
                                    string url = "https://danbooru.donmai.us" + t[counter].file_url;

                                    string[] returnResult = { url, "Danbooru", t[counter].tag_string };
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
                }
            }
            catch(Exception error)
            {
                return null;
            }
        }
    }
}
