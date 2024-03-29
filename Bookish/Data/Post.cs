﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Bookish.Data
{
    public class Post
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public string BookTitle { get; set; }

        public string Author { get; set; }

        /// <summary>
        /// The works id allows for utilizing
        /// Open Libary API
        /// </summary>
        public string WorksId { get; set; }

        [MaxLength(13)]
        public string ISBN { get; set; }

        public string Body { get; set; }

        public DateTime Posted_At { get; set; }

        public int Posted_ById { get; set; }
        [ForeignKey("Posted_ById")]
        public User Posted_By { get; set; }

        public bool IsHidden { get; set; }

        public ICollection<Comment> Comments { get; set; }

        public ICollection<Rating> Ratings { get; set; }
    }

}
