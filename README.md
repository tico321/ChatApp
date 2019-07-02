# Chat Application

## Pre-Requisites
* .net core 2.2 sdk
* RabbitMQ
 * If you already have an instance you need to update the RabbitMQ configuration in the following config files:
   * Web/appsettings.json
   * Bot/appsettings.json
 * if you don't and you have docker you can run the following command:
    ```
    $ docker run -d --hostname local-rabbit --name rabbitmq3 -p 5672:5672 -p 15672:15672 rabbitmq:3-management
    ```

## Running the application
The simplest way to run the application is to open to terminals.
In the first one go to the Web project and run the application:
```
    $ cd Web
    $ dotnet run
```
In the second one go to the Bot project and run the application:
```
    $ cd Bot
    $ dotnet run
```

## Features Implemented
* Allow registered users to log in and talk with other users in a chatroom.
* Allow users to post messages as commands into the chatroom with the following format
/stock=stock_code
* Create a decoupled bot that will call an API using the stock_code as a parameter
(https://stooq.com/q/l/?s=aapl.us&f=sd2t2ohlcv&h&e=csv, here aapl.us is the
stock_code)
* The bot should parse the received CSV file and then it should send a message back into
the chatroom using a message broker like RabbitMQ. The message will be a stock quote
using the following format: “APPL.US quote is $93.42 per share”. The post owner will be
the bot.
* Have the chat messages ordered by their timestamps and show only the last 50
messages.
* Unit test the functionality you prefer.

## Bonus Features Implemented
* Use .NET identity for users authentication
* Partially implemented
  * Handle messages that are not understood or any exceptions raised within the bot.

## Notes on the implementation
The architecture is based on the Clean Architecture model of microsoft and vertical slice architecture.
Here are some references to these architectures:
* [Microsoft Clean Architecture](https://github.com/dotnet-architecture/eShopOnContainers)
* [Vertical Slice Architecture](https://jimmybogard.com/vertical-slice-architecture/)
* [Commands and Queries Clean Architecture](https://github.com/JasonGT/NorthwindTraders)

The project represents a simplified version of the architecture models mentioned above. The idea is to have all the
domain logic inside the ApplicationCore project (DDD). External implementations are abstracted as interfaces and
implemented in the Infrastructure project.

Given that most of the logic is implemented inside the ApplicationCore
project, this project was the focus of unit testing. In order to test the commands and queries an InMemory database
provider is being used along with FakeItEasy to mock external dependencies.

There is no repository as for this project I'm enforcing the use of EF, the reason for this is that I consider EF is
already the abstraction I need for the data access layer. The project uses just an **InMemory database** (so all the
data is going to be lost whenever the Web service is restarted) but it could be easily switched to any other provider
of our convinience.

The project is using Razor pages for convinience given that it already has a template with authentication that uses
Identity. Note that with more time I would have been inclined to use React for the UI and maybe a third party auth
provider like Auth0 or Identity3.

There is a global error filter which help us returning [error details](https://tools.ietf.org/html/rfc7807) from the API,
And all the commands have validators (Fluent Validation) which validate the fields are valid, so no invalid messages are
processed as they are directly rejected. (Validators are invoked automatically by mediatr pipelines)
Validation can be easily tested by sending an empty command or a command with more than 100 chars.

## Wanted Features
* With more time I would added a docker compose file so RabbitMQ along with the services could be easily started, maybe
I would also have included a persisting database provider along with this.
* Setting up a consumer for RabbitMQ inside the Web project was not easy and I had to use a workaround. This is something
I would like to improve.
* Errors are being handled and the project has validations that can easily be applied, but better error handling for
the integration between Web and Bot is missing.
* More appealing error messages on UI.
* For the moment everytime there are new comments the first top 50 are retrieved from UI. We already have SignalR working
so it would be better to retrieve just the new message.
* Although Authorization is required to send requests to the Web project, the Bot doesn't have any kind of authorization,
so it would be interesting to add a Client Credentials Grant flow for the communication between Web and Bot.


