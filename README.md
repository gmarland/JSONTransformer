# JSONTransformer

The goal of this project is to create an engine that translates a JSON input file based on a seperate transformation file to give a new structure.

The reason for doing this is to easily allow the passing of data between systems without the need to build a custom intergration. The initial needs were layed down to generate xAPI statements and more functionality will be added as required.

## Quick Example

<b>Source</b>
```json
  {
    "question": "How are you doing?",
    "answers": [
      "I'm doing pretty well",
      "I wish I was better"
    ]
  }
```

<b>Transformation</b>
````json
  {
    "responseType": "survey",
    "{{ each(answer IN answers) AS answers }}": {
        "text": "{{ question }}",
        "answer": "{{ answer }}"
    }
  }
```

<b>Result</b>
````json
{
  "responseType": "survey",
  "answers": [{
    "text": "How are you doing?",
    "answer": "I'm doing pretty well"
  }, {
    "text": "How are you doing?",
    "answer": "I wish I was better"
  }]
}
```


## Getting started

The main function requires 2 parameters, the source you want to translate from and the transformation you want to apply. Both of these are JTokens and therefore can be a JObject or JArray. If the source is a JObject the transformation will only be applied to that object. If the source is a JArray the transformation will be applied to each child object and then appended together for a returning JArray. 


## Syntax for Transformations

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


#### Comparitors

If statements at present can evaluate with either strings or numbers. Bear in mind that strings must be enclosed using single quotes ('). The following comparisons may be used:

<b>Equals</b> - e.g. 
```"{{ if (child.object.property == 'this value') }}":```

<b>Not Equals</b> - e.g. 
```"{{ if (child.object.property != 'this value') }}":```

<b>Contains</b> - case sensitive keyword. e.g. 
```"{{ if (child.object.property CONTAINS 'this value') }}":```

<b>And Operator</b> - e.g.
```"{{ if (child.object.property == 'this value') && (other.object.property == 'this value') }}":```

<b>Or Operator</b> - e.g.
```"{{ if (child.object.property == 'this value') || (other.object.property == 'this value') }}":```


### Each Statements

The each statement allows the iteration over an object in the source document and apply a trasformation. Once the transformation has been performed it is appended to a node with the name specified after the "AS" keyword (for this reason an AS is required). e.g. As in the example, this source

```json
  {
    "question": "How are you doing?",
    "answers": [
    "I'm doing pretty well",
    "I wish I was better"
    ]
  }
```

Having this transformation applied

```json
  {
    "responseType": "survey",
    "{{ each(answer IN answers) AS answers }}": {
      "text": "{{ question }}",
      "answer": "{{ answer }}"
    }
  }
```

Would result in the following output

```json
  {
    "responseType": "survey",
    "answers": [{
      "text": "How are you doing?",
      "answer": "I'm doing pretty well"
    }, {
      "text": "How are you doing?",
      "answer": "I wish I was better"
    }]
  }
```

## Going Forward

This is very much version 1.0 of the project and would benefit from real world applications and suggestions.
