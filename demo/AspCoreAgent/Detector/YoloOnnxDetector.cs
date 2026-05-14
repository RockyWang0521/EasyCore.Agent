namespace AspCoreAgent
{
    using Microsoft.ML.OnnxRuntime;
    using Microsoft.ML.OnnxRuntime.Tensors;
    using OpenCvSharp;

    public sealed class YoloOnnxDetector : IDisposable
    {
        private readonly InferenceSession _session;

        private const int InputWidth = 640;
        private const int InputHeight = 640;

        private readonly string[] _classNames =
        {
        "crack",
        "scratch",
        "hole"
    };

        public YoloOnnxDetector(string modelPath)
        {
            _session = new InferenceSession(modelPath);
        }

        public List<YoloDetection> DetectAndDraw(
            string imagePath,
            string outputPath,
            float confThreshold = 0.35f,
            float iouThreshold = 0.45f)
        {
            using var image = Cv2.ImRead(imagePath);

            if (image.Empty())
                throw new Exception("图片读取失败");

            var input = Preprocess(image, out var scale, out var padX, out var padY);

            var inputName = _session.InputMetadata.Keys.First();

            using var results = _session.Run(new[]
            {
            NamedOnnxValue.CreateFromTensor(inputName, input)
        });

            var output = results.First().AsTensor<float>();

            var detections = ParseOutput(
                output,
                image.Width,
                image.Height,
                scale,
                padX,
                padY,
                confThreshold);

            detections = Nms(detections, iouThreshold);

            foreach (var item in detections)
            {
                Cv2.Rectangle(
                    image,
                    new Point(item.X1, item.Y1),
                    new Point(item.X2, item.Y2),
                    Scalar.Red,
                    2);

                var text = $"{item.ClassName} {item.Confidence:0.00}";

                Cv2.PutText(
                    image,
                    text,
                    new Point(item.X1, Math.Max(item.Y1 - 6, 20)),
                    HersheyFonts.HersheySimplex,
                    0.6,
                    Scalar.Red,
                    2);
            }

            Cv2.ImWrite(outputPath, image);

            return detections;
        }

        private DenseTensor<float> Preprocess(
            Mat image,
            out float scale,
            out int padX,
            out int padY)
        {
            var originalWidth = image.Width;
            var originalHeight = image.Height;

            scale = Math.Min(
                (float)InputWidth / originalWidth,
                (float)InputHeight / originalHeight);

            var resizeWidth = (int)(originalWidth * scale);
            var resizeHeight = (int)(originalHeight * scale);

            padX = (InputWidth - resizeWidth) / 2;
            padY = (InputHeight - resizeHeight) / 2;

            using var resized = new Mat();
            Cv2.Resize(image, resized, new Size(resizeWidth, resizeHeight));

            using var canvas = new Mat(
                new Size(InputWidth, InputHeight),
                MatType.CV_8UC3,
                new Scalar(114, 114, 114));

            var roi = new Rect(padX, padY, resizeWidth, resizeHeight);
            resized.CopyTo(new Mat(canvas, roi));

            Cv2.CvtColor(canvas, canvas, ColorConversionCodes.BGR2RGB);

            var tensor = new DenseTensor<float>(new[] { 1, 3, InputHeight, InputWidth });

            for (var y = 0; y < InputHeight; y++)
            {
                for (var x = 0; x < InputWidth; x++)
                {
                    var pixel = canvas.At<Vec3b>(y, x);

                    tensor[0, 0, y, x] = pixel.Item0 / 255.0f;
                    tensor[0, 1, y, x] = pixel.Item1 / 255.0f;
                    tensor[0, 2, y, x] = pixel.Item2 / 255.0f;
                }
            }

            return tensor;
        }

        private List<YoloDetection> ParseOutput(
            Tensor<float> output,
            int originalWidth,
            int originalHeight,
            float scale,
            int padX,
            int padY,
            float confThreshold)
        {
            var detections = new List<YoloDetection>();

            var dims = output.Dimensions.ToArray();

            var channels = dims[1];
            var boxCount = dims[2];

            var classCount = channels - 4;

            for (var i = 0; i < boxCount; i++)
            {
                var cx = output[0, 0, i];
                var cy = output[0, 1, i];
                var w = output[0, 2, i];
                var h = output[0, 3, i];

                var maxScore = 0f;
                var classId = 0;

                for (var c = 0; c < classCount; c++)
                {
                    var score = output[0, 4 + c, i];

                    if (score > maxScore)
                    {
                        maxScore = score;
                        classId = c;
                    }
                }

                if (maxScore < confThreshold)
                    continue;

                var x1 = (cx - w / 2 - padX) / scale;
                var y1 = (cy - h / 2 - padY) / scale;
                var x2 = (cx + w / 2 - padX) / scale;
                var y2 = (cy + h / 2 - padY) / scale;

                x1 = Math.Clamp(x1, 0, originalWidth - 1);
                y1 = Math.Clamp(y1, 0, originalHeight - 1);
                x2 = Math.Clamp(x2, 0, originalWidth - 1);
                y2 = Math.Clamp(y2, 0, originalHeight - 1);

                detections.Add(new YoloDetection
                {
                    ClassId = classId,
                    ClassName = classId < _classNames.Length
                        ? _classNames[classId]
                        : $"class_{classId}",
                    Confidence = maxScore,
                    X1 = (int)x1,
                    Y1 = (int)y1,
                    X2 = (int)x2,
                    Y2 = (int)y2
                });
            }

            return detections;
        }

        private static List<YoloDetection> Nms(
            List<YoloDetection> detections,
            float iouThreshold)
        {
            var result = new List<YoloDetection>();

            var sorted = detections
                .OrderByDescending(x => x.Confidence)
                .ToList();

            while (sorted.Count > 0)
            {
                var current = sorted[0];
                result.Add(current);
                sorted.RemoveAt(0);

                sorted = sorted
                    .Where(x => CalculateIou(current, x) < iouThreshold)
                    .ToList();
            }

            return result;
        }

        private static float CalculateIou(YoloDetection a, YoloDetection b)
        {
            var x1 = Math.Max(a.X1, b.X1);
            var y1 = Math.Max(a.Y1, b.Y1);
            var x2 = Math.Min(a.X2, b.X2);
            var y2 = Math.Min(a.Y2, b.Y2);

            var width = Math.Max(0, x2 - x1);
            var height = Math.Max(0, y2 - y1);

            var intersection = width * height;

            var areaA = Math.Max(0, a.X2 - a.X1) * Math.Max(0, a.Y2 - a.Y1);
            var areaB = Math.Max(0, b.X2 - b.X1) * Math.Max(0, b.Y2 - b.Y1);

            var union = areaA + areaB - intersection;

            if (union <= 0)
                return 0;

            return (float)intersection / union;
        }

        public void Dispose()
        {
            _session.Dispose();
        }
    }
}
