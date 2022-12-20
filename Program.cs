using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Net;
using System.Threading.Tasks;
using System.Text;
using System.Threading;
using System.Diagnostics;
using Newtonsoft.Json;

namespace recipeexport
{
    class Program
    {
        private static string inPath = "Recipes";
        private static string outPath = "out";
        public static void Main(string[] args)
        {
            var cwd = Directory.GetCurrentDirectory();
            inPath = Path.Combine(cwd, inPath);
            outPath = Path.Combine(cwd, outPath);

            if (Directory.Exists(outPath))
            {
                Directory.Delete(outPath, true); //"directory not empty" is the dumbest error ever
            }
            Directory.CreateDirectory(outPath);
            Directory.CreateDirectory(Path.Combine(outPath, "images"));
            Directory.CreateDirectory(Path.Combine(outPath, "text"));

            foreach (var subdir in Directory.GetDirectories(inPath))
            {
                Console.WriteLine(subdir);
                var recipeText = Path.Combine(subdir, "recipe.json");
                if (File.Exists(recipeText))
                {
                    string Authors = "";
                    Recipe recipeObj = null;
                    try
                    {
                        var recipe1Auth = JsonConvert.DeserializeObject<Recipe1Author>(File.ReadAllText(recipeText));
                        recipeObj = recipe1Auth;
                        Authors = recipe1Auth.author?.name ?? "";
                    }
                    catch (Exception se)
                    {
                        var recipeMultiAuth = JsonConvert.DeserializeObject<RecipeMultiAuthor>(File.ReadAllText(recipeText));
                        recipeObj = recipeMultiAuth;
                        if (recipeMultiAuth.author?.Length > 0)
                        {
                            Authors = string.Join(", ", recipeMultiAuth.author.Select(a => a.name));
                        }
                    }
                    if(recipeObj == null)
                    {
                        throw new Exception("recipe obj null?");
                    }

                    var recipeName = subdir.Substring((Path.GetDirectoryName(subdir)?.Length ?? 0) + 1);
                    if (!string.IsNullOrWhiteSpace(Authors))
                    {
                        Console.WriteLine($"{recipeName} by {Authors}");
                    }
                    else
                    {
                        Console.WriteLine($"{recipeName}, author unknown");
                    }

                    var imageFileName = recipeName + ".jpg";
                    foreach (var invalidChar in Path.GetInvalidFileNameChars())
                    {
                        imageFileName = imageFileName.Replace(invalidChar, '_');
                    }

                    var recipeTextFilename = imageFileName.Substring(0, imageFileName.Length - 4) + ".md";
                    var image = Path.Combine(subdir, "full.jpg");
                    if (File.Exists(image))
                    {
                        Console.WriteLine("image exists");
                        File.Copy(image, Path.Combine(outPath, "images", imageFileName));
                    }
                    else
                    {
                        Console.WriteLine("no image for this recipe");
                    }


#region recipe.json -> recipe.md
                    var sb = new StringBuilder();
                    sb.AppendLine("---");
                    sb.AppendLine();
                    sb.AppendLine("layout: recipe");
                    sb.AppendLine($"title: \"{recipeName}\"");
                    if (File.Exists(image))
                    {
                        sb.AppendLine($"image: {imageFileName}");
                    }
                    sb.AppendLine($"tags: {recipeObj.recipeCategory}");
                    sb.AppendLine();
                    sb.AppendLine();

                    sb.AppendLine("ingredients:");
                    foreach(var ing in recipeObj.recipeIngredient)
                    {
                        sb.AppendLine($"- {ing}");
                    }
                    sb.AppendLine();

                    sb.AppendLine("directions: ");
                    if(recipeObj.recipeInstructions?.Length > 0)
                    {
                        foreach(var inst in recipeObj.recipeInstructions)
                        {
                            sb.AppendLine("- " + inst);
                        }
                    }
                    else
                    {
                        sb.AppendLine("- combine");
                    }
                    
                    sb.AppendLine();

                    sb.AppendLine("---");
                    sb.AppendLine();
                    if(!string.IsNullOrWhiteSpace(recipeObj.description))
                    {
                        sb.AppendLine(recipeObj.description);
                    }
                    if(!string.IsNullOrWhiteSpace(Authors))
                    {
                        sb.AppendLine($"author: {Authors}");
                    }
                    if(recipeObj.url != null)
                    {
                        sb.AppendLine($"source: {recipeObj.url}");
                    }
                    File.WriteAllText(Path.Combine(outPath, "text", recipeTextFilename), sb.ToString());
#endregion
                }
                else
                {
                    Console.WriteLine("no recipe text for " + subdir + "?");
                }
            }
            Console.WriteLine("done :)");
        }
    }
}