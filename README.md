# bottomly
[![Build Status](https://travis-ci.org/Sharkwald/bottomly.svg?branch=master)](https://travis-ci.org/Sharkwald/bottomly)

A python slack bot

## Running Bottomly

### Development

Simply executing `main.py` will allow Bottomly to run, connect to slack, and listen for events.

### Production

The `dockerfile` allows the easy creation of a simple docker image based on Alpine which will allow the app to be run from within a container and hosted in your container framework of choice.

## Design

Modules are broadly divided by responsibility. Files in the root are related to global config and application spin up, everyhing else is in a module.

### Commands

Classes in here implement the actions sent to the bot via the delivery channel(s). In most cases they will peform some action and return a result directly, without calling out to other modules, however, if access to the persistence layer is required, it should be done through the `model` module.

### Model

This defines the persistence layer of the bot. All behaviours which relate to persisting information beyond a simple request/response model should be defined here, e.g. describing user histories, their karma, different user lists etc.

### Slack_channel

Implements the slack delivery channel, which is currently the only delivery channel. In future, the potential exists to define other ones, such as IRC, without needing to re-implement the `command` or `model` modules.

Uses the `SlackSocket` implementation of Slack's RTM protocl.

`slack_event_handler.py` handles the broad event loop, and loops through the custom handlers to find one that can handle the given message event.

`*_event_handler.py` files implement custom handlers and implement `AbstractEventHandler`. This mandates that they implement the following functions:

* `can_handle(slack_event)`: registers that this handler for a particular slack event. Returns a boolean.
* `handle(slack_event)`: extracts the relevant information from the given `slack_event` and passes that to a corresponding `command` object for execution.
* `_get_command_symbol`: Returns the trigger character(s) which specify what messages this handler will call on, e.g. `.g` for a google search command (assuming the bot is configured to use `.` as a common prefix).

### Tests

Has an internal structure matching the rest of the app, and each app file should have a corresponding test file.

## Configuration

The following environment variables _must_ be configured for the app to run & all unit tests pass:

* `bottomly_env`: A string describing the active environment. If it is not set to `live` all output messages will display marked as "DEBUG".
* `bottomly_prefix`: The prefix for bottomly commands. While events can overload this, for example karma changes can trigger on `++` or `--` regardless of the common prefix, most commands should require a single character prefix such as `.` or `!`. This sets that prefix globally, allowing for easy switching between test and production, for example.
* `bottomly_google_api_key`: A valid google API key
* `bottomly_google_cse_id`: A valid google custom search engine ID
* `bottomly_mongo_conn_str`: A mongo DB connection string. Since we're using `pymodm` for data access, this connection string must include a DB name.
* `bottomly_slack_bot_token`: The "Bot User OAuth Access Token" from slack to allow access to their RTM endpoint.
* `bottomly_giphy_api_key`: a valid giphy API key.


## Contributing

Most actions will be carried out by adding a `handler`/`command` pair in the relevant modules, with the command handing off to the model layer for any work that requires persistence. PRs will *not be merged* without proper test coverage.

