# JSONTransformer

The goal of this project is to create an engine that translates a JSON input file based on a seperate transformation file to give a new structure.

The reason for doing this is to easily allow the passing of data between systems without the need to build a custom intergration. The initial needs were layed down to generate xAPI statements and more functionality will be added as required.

## Quick Example

<table width="100%">
  <tr>
    <td width="33.33%" align="left" valign="top">
      <div><b>Source</b></div>
      <div>
        {<br/>
          &nbsp;"question": "How are you doing?",<br/>
          &nbsp;"answers": [<br/>
          &nbsp;&nbsp;"I'm doing pretty well",<br/>
          &nbsp;&nbsp;"I wish I was better"<br/>
          &nbsp;]<br/>
        }
      </div>
    </td>
    <td width="33.33%" align="left" valign="top">
      <div><b>Transformation</b></div>
      <div>
      {<br/>
        &nbsp;"responseType": "survey",<br/>
        &nbsp;"{{ each(answer IN answers) AS answers }}": {<br/>
        &nbsp;&nbsp;"text": "{{ question }}",<br/>
        &nbsp;&nbsp;"answer": "{{ answer }}"<br/>
        &nbsp;}<br/>
      }
      </div>
    </td>
    <td width="33.33%" align="left" valign="top">
      <div><b>Result</b></div>
      <div>
      {<br/>
        &nbsp;"responseType": "survey",<br/>
        &nbsp;"answers": [{<br/>
        &nbsp;&nbsp;"text": "How are you doing?",<br/>
        &nbsp;&nbsp;"answer": "I'm doing pretty well"<br/>
        &nbsp;}, {<br/>
        &nbsp;&nbsp;"text": "How are you doing?",<br/>
        &nbsp;&nbsp;"answer": "I wish I was better"<br/>
        &nbsp;}]<br/>
      }
      </div>
    </td>
  </tr>
</table>

## Getting started

The main function requires 2 parameters, the source you want to translate from and the transformation you want to apply. Both of these are JTokens and therefore can be a JObject or JArray. If the source is a JObject the transformation will only be applied to that object. If the source is a JArray the transformation will be applied to each child object and then appended together for a returning JArray. 

## Syntax for transformations

### Replacements

The basic replacement is marked using {{ .. }} within the JSON property or value. Within that a simple dot notation can be used, e.g. {{ this.is.a.path.to.child }}

Arrays within the path can be navigated using the usual 0 based bracket notation, e.g. {{ path.to.array[0].child }}

### If Statements

If statements can be included in a transformation be setting them in the a property. This will then evaluate whether to include the child. e.g.

```json
{
  "file": "transformation",
  "{{ if (child.in.source == 'this value' }}": {
    "this": "here",
    "only": "if the parent condition is true"
  }
}
```

would translate to the following if the if evaluated true:

```json
{
  "file": "transformation",
  "this": "here",
  "only": "if the parent condition is true"
}
```
Using the "AS" (case sensitive) keyword you can also create a parent object for the if statement. e.g.

```json
{
  "file": "transformation",
  "{{ if (child.in.source == 'this value' }} AS parent": {
    "this": "here",
    "only": "if the parent condition is true"
  }
}
```

would translate to the following if the if evaluated true:

```json
{
  "file": "transformation",
  "parent": {
    "this": "here",
    "only": "if the parent condition is true"
  }
}
```
