# Facial-Recognition App

A simple facial recognition app that utilizes Microsoft Cognitive Services' Face API

## Microsoft Cognitive Services
Formerly known as Project Oxford. They are a set of machine-learning application programming interfaces (REST APIs), SDKs and services that helps developers to make smarter application by add intelligent features – such as emotion and video detection; facial, speech and vision recognition; and speech and language understanding.

There are four main components:

Face recognition: recognizes faces in photos, groups faces that look alike and verifies whether two faces are the same,
Speech processing: recognize speech and translate it into text, and vice versa,
Visual tools: analyze visual content to look for things like inappropriate content or a dominant color scheme, and
Language Understanding Intelligent Service (LUIS): understand what users mean when they say or type something using natural, everyday language.

Face recognition API is implemented in this application. So what is Face API?

### Face API
It is a cloud-based service that provides the most advanced face algorithms to detect and recognize human faces in images.

Face API has:

> Face Detection.
> Face Verification.
> Similar Face Searching.
> Face Grouping.
> Face Identification.

#### How to set up the application

Sign up to get an API(Authorization) key at this link https://www.microsoft.com/cognitive-services/en-us/sign-up.
After successfully joining it will redirect to subscriptions page. 

Click on Request new trials > Face – Preview > Agree Terms > Subscribe

In the Keys column from Key 1 click on “Show” to preview the API Key, click “Copy” to copy the key for further use.
Key can be regenerate by clicking on “Regenerate”.

Open the solution and navigate to the Web.config file.
Under the section `<appsettings></appsettings>` replace the value of `<add key="FaceServiceKey" value="xxxxxxxxxxxxxxxxxxxxxxxxxxx" />` with the API key you just generated.

And you're set! Build and run the app.
