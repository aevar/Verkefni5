using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace MusicMashup.Models
{
    public class MusicModel
    {
        [Required]
        string Title { get; set; }
        [Required]
        string Artist { get; set; }
        string Disk { get; set; }
        int Track { get; set; }
        int Length { get; set; }
        string Genere { get; set; }
    }
}