using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using System.Web.Caching;
using System.Web;
using MusicMashup.Models;
using System.Collections;

namespace MusicMashup.Controllers
{
    public class MusicController : ApiController
    {
        // GET /api/music
        public IEnumerable<Models.MusicModel> Get()
        {
            Cache cache = HttpContext.Current.Cache;

            List<Models.MusicModel> list = (List<Models.MusicModel>)cache["songlist"];
            if (list == null)
            {
                list = new List<Models.MusicModel>();
                foreach (string filepath in System.IO.Directory.GetFiles(HttpContext.Current.Server.MapPath("~/Content/Music")))
                {
                    TagLib.File file = TagLib.File.Create(filepath);
                    Models.MusicModel song = new Models.MusicModel();
                    var s = file.Tag.AlbumArtists.ToList();
                    s.AddRange(file.Tag.Performers.ToList());
                    song.Artist = string.Join(", ", s.ToArray());
                    //song.Artist = string.Join(", ", file.Tag.Performers, file.Tag.AlbumArtists);
                    song.MusicId = Guid.Parse(file.Name.Split('\\').Last().Replace(".mp3",""));
                    song.Length = file.Length;
                    song.Track = file.Tag.Track;
                    song.Title = file.Tag.Title;
                    song.Genres = string.Join(", ", file.Tag.Genres);
                    song.Album = file.Tag.Album;
                    song.Url = "/api/music/" + file.Name.Split('\\').Last().Replace(".mp3", "");

                    list.Add(song);
                    file.Dispose();
                }

                cache.Insert("songlist", list, null, Cache.NoAbsoluteExpiration, TimeSpan.FromMinutes(30));
            }

            return list;
        }

        // GET /api/music/5
        public Models.MusicModel Get(Guid id)
        {
            return Get().SingleOrDefault(x => x.MusicId == id);
        }

        // GET /api/music/5
        public HttpResponseMessage Get(Guid id, string play)
        {
            string musicPath = HttpContext.Current.Server.MapPath("~/Content/Music") + "\\" + id.ToString() + ".mp3";
            if (System.IO.File.Exists(musicPath))
            {
                HttpResponseMessage result = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
                var stream = new System.IO.FileStream(musicPath, System.IO.FileMode.Open);
                result.Content = new StreamContent(stream);
                result.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("audio/mpeg");
                return result;
            }
            else
            {
                return new HttpResponseMessage(System.Net.HttpStatusCode.NotFound);
            }
        }

        // POST /api/music
        public HttpResponseMessage<MusicModel> Post()
        {
            var files = HttpContext.Current.Request.Files;
         
            // This code should work with any number of files uploaded by the user, either by having 
            // multiple <input type="file" /> controls inside the form, or by adding the new multiple="multiple" attribute
            // (which is not supported in all browsers).
            foreach( var item in files.AllKeys )
            {
                var postedFile = files[item];
                string id = Guid.NewGuid().ToString();
                postedFile.SaveAs(HttpContext.Current.Server.MapPath("~/Content/Music") + "\\" + id + ".mp3");
            }
            RemoveCache();
            return new HttpResponseMessage<MusicModel>(System.Net.HttpStatusCode.Accepted);
            
        }

        // PUT /api/music/5
        public HttpResponseMessage Put(Models.MusicModel data)
        {
            HttpResponseMessage theresponse = new HttpResponseMessage();
            
            string filePath = HttpContext.Current.Server.MapPath("~/Content/Music") + "\\" + data.MusicId.ToString() + ".mp3";
            if (System.IO.File.Exists(filePath))
            {
                TagLib.File file = TagLib.File.Create(filePath);
                if (!string.IsNullOrWhiteSpace(data.Title)) { file.Tag.Title = data.Title; }
                if (!string.IsNullOrWhiteSpace(data.Artist)) { file.Tag.Performers = data.Artist.Split(','); }
                if (!string.IsNullOrWhiteSpace(data.Album)) { file.Tag.Album = data.Album; }
                if (!string.IsNullOrWhiteSpace(data.Genres)) { file.Tag.Genres = data.Genres.Split(','); }
                file.Tag.Track = data.Track;
                file.Save();

                theresponse.StatusCode = System.Net.HttpStatusCode.OK;
            }
            else
            {
                theresponse.StatusCode = System.Net.HttpStatusCode.NotFound;
            }
            RemoveCache();
            return theresponse;
        }

        // DELETE /api/music/5
        public HttpResponseMessage Delete(Guid id)
        {
            HttpResponseMessage response = new HttpResponseMessage();

            string filePath = HttpContext.Current.Server.MapPath("~/Content/Music") + "\\" + id.ToString() + ".mp3";
            if (System.IO.File.Exists(filePath))
            {
                System.IO.FileInfo fi = new System.IO.FileInfo(filePath);
                try
                {
                    fi.Delete();
                    response.StatusCode = System.Net.HttpStatusCode.OK;
                }
                catch (System.IO.IOException e)
                {
                    response.StatusCode = System.Net.HttpStatusCode.Unauthorized;
                }
            }
            else
            {
                response.StatusCode = System.Net.HttpStatusCode.NotFound;
            }
            RemoveCache();
            return response;
        }
        public void RemoveCache()
        {
            IDictionaryEnumerator IDenum = HttpContext.Current.Cache.GetEnumerator();
            while (IDenum.MoveNext())
            {
                HttpContext.Current.Cache.Remove(IDenum.Key.ToString());
            }
        }
    }
}
