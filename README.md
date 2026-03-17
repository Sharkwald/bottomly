# bottomly

[![Build & Test](https://github.com/Sharkwald/bottomly/actions/workflows/dotnet.yml/badge.svg)](https://github.com/Sharkwald/bottomly/actions/workflows/dotnet.yml)

A .NET Slack bot

## Running Bottomly

### Development

The project uses [.NET Aspire](https://learn.microsoft.com/en-us/dotnet/aspire/get-started/aspire-overview) for local orchestration. Running the `Bottomly.AppHost` project will spin up MongoDB, Ollama (LLM), and the bot itself:

```bash
dotnet run --project Bottomly.AppHost
```

### Production

Run the `Bottomly` project directly, ensuring MongoDB and Ollama are available and all required environment variables are set:

```bash
dotnet run --project Bottomly
```

## Design

The solution is divided into four projects:

* **Bottomly** — the main application
* **Bottomly.AppHost** — .NET Aspire orchestration host for local development
* **Bottomly.ServiceDefaults** — shared Aspire configuration (telemetry, service discovery, HTTP resilience)
* **Bottomly.Tests** — xUnit test suite

Within the `Bottomly` project, code is organised by responsibility:

### Commands

Classes here implement the actions triggered by bot commands. Each command performs an action and returns a result, delegating any persistence work to the `Repositories` layer.

### Repositories

Defines the persistence layer. All behaviours which relate to persisting information beyond a simple request/response cycle are defined here, e.g. user karma and member lists. Backed by MongoDB.

### Slack

Implements the Slack delivery channel using [SlackNet](https://github.com/soxtoby/SlackNet) in Socket Mode.

`SlackWorker` is a background service that maintains the Slack connection and dispatches incoming events. Event handlers are split by type:

* `MessageEventHandlers/` — handle text messages. Each handler implements `IMessageEventHandler`, which requires:
  * `CanHandle(message)`: returns `true` if this handler should process the given message.
  * `Handle(message)`: extracts relevant information and invokes the corresponding command.
* `ReactionHandlers/` — handle emoji reactions (e.g. karma changes).
* `MembershipEventHandlers/` — handle member join events.

### LlmBot

Wraps [OllamaSharp](https://github.com/awaescher/OllamaSharp) to provide conversational AI responses via a locally-hosted Ollama instance.

### Tests

Has an internal structure matching the rest of the app. Each app file should have a corresponding test file.

## Configuration

The following secrets/environment variables _must_ be configured for the app to run:

* `bottomly_env`: Describes the active environment. If not set to `live`, output messages will be marked as "DEBUG".
* `bottomly_prefix`: The prefix for bot commands (e.g. `!`). Most commands require this prefix, allowing easy switching between test and production environments.
* `bottomly_slack_bot_token`: The Slack bot token (`xoxb-...`) for Socket Mode access.
* `bottomly_slack_app_token`: The Slack app-level token (`xapp-...`) for Socket Mode.
* `bottomly_google_api_key`: A valid Google API key.
* `bottomly_google_cse_id`: A valid Google Custom Search Engine ID.
* `bottomly_giphy_api_key`: A valid Giphy API key.
* `bottomly_github_token`: A GitHub personal access token.

MongoDB and Ollama connection strings are managed automatically by Aspire in development. In production, configure them via standard .NET connection string settings.

## Contributing

Most additions will involve adding a handler and command pair in the relevant modules, with the command delegating to the repository layer for any persistence work. PRs will *not be merged* without proper test coverage.
