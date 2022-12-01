using Scorpian.Asset;

if (args.Length != 2)
{
    Console.WriteLine("Missing content directory. Needs to be in this format: $ packer /input/folders /output/folders");
    return;
}

var packs = Directory.EnumerateDirectories(args[0]);
var outputFolder = args[1];

foreach (var pack in packs)
{
    var packName = Path.GetFileName(pack);
    var outputFile = Path.Combine(outputFolder, $"{packName}.pack");
    
    AssetManager.Pack(pack, outputFile);
}