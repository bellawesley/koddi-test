﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace KoddiTestApp.Models
{
    [MetadataType(typeof(TweetMetadata))]

    // Partial class of Tweet model. Here, data annotation validators validate the model by limiting character count per tweet.
    public partial class Tweet
    {
    }
    public class TweetMetadata
    {
        [Display(Name = "Tweet")]
        [DataType(DataType.Text)]
        [Required(ErrorMessage = "*")]
        [MaxLength(140, ErrorMessage = "Please limit tweet to 140 characters")]
        public string Note { get; set; }
    }
}