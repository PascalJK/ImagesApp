using SkiaSharp;

namespace ImagesApp.ViewModels;

public class HomeViewModel : BaseViewModel
{
    public ObservableRangeCollection<ImageData> ImagesCollection { get; set; } = new();

    #region ICommand
    public ICommand RemoveImageCommand => new Command<ImageData>((img) => ImagesCollection.Remove(img));
    public ICommand RemoveAllImagesCommand => new AsyncCommand(RemoveAllImageAsync);
    public ICommand SelectImagesCommand => new AsyncCommand(SelectImagesAsync);
    #endregion

    public HomeViewModel()
    {
        Title = "Home Page";
    }

    private async Task SelectImagesAsync()
    {
        try
        {
            var customFileType = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
            {
               { DevicePlatform.iOS, new[] { "UTType.Image" } },
               { DevicePlatform.Android, new[] { "image/*", } },
            });

            var result = await FilePicker.PickMultipleAsync(new PickOptions
            {
                FileTypes = customFileType,
            });

            if (result != null)
            {
                if (result.Count() + ImagesCollection.Count > 50)
                {
                    await Shell.Current.DisplayAlert("Image limit reached", "You cannot upload more then 50 images. To upload more images use the browser.", "OK");
                    return;
                }

                foreach (var item in result)
                {
                    // Compress Image before uploading.
                    var imageResult = await CheckImageSize(item);
                    var image = new ImageData()
                    {
                        ImageUrl = imageResult,
                        ImageFormat = item.ContentType
                    };
                    ImagesCollection.Add(image);
                }
            }
        }
        catch (Exception ex)
        {
            // The user canceled or something went wrong
        }
    }

    public async Task<string> CheckImageSize(FileResult file)
    {
        var xfile = await file.OpenReadAsync();

        return xfile.Length >= 2000000 ? CreateThumbnail(file, SKFilterQuality.None) : file.FullPath;
    }

    public string CreateThumbnail(FileResult file, SKFilterQuality filterQuality)
    {
        var bitmap = SKBitmap.Decode(file.FullPath);
        int h = bitmap.Height;
        int w = bitmap.Width;
        int newWidth = w;
        int newHeight = h;

        //resize algorythm
        if (h > 1080 || w > 1080)
        {
            int rectHeight = 1080;
            int rectWidth = 1080;

            //aspect ratio calculation
            float W = w;
            float H = h;
            float aspect = W / H;

            //new dimensions by aspect ratio
            newWidth = (int)(rectWidth * aspect);
            newHeight = (int)(newWidth / aspect);

            //if one of the two dimensions exceed the box dimensions
            if (newWidth > rectWidth || newHeight > rectHeight)
            {
                //depending on which of the two exceeds the box dimensions set it as the box dimension and calculate the other one based on the aspect ratio
                if (newWidth > newHeight)
                {
                    newWidth = rectWidth;
                    newHeight = (int)(newWidth / aspect);
                }
                else
                {
                    newHeight = rectHeight;
                    newWidth = (int)(newHeight * aspect);
                }
            }
        }

        SKBitmap resizedImage = bitmap.Resize(new SKImageInfo(newWidth, newHeight), filterQuality);
        SKData image = resizedImage.Encode(SKEncodedImageFormat.Jpeg, 90);
        var path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var filepath = Path.Combine(path, file.FileName);
        string finalPath = filepath;
        using (FileStream stream = File.OpenWrite(filepath))
            image.SaveTo(stream);
        return finalPath;
    }

    private async Task RemoveAllImageAsync()
    {
        if(ImagesCollection.Count <= 0)
        {
            await Shell.Current.DisplayAlert("", "No images selected.", "OK");
            return;
        }
        var ans = await Shell.Current.DisplayAlert("", "Remove all images?", "Yes", "Cancel");
        if (ans)
            ImagesCollection.Clear();
    }
}
