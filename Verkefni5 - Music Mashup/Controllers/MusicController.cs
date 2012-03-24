using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using System.Web.Caching;
using System.Web;
using MusicMashup.Models;

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
                    song.Genere = string.Join(", ", file.Tag.Genres);
                    song.Disk = file.Tag.Album;
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

        // POST /api/music
        public HttpResponseMessage<MusicModel> Post(dynamic data)
        {
            var files = HttpContext.Current.Request.Files;

            // This code should work with any number of files uploaded by the user, either by having 
            // multiple <input type="file" /> controls inside the form, or by adding the new multiple="multiple" attribute
            // (which is not supported in all browsers).
            foreach( var item in files.AllKeys )
            {
                var postedFile = files[item];
                postedFile.SaveAs( "/Content/Music" );
            }
            return new HttpResponseMessage<MusicModel>(System.Net.HttpStatusCode.Accepted);
        }

        // PUT /api/music/5
        public HttpResponseMessage Put(dynamic data)
        {
            //TODO: Fá lag og vista.. skila réttu responsi.. eyða Cache
            
            return new HttpResponseMessage(System.Net.HttpStatusCode.Accepted);
        }

        // DELETE /api/music/5
        public void Delete(int id)
        {
            //Á þetta að vera hægt???
        }
    }
}
