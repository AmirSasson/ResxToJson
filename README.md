# ResxToJson
Converting Resx to hierarchical json file
```posh
Usage:
  ResxToJson [options]

Options:
  --resx-file <resx-file>    The path of resx file
  --out-folder <out-folder>  The output file of json file
  -v, --verbose <verbose>    verbosity level 'v' or 'vv' or 'vvv'
  --out-file <out-file>      optional out file name
  --version                  Show version information
  -?, -h, --help             Show help and usage information
```

# example:  
```xml
<root>
  <data name="Test2_Amir_amir1" xml:space="preserve">
      <value>test3</value>
      <comment>test comment3</comment>
    </data>
    <data name="Test_Amir_amir1" xml:space="preserve">
      <value>test</value>
      <comment>test comment</comment>
    </data>
    <data name="Test_Amir_amir2" xml:space="preserve">
      <value>test2</value>
      <comment>test comment2</comment>
    </data>
 </root>
```
into a hierarchical json file:
```json
{
  "Test2": {
    "Amir": {
      "amir1": "test3",
      "_key_amir1": "test comment3"
    }
  },
  "Test": {
    "Amir": {
      "amir1": "test",
      "_key_amir1": "test comment",
      "amir2": "test2",
      "_key_amir2": "test comment2"
    }
  }
}
```
