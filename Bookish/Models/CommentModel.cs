﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Bookish.Models
{
    /// <summary>
    /// A comment model is the data for a comment on a post
    /// or another comment. It shows the comment and all the 
    /// children comments
    /// </summary>
    public class CommentModel : IListItem
    {
        /// <summary>
        /// The identifier of the comment
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The post the comment is under
        /// </summary>
        public int Post_Id { get; set; }

        /// <summary>
        /// The parent comment id
        /// </summary>
        public int? Parent_Id { get; set; }

        /// <summary>
        /// The body text of the comment
        /// </summary>
        public string Body { get; set; }

        /// <summary>
        /// The time the comment was made
        /// </summary>
        public DateTime Commented_At { get; set; }

        /// <summary>
        /// The total amount of comments under the comment
        /// in order to verify that all comments have been 
        /// loaded or not
        /// </summary>
        public int TotalComments { get; set; }

        /// <summary>
        /// The user who made the comment
        /// </summary>
        public string Commented_By { get; set; }

        /// <summary>
        /// A collection of the comments comments, this
        /// is incomplete at first glance for large comment trees
        /// </summary>
        public List<CommentModel> Comments { get; set; }

        /// <summary>
        /// The rating of the authorized user
        /// if they have rated it before
        /// </summary>
        public RatingModel Rating { get; set; }

        /// <summary>
        /// The post title for when the comment is
        /// detached from the post
        /// </summary>
        public string PostTitle { get; set; }

        /// <summary>
        /// Determines if the comment is hidden
        /// </summary>
        public bool IsHidden { get; set; }

        public DateTime GetCreatedDate()
        {
            return Commented_At;
        }
    }
}
