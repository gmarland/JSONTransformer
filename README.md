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
          "question": "How are you doing?",<br/>
          "answers": [<br/>
            "I'm doing pretty well",<br/>
            "I wish I was better"<br/>
          ]<br/>
        }
      </div>
    </td>
    <td width="33.33%" align="left" valign="top">
      <div><b>Transformation</b></div>
      {
        "responseType": "survey",<br/>
        "{{ each(answer IN answers) AS answers }}": {<br/>
          "text": "{{ question }}",<br/>
          "answer": "{{ answer }}"<br/>
        }<br/>
      }
    </td>
    <td width="33.33%" align="left" valign="top">
      <div><b>Result</b></div>
      {
        "responseType": "survey",<br/>
        "answers": [{<br/>
          "text": "How are you doing?",<br/>
          "answer": "I'm doing pretty well"<br/>
        }, {<br/>
          "text": "How are you doing?",<br/>
          "answer": "I wish I was better"<br/>
        }]<br/>
      }
    </td>
  </tr>
</table>

## Getting started

The main function requires 2 parameters, the source you want to translate from and the transformation you want to apply. Both of these are JTokens and therefore can be a JObject or JArray. If the source is a JObject the transformation will only be applied to that object. If the source is a JArray the transformation will be applied to each child object and then appended together for a returning JArray. 
