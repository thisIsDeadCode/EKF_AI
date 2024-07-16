using MLModel3_ConsoleApp1;
using System.Drawing;
using System.Drawing.Imaging;

namespace EKF_AI.Services
{
    public class ClassificationService
    {
        public ClassificationService()
        {

        }

        public List<string> GetElectricalElements(string imageId, string path, string rectanglesData, int minPrecision)
        {
            var subPath = path.Split('.').First();

            byte[] imageBytes = File.ReadAllBytes(path);

            if (!Directory.Exists(subPath))
            {
                Directory.CreateDirectory(subPath);
            }

            List<string> imgResults = new List<string>();

            using (MemoryStream ms = new MemoryStream(imageBytes))
            {
                using (Bitmap image = new Bitmap(ms))
                {
                    string[] rectangles = rectanglesData.Replace(".", ",").Split('\n');
                    int counter = 0;

                    foreach (string rect in rectangles)
                    {
                        var parts = rect.Split(' ').Select(float.Parse).ToArray();
                        float precision = parts[0];
                        float x = parts[1];
                        float y = parts[2];
                        float x1 = parts[3];
                        float x2 = parts[4];

                        int absX = (int)(x * image.Width);
                        int absY = (int)(y * image.Height);
                        int absX1 = (int)(x1 * image.Width);
                        int absY1 = (int)(x2 * image.Height);

                        Rectangle cropRect = Rectangle.FromLTRB(absX1, absY1, absX, absY);
                        Bitmap croppedImage = new Bitmap(cropRect.Width, cropRect.Height);

                        using (Graphics g = Graphics.FromImage(croppedImage))
                        {
                            g.DrawImage(image, new Rectangle(0, 0, croppedImage.Width, croppedImage.Height), cropRect, GraphicsUnit.Pixel);
                        }

                        byte[] bitmapBytes = BitmapToBytes(image, ImageFormat.Png);

                        MLModel3.ModelInput sampleData = new MLModel3.ModelInput()
                        {
                            ImageSource = bitmapBytes,
                        };

                        var sortedScoresWithLabel = MLModel3.PredictAllLabels(sampleData);
                        var predictedlabel = sortedScoresWithLabel.OrderByDescending(x => x.Value).FirstOrDefault();

                        if(predictedlabel.Value > minPrecision / 100)
                        {
                            precision = predictedlabel.Value;
                            imgResults.Add($"{predictedlabel.Key} {precision} {x} {y} {x1} {x2}");

                            string fileName = Path.Combine(subPath, $"cropped_{precision}_{counter}.png");
                            croppedImage.Save(fileName, ImageFormat.Png);
                        }

                        counter++;
                    }
                }
            }

            return imgResults;
        }

        private byte[] BitmapToBytes(Bitmap bitmap, ImageFormat format)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                bitmap.Save(ms, format);
                return ms.ToArray();
            }
        }
    }
}
