// See https://aka.ms/new-console-template for more information
using Microsoft.Extensions.Logging;
using System.CommandLine;
using System.Linq;
using System.Net.WebSockets;
using System.Text.Json;
using System.Xml;

var fileOption = new Option<string?>(
          name: "--resx-file",
          description: "The path of resx file"
          );

var jsonOutputFolderOption = new Option<string?>(
          name: "--out-folder",
          description: "The output file of json file"
          );

var verbosityOption = new Option<string?>(
          name: "--verbose",
          description: "verbosity level 'v' or 'vv' or 'vvv'"
          );
verbosityOption.AddAlias("-v");

var outFileName = new Option<string?>(
          name: "--out-file",
          description: "optional out file name"
          );

var rootCommand = new RootCommand("Converting Resx to hierarchical json file");
rootCommand.AddOption(fileOption);
rootCommand.AddOption(jsonOutputFolderOption);
rootCommand.AddOption(verbosityOption);
rootCommand.AddOption(outFileName);



rootCommand.SetHandler((resx, output, verbosityOption, outFileName) =>
{
    var loggerFactory = LoggerFactory.Create((builder) =>
    {
        builder.SetMinimumLevel(verbosityOption?.Length == 1 ? LogLevel.Error : verbosityOption?.Length == 2 ? LogLevel.Information : LogLevel.Debug);
        builder.AddConsole();
    });

    var logger = loggerFactory.CreateLogger("Converter");
    logger.LogInformation($"Start converting resx file '${resx}' to folder: {output}");

    using var stream = new FileStream(resx, FileMode.Open);
    var resxXmlDoc = new XmlDocument();
    resxXmlDoc.Load(stream);

    Dictionary<string, object> outputObject = new();

    var dataNodes = resxXmlDoc.SelectNodes("//data")?.Cast<XmlNode>();
    logger.LogInformation($"found {dataNodes?.Count()} data nodes");

    int handledCount = 0;
    foreach (var dataNode in dataNodes!)
    {
        var text = dataNode.SelectSingleNode("value")?.InnerText;
        var comment = dataNode.SelectSingleNode("comment")?.InnerText;
        var name = dataNode.Attributes?["name"]?.Value;
        logger.LogDebug($"converting data '{name}' .. ");

        var nameTokens = name!.Split("_");

        var ancesstors = nameTokens.Take(nameTokens.Length - 1).ToList();
        // add Update Dictionary 
        var parent = outputObject;
        foreach (var nameToken in ancesstors)
        {
            // find parent
            if (!parent.TryGetValue(nameToken, out object parenValue))
            {
                logger.LogDebug($"Creating Parent '${nameToken}' .. ");
                parenValue = new Dictionary<string, object>();
                parent[nameToken] = parenValue;
            }
            parent = parenValue as Dictionary<string, object>;
        }
        parent[nameTokens.Last()] = text;
        if (!string.IsNullOrWhiteSpace(comment!))
            parent[$"_key_{nameTokens.Last()}"] = comment;
        handledCount++;
    }

    var serialized = System.Text.Json.JsonSerializer.Serialize(outputObject, new JsonSerializerOptions { WriteIndented = true });
    logger.LogInformation($"serializing output object with {handledCount} text nodes");

    if (!Directory.Exists(output))
    {
        Directory.CreateDirectory(output);
    }
    var outfile = string.IsNullOrWhiteSpace(outFileName) ? $"{new FileInfo(resx).Name}.json" : outFileName;
    var outFileFullPath = Path.Combine(output, outfile);
    logger.LogInformation($"Saving File '{outFileFullPath}' ..");
    File.WriteAllText(outFileFullPath, serialized);

},
    fileOption, jsonOutputFolderOption, verbosityOption, outFileName);

return await rootCommand.InvokeAsync(args);