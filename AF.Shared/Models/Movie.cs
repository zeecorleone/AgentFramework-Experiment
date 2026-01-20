using System;
using System.Collections.Generic;
using System.Text;

namespace AF.Shared.Models;

public class Movie
{
    public required string Title { get; set; }
    public int YearOfRelease { get; set; }
    public required string Director { get; set; }
    public MovieGenre Genre { get; set; }
    public decimal ImdbScore { get; set; }
}

public enum MovieGenre
{
    ScienceFiction,
    Drama,
    Comedy,
    Horror,
    LoveStory,
    Other
}

public class MovieResult
{
    public required string MessageBack { get; set; }
    public required Movie[] Top10Movies { get; set; }
}