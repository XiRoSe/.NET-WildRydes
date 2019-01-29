# .NET AWS WildRydes version.

 A translation of the AWS guide lambda from node.js to .NET.
 
 Link to the guide - [WildRydes AWS Guide](https://aws.amazon.com/getting-started/projects/build-serverless-web-app-lambda-apigateway-s3-dynamodb-cognito/)
  
## Key Features & Tech

 - Using AWS APIGateway.
 - Lambda functions access with .NET.
 - Using DynamoDB access.
 - Using OOP for simplicity. 
 - Using 'OpenWeatherMap' API.
 - Using CloudWatch logger service.

## Needed installations of SDK, Templates & Libreries for the Project

 - AWS SDK and templates for AWS Lambda in visual studio.
 - DynamoDB installation from Nuget.
 - Newtonsoft.Json installation from Nuget.
 - Amazon.Lambda.Core (in case you won't allready have it) from Nuget.
 
## How to actually make it run

 - Follow the guide through steps 1,2 & 3, when you get to step 4 replace the Node.js lambda with this one by cloning it and uploading it to the lambda section (don't forget to select the .NET framework lambda).
 - After that just continue untill the end of the guide and see the magic in action.
 
## Pictures & Videos

#### Landing Page
<img src="https://cdn-images-1.medium.com/max/1100/0*7xXkQgluLZvDE-TO.jpg"  width="500" height="500">

#### Preview Video (Youtube link)
[<img src="https://i.ytimg.com/an_webp/UJxog8pDydU/mqdefault_6s.webp?du=3000&sqp=CO_BweIF&rs=AOn4CLAyWupiLjzEM3eaP65QS-gS53_IOg"  width="500" height="500">](https://www.youtube.com/watch?v=UJxog8pDydU)

#### For conclusion
I hope You'll like what iv'e wrote, I also added DynamoDB writing by the Document Model and by attributeValue inside the Function.cs in comments.

 #### This Lambda .NET function is free to use and made in purpose to help those who want to learn how to use Lambda functions with .NET.
