using System;
using System.IO;
using System.Drawing;
using System.Linq;
using static ConsoleUtilitiesLite.ConsoleUtilitiesLite;
using System.Text.RegularExpressions;

namespace SpriteSheet_Generator
{
    class Program
    {
        static string MainPath;
        static readonly string[] Title = new string[]
        {
            "█▀ █▀█ █▀█ █ ▀█▀ █▀▀   █▀ █░█ █▀▀ █▀▀ ▀█▀   █▀▀ █▀▀ █▄░█ █▀▀ █▀█ ▄▀█ ▀█▀ █▀█ █▀█",
            "▄█ █▀▀ █▀▄ █ ░█░ ██▄   ▄█ █▀█ ██▄ ██▄ ░█░   █▄█ ██▄ █░▀█ ██▄ █▀▄ █▀█ ░█░ █▄█ █▀▄"
        };
        static readonly string[] AllowedExtensions = new string[] { "jpg", "png" };
        static readonly Regex RegexFormat = new Regex(@"(\D*)(\d+)(\.png|\.jpg)$");

        static void Main()
        {
            Console.Clear();
            ShowTitle(Title);
            while (true)
            {
                Console.Write("Please input the path to the folder with all the images: ");
                MainPath = Console.ReadLine().Trim();

                if (MainPath.ToLower().Equals("q"))
                    break;
                if (!Directory.Exists(MainPath))
                {
                    ErrorMessage("ERROR! Input a valid path.");
                    continue;
                }

                try
                {
                    MergeImages();
                }
                catch (Exception ex)
                {
                    ErrorMessage(ex.Message);
                }
                Division();
            }
        }

        private static void MergeImages()
        {
            string[] imagesPaths = Directory.GetFiles(MainPath).Where(p => AllowedExtensions.Contains(Path.GetExtension(p).TrimStart('.').ToLowerInvariant())).OrderBy(f => f).ToArray();

            if (imagesPaths.Length == 0)
                throw new FileNotFoundException("No valid images in the provided folder! (formats are: jpg or png)");

            (int maxWidth, int maxHeihgt) maxSize = (0, 0);
            foreach (var imagePath in imagesPaths)
            {
                Image image = Image.FromFile(imagePath);

                if (image.Size.Width > maxSize.maxWidth)
                    maxSize.maxWidth = image.Size.Width;

                if (image.Size.Height > maxSize.maxHeihgt)
                    maxSize.maxHeihgt = image.Size.Height;

                image.Dispose();
            }

            var numberOfColumns = GetNumberOfColumns(imagesPaths.Length);
            int numberOfRows = GetNumberOfRows(imagesPaths.Length);

            Point location = Point.Empty;
            using Bitmap bitmap = new Bitmap(numberOfColumns * maxSize.maxWidth, numberOfRows * maxSize.maxHeihgt);
            using Graphics g = Graphics.FromImage(bitmap);
            int currentColumnCount = 0;
            int previousLoggedStringLength = 0;
            string message;

            for (int i = 0; i < imagesPaths.Length; i++)
            {
                ClearPreviousLog(previousLoggedStringLength);
                using Image image = Image.FromFile(imagesPaths[i]);
                g.DrawImage(image, location);
                currentColumnCount++;

                message = $"Merging: {imagesPaths[0]}";
                Log(message);
                previousLoggedStringLength = message.Length;

                if (currentColumnCount == numberOfColumns)
                {
                    location.Y += maxSize.maxHeihgt;
                    location.X = 0;
                    currentColumnCount = 0;
                }
                else
                    location.X += maxSize.maxWidth;
            }

            if (!RegexFormat.IsMatch(imagesPaths[0]))
                throw new FormatException("The images were not in the correct naming format, the correct format is nameXXX*.jpg or .png. Name must not have numbers in it.");

            var match = RegexFormat.Match(imagesPaths[0]);
            string spriteName = match.Groups[1].Value;
            string fileNumber = match.Groups[2].Value;
            string fileExtension = match.Groups[3].Value;

            string spritePath = imagesPaths[0].Remove(imagesPaths[0].Length - (fileNumber.Length + fileExtension.Length));
            spritePath = Path.Combine(spritePath, spriteName) + fileExtension;
            bitmap.Save(spritePath);

            SuccessMessage($"DONE! Final sprite is in: {spritePath}");
        }

        private static int GetNumberOfColumns(int numberOfElements)
        {
            float squareRoot = MathF.Sqrt(numberOfElements);
            bool isPerfectSquare = squareRoot % 1 == 0;
            if (isPerfectSquare)
                return (int)MathF.Sqrt(numberOfElements);

            return (int)Math.Round(squareRoot, 0);
        }

        private static int GetNumberOfRows(int numberOfElements)
        {

            float squareRoot = MathF.Sqrt(numberOfElements);
            bool isPerfectSquare = squareRoot % 1 == 0;
            if (isPerfectSquare)
                return (int)MathF.Sqrt(numberOfElements);

            return (int)Math.Round(squareRoot, 0) + 1;
        }
    }
}
