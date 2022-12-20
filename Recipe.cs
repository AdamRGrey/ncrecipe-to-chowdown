#pragma warning disable 8618

using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Net;
using System.Threading.Tasks;
using System.Text;
using System.Threading;
using System.Diagnostics;


namespace recipeexport
{
    public class Recipe
    {
        public string name { get; set; }
        public string description { get; set; }
        
        public string recipeCategory{get;set;}
        public string[] recipeIngredient {get;set;}
        public string[] recipeInstructions {get;set;}
        public Uri url{get;set;}
    }
    public class RecipeMultiAuthor : Recipe
    {
        public RecipeAuthor[] author { get; set; }
    }
    public class Recipe1Author : Recipe
    {
        public RecipeAuthor author { get; set; }
    }
    public class RecipeAuthor
    {
        public string @type { get; set; }
        public string name { get; set; }
    }
}