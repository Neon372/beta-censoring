using BetaCensor.Web.Providers;
using CensorCore;
using Microsoft.Extensions.FileProviders;

namespace BetaCensor.Web;
#pragma warning disable 1998
public class StickerProvider : CensorCore.IAssetStore {
    private readonly StickerOptions? _options;
    private readonly List<string> _captions;
    private readonly List<CategoryProvider> _categoryProviders;
    private readonly CompositeFileProvider _provider;

    public StickerProvider(StickerOptions? options, CaptionOptions? captions, params IFileProvider[] providers) : this(options, captions, providers.AsEnumerable()) {
    }
    public StickerProvider(StickerOptions? options, CaptionOptions? captions, IEnumerable<IFileProvider> providers) {
        _options = options;
        _captions = LoadCaptions(captions) ?? _defaultCaptions;
        _categoryProviders = providers.Where(p => p is CategoryProvider).Cast<CategoryProvider>().ToList();
        _provider = new CompositeFileProvider(providers);
    }

    private List<string>? LoadCaptions(CaptionOptions? options) {
        if (options == null) return null;
        try {
            var list = options.Captions;
            if (options.FilePaths.Any()) {
                list.AddRange(options.FilePaths.Where(f => File.Exists(f) && Path.GetExtension(f) == ".txt").SelectMany(f => File.ReadAllLines(f)));
            }
            return list;
        } catch {
            return null;
        }
    }

    public Task<string?> GetRandomCaption(string? category) {
        return Task.FromResult<string?>(_captions.Random());
    }

    public Task<RawImageData?> GetRandomImage(string imageType, float? ratio, List<string>? category) {
        throw new NotImplementedException();
    }

    public async Task<IEnumerable<RawImageData>> GetImages(string imageType, List<string>? category) {
        if (imageType == CensorCore.Censoring.KnownAssetTypes.Stickers && category is not null && category.Any()) {
            var results = new List<IFileInfo?>();
            foreach (var item in category) {
                var catResults = _provider.GetDirectoryContents(item).ToList();
                var nestedFiles = new List<IFileInfo>();
                foreach (var dir in catResults.Where(s => s.IsDirectory && s.Exists))
                {
                    nestedFiles.AddRange(FileProviderExtensions.DirSearch(_provider, dir));
                }
                var nest = nestedFiles.ToList();
                results.AddRange(catResults.Where(f => !f.IsDirectory && f.Exists).Concat(nestedFiles));
            }
            var candidates = results.Where(fi => fi is not null && fi.Exists);
            return candidates.Select(c =>new RawImageData(ReadFile(c!)));
        }
        else {
            return Array.Empty<RawImageData>();
        }
    }

    private byte[] ReadFile(IFileInfo fi) {
        using var readStream = fi.CreateReadStream();
        var ms = new MemoryStream();
        readStream.CopyTo(ms);
        return ms.ToArray();
    }

    private bool CloseEnough(float stickerRatio, float targetRatio) {
        var margin = _options?.RatioMargin ?? 25F;
        var lowerBound = 1 - (margin / 100);
        var upperBound = 1 + (margin / 100);
        var diff = stickerRatio / targetRatio;
        return lowerBound <= diff && diff <= upperBound;
    }

    public async Task<IEnumerable<string>> GetCategories() {
        var providedCats = _categoryProviders.Select(p => p.Category).ToList();
        var availableCats = _provider.GetDirectoryContents(string.Empty).Where(f => f.IsDirectory && f.Exists).Select(f => f.Name).ToList();
        return providedCats.Concat(availableCats).Distinct();
    }

    private static List<string> _defaultCaptions = new() {
        "beta",
        "cuck",
        "edge",
        "leak",
        "denied",
        "pathetic",
        "sissy",
        "stupid",
        "censored",
        "censored",
        "censored",
        "suck cock",
        "bitch",
        "locked",
        "stay locked",
        "strain",
        "no betas",
        "alphas only",
        "for alphas"
    };
}
#pragma warning restore 1998
