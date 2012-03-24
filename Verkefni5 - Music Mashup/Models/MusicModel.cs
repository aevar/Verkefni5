using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace MusicMashup.Models
{
    public class MusicModel
    {
        [Key]
        public Guid MusicId { get; set; }
        public string Title { get; set; }
        public string Artist { get; set; }
        public string Album { get; set; }
        public uint Track { get; set; }
        public long Length { get; set; }
        public string Genres { get; set; }

        public string Url { get; set; }
    }
}