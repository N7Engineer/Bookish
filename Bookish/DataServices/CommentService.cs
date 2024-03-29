﻿using Bookish.Data;
using Bookish.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Bookish.DataServices
{
    /// <summary>
    /// Comment service is basic CRUD for comments
    /// </summary>
    public class CommentService : ICommentService
    {
        private Context context;

        private IMessageService messageService;

        public CommentService(Context context, IMessageService messageService)
        {
            this.context = context;
            this.messageService = messageService;
        }

        /// <summary>
        /// CommentModel queryable that is able to determine the total
        /// amount of sub comments by utilizing a subquery
        /// </summary>
        /// <param name="predicate">The where predicate for filtering</param>
        /// <returns>
        /// A CommentModel queryable
        /// </returns>
        public IQueryable<CommentModel> CommentModelQuery(IQueryable<Comment> commentQuery, int? userId)
        {
            int authId = userId == null ? 0 : (int)userId;
            return commentQuery
                .Select(com => new
                {
                    com,
                    children = context.Comments.Where(c => c.Commented_UnderId == com.Id)
                })
                .Select(com => new CommentModel
                {
                    Body = com.com.Body,
                    Commented_At = com.com.Commented_At,
                    PostTitle = com.com.Commented_On.Title,
                    Id = com.com.Id,
                    Parent_Id = com.com.Commented_UnderId,
                    Post_Id = com.com.Commented_OnId,
                    IsHidden = com.com.IsHidden,
                    Commented_By = com.com.Commented_By.Username,
                    Rating = com.com.Ratings.Where(r => r.User_Id == authId).Select(r => new RatingModel { 
                        Id = r.Id,
                        Comment_Id = com.com.Id,
                        isUpvote = r.IsUpvoted
                    }).FirstOrDefault(),
                    TotalComments = com.children.Count()
                });
        }

        /// <summary>
        /// Creates a comment from a comment model
        /// </summary>
        /// <param name="comment">The comment to create</param>
        /// <returns>
        /// The newly created comment model
        /// </returns>
        public CommentModel CreateComment(AuthUserModel authUser, CommentModel comment)
        {
            // TODO: Verify information
            Comment commentDB = new Comment
            {
                Body = comment.Body,
                Commented_ById = authUser.Id,
                Commented_At = DateTime.Now,
                Commented_UnderId = comment.Parent_Id,
                Commented_OnId = comment.Post_Id,
            };

            var postInformation = context.Posts
                .Where(p => p.Id == comment.Post_Id)
                .Select(p => new { p.Title, p.Posted_ById })
                .FirstOrDefault();

            context.Comments.Add(commentDB);
            context.SaveChanges();

            CommentModel commentModel =  this.CommentModelQuery(context.Comments.Where(com => com.Id == commentDB.Id), null)
                .FirstOrDefault();

            // If this was a post level comment, notify the poster
            if (commentDB.Commented_UnderId == null && postInformation.Posted_ById != authUser.Id)
            {
                messageService.CreateMessage(new AuthMessageModel
                {
                    Comment = commentModel,
                    Title = "post reply"
                }, postInformation.Posted_ById); 
            }

            // If this is under a comment and the current commentor is not the user notify the user
            if (commentDB.Commented_UnderId != null)
            {
                int? commentorId = context.Comments
                    .Where(c => c.Id == commentDB.Commented_UnderId)
                    .Select(c => c.Commented_ById)
                    .FirstOrDefault();
                if (commentorId.HasValue && commentorId != authUser.Id)
                {
                    messageService.CreateMessage(new AuthMessageModel
                    {
                        Comment = commentModel,
                        Title = "comment reply"
                    }, commentorId.Value); 
                }
            }

            return commentModel;
        }

        /// <summary>
        /// Gets a list of comments model from a given queryable
        /// </summary>
        /// <param name="commentQuery">The queryable to convert to models</param>
        /// <param name="userId">The authorized users id if there is one</param>
        /// <returns>
        /// A list of comment models
        /// </returns>
        public List<CommentModel> GetCommentModels(IQueryable<Comment> commentQuery, int? userId)
        {
            return CommentModelQuery(commentQuery, userId)
                .ToList();
        }

        /// <summary>
        /// Gets a list of comments under a given post
        /// </summary>
        /// <param name="postId">The post id</param>
        /// <param name="skip">How many comments to skip</param>
        /// <param name="take">How many comments to take</param>
        /// <param name="userId">The authorized users id if there is one</param>
        /// <returns>
        /// A list of comment models for a given post
        /// </returns>
        public List<CommentModel> GetPostComments(int postId, int skip, int take, int? userId)
        {
            return this.CommentModelQuery(context.Comments.Where(com => com.Commented_OnId == postId && com.Commented_UnderId == null), userId)
                .Skip(skip)
                .Take(take)
                .ToList();
        }

        /// <summary>
        /// Gets a list of comments under a given comment
        /// </summary>
        /// <param name="commentId">The comment id</param>
        /// <param name="skip">How many comments to skip</param>
        /// <param name="take">How many comments to take</param>
        /// <param name="userId">The authorized users id if there is one</param>
        /// <returns>
        /// A list of comment models for a given comment
        /// </returns>
        public List<CommentModel> GetSubComments(int commentId, int skip, int take, int? userId)
        {
            return this.CommentModelQuery(context.Comments.Where(com => com.Commented_UnderId == commentId), userId)
                .OrderBy(com => com.Id)
                .Skip(skip)
                .Take(take)
                .ToList();
        }

        /// <summary>
        /// Hides a comment if the user is a moderator
        /// </summary>
        /// <param name="authUser">The user making the request</param>
        /// <param name="commentId">The comment that is being hidden</param>
        /// <param name="hideComment">If we are hiding the comment</param>
        /// <returns>
        /// A updated comment model
        /// </returns>
        public CommentModel HideComment(AuthUserModel authUser, int commentId, bool hideComment)
        {
            // Validate user is moderator
            bool isModerator = context.Users
                .Where(u => u.Id == authUser.Id)
                .Select(u => u.IsModerator)
                .FirstOrDefault();

            if (!isModerator)
            {
                throw new Exception("Not a moderator");
            }

            IQueryable<Comment> commentQuery = context.Comments
                .Where(c => c.Id == commentId);

            Comment comment = commentQuery
                .FirstOrDefault();

            comment.IsHidden = hideComment;
            context.SaveChanges();

            return this.CommentModelQuery(commentQuery, authUser.Id)
                .FirstOrDefault();
        }
    }
}
