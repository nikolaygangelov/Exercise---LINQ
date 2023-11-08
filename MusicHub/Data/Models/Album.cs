﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace MusicHub.Data.Models;

public class Album
{
    public Album()
    {
        Songs = new HashSet<Song>();
    }

    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(Validations.AlbumNameMaxLength)]
    public string Name { get; set; }

    [Required]
    public DateTime ReleaseDate { get; set; }

    public decimal Price => Songs.Sum(s => s.Price);

    public int? ProducerId { get; set; }
    [ForeignKey(nameof(ProducerId))]
    public Producer Producer { get; set; }

    public ICollection<Song> Songs { get; set; }
}

